﻿using System;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using UnityEditor.Rendering;

public static class InspectorLayout
{

    static GUIStyle m_TitlebarFoldout = null;
    static GUIStyle m_InspectorTitlebar = null;
    static GUIStyle m_InspectorTitlebarText = null;

    public static GUIStyle titlebarFoldout => (m_InspectorTitlebarText == null) ? GetStyle("Titlebar Foldout") : m_TitlebarFoldout;
    public static GUIStyle inspectorTitlebar => (m_InspectorTitlebarText == null) ? GetStyle("IN Title") : m_InspectorTitlebar;
    public static GUIStyle inspectorTitlebarText => (m_InspectorTitlebarText == null) ? GetStyle("IN TitleText") : m_InspectorTitlebarText;

    static GUIStyle GetStyle(string styleName)
    {
        GUIStyle gUIStyle = GUI.skin.FindStyle(styleName) ?? EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).FindStyle(styleName);
        if (gUIStyle == null)
        {
            Debug.LogError("Missing built-in guistyle " + styleName);
            gUIStyle = null;
        }

        return gUIStyle;
    }

    internal static Rect GetInspectorTitleBarObjectFoldoutRenderRect(Rect rect, GUIStyle baseStyle)
    {
        var offsetX = (float)titlebarFoldout.margin.left + 1f;
        var offsetY = (rect.height - 13f) / 2f + (float)(baseStyle?.padding.top ?? 0);

        return new Rect(rect.x + offsetX, rect.y + offsetY, rect.width - offsetX, 13f);
    }

    public static FieldInfo m_LastControlIdField = typeof(EditorGUIUtility).GetField("s_LastControlID", BindingFlags.Static | BindingFlags.NonPublic);
    public static int lastControlId
    {
        get
        {
            if (m_LastControlIdField == null)
            {
                Debug.LogError("Compatibility with Unity broke: can't find lastControlId field in EditorGUI");
                return 0;
            }
            return (int)m_LastControlIdField.GetValue(null);
        }
    }

    static readonly int s_TitlebarHash = "GenericTitlebar".GetHashCode();
    public static bool FoldoutTitlebar(Rect position, bool foldout, GUIContent label, bool skipIconSpacing, GUIStyle baseStyle, GUIStyle textStyle)
    {
        int controlID = GUIUtility.GetControlID(s_TitlebarHash, FocusType.Keyboard, position);
        var foldoutPosition = GetInspectorTitleBarObjectFoldoutRenderRect(position, baseStyle);
        position.width += position.x;
        position.x = 0;
        Rect position2 = new Rect(position.x + (float)baseStyle.padding.left + (float)((!skipIconSpacing) ? 20 : 0), position.y + (float)baseStyle.padding.top, EditorGUIUtility.labelWidth, 16f);
        bool hover = position.Contains(Event.current.mousePosition);
        if (Event.current.type == EventType.Repaint)
        {
            baseStyle.Draw(position, GUIContent.none, controlID, foldout, hover);
        }
        var expanded = EditorGUI.Foldout(foldoutPosition, foldout, GUIContent.none, true);//, showMetadata, isBoxed: false, null, null, String.Empty);
        if (expanded)
            EditorGUI.EndFoldoutHeaderGroup();
        //titlebarFoldout.Draw(GetInspectorTitleBarObjectFoldoutRenderRect(position, baseStyle), GUIContent.none, controlID, foldout, hover);
        if (Event.current.type == EventType.Repaint)
        {
            textStyle.Draw(position2, EditorGUIUtility.TrTempContent(label.text), controlID, foldout, hover);
        }
        return expanded;
    }

    public static bool FoldoutTitlebar(Rect position, bool foldout, GUIContent label, bool skipIconSpacing)
    {
        return FoldoutTitlebar(position, foldout, label, skipIconSpacing, inspectorTitlebar, inspectorTitlebarText);
    }

    public static bool FoldoutTitlebar(bool foldout, GUIContent label, bool skipIconSpacing)
    {
        return FoldoutTitlebar(foldout, label, skipIconSpacing, inspectorTitlebar, inspectorTitlebarText);
    }

    public static bool FoldoutTitlebar(bool foldout, GUIContent label, bool skipIconSpacing, GUIStyle baseStyle, GUIStyle textStyle)
    {
        var rect = GUILayoutUtility.GetRect(GUIContent.none, baseStyle);
        return FoldoutTitlebar(rect, foldout, label, skipIconSpacing, baseStyle, textStyle);
    }
}
