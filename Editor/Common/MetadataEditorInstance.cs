using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// An Editor for a metadata instance that's currently shown in the inspector
/// Holds a "metadata" for each selected "target" in the inspector
/// </summary>
sealed class MetadataEditorInstance : IDisposable
{
    string                  metadataKeyName;
    GUIContent              metadataContent;
    UnityEngine.Object[]    targets;
    bool                    showMetadata;
    CustomAssetMetadata[]   metadata;
    Editor                  metaDataEditor;
    bool                    isGenericInspector;
    SerializedObject        serializedObject;

    private MetadataEditorInstance() { }

    public static MetadataEditorInstance Create(UnityEngine.Object[] targets, CustomAssetMetadata[] metadata, Type type)
    {
        var metadataName = ObjectNames.NicifyVariableName(type.Name);
        var metadataKeyName = type.FullName;
        return new MetadataEditorInstance
        {
            targets         = targets,
            metadata        = metadata,
            metadataKeyName = metadataKeyName,
            metadataContent = new GUIContent(metadataName),
            showMetadata    = EditorPrefs.GetBool(metadataKeyName, false)
        };
    }

    public void Dispose()
    {
        EditorPrefs.SetBool(metadataKeyName, showMetadata);
        metadata = null;
        if (metaDataEditor != null)
            UnityEngine.Object.DestroyImmediate(metaDataEditor);
        metaDataEditor = null;
    }

    public bool destroyed { get { return metadata == null; } }

    public bool mixedValues
    {
        get
        {
            if (metadata == null ||
                metadata.Length != targets.Length)
                return true;

            if (metadata.Length == 0)
                return false;

            foreach (var item in metadata)
            {
                if (item == null)
                    return true;
            }
            return false;
        }
    }

    void ShowMetadataEditor()
    {
        if (metaDataEditor == null)
        {
            metaDataEditor = Editor.CreateEditor(metadata);
            isGenericInspector = metaDataEditor.GetType().FullName == "UnityEditor.GenericInspector";
            if (isGenericInspector)
            {
                serializedObject = new SerializedObject(metadata);
            }
        }

        if (metaDataEditor != null)
        {
            if (isGenericInspector)
            {
                serializedObject.UpdateIfRequiredOrScript();
                SerializedProperty iterator = serializedObject.GetIterator();
                bool enterChildren = true;
                while (iterator.NextVisible(enterChildren))
                {
                    if (iterator.propertyPath == "m_Script")
                        continue;

                    EditorGUILayout.PropertyField(iterator, true);
                    enterChildren = false;
                }
                serializedObject.ApplyModifiedProperties();
            } else
                metaDataEditor.OnInspectorGUI();
        }
    }

    public void OnInspectorGUI()
    {
        if (targets == null || targets.Length == 0 || metadata == null || metadata.Length == 0)
            return;

        var rect = GUILayoutUtility.GetRect(GUIContent.none, InspectorLayout.inspectorTitlebar);
        showMetadata = InspectorLayout.FoldoutTitlebar(rect, showMetadata, metadataContent, false, InspectorLayout.inspectorTitlebar, InspectorLayout.inspectorTitlebarText);
        var controlId = InspectorLayout.lastControlId;
        if (showMetadata)
        {
            if (mixedValues)
            {
                // TODO: show proper message for mixed values
                EditorGUILayout.LabelField("...");
            } else
            {
                EditorGUILayout.Space(3);
                ShowMetadataEditor();
                EditorGUILayout.Space();
            }
        }

        var evt = Event.current;
        var type = evt.GetTypeForControl(controlId);
        switch (type)
        {
            case EventType.MouseUp:
            {
                //EditorGUIUtility.hotControl == controlId)
                if (evt.button == 1 && rect.Contains(evt.mousePosition))
                {
                    var menu = new GenericMenu();
                    menu.AddItem(new GUIContent("Remove"), false, DestroyMetadata);
                    menu.ShowAsContext();
                }
                break;
            }
        }
    }

    void DestroyMetadata()
    {
        if (metadata != null)
        {
            for (int i = 0; i < metadata.Length; i++)
            {
                AssetMetadataUtility.Destroy(metadata[i]);
            }
        }
        metadata = null;
    }
}


public sealed class MetadataEditor : IDisposable
{
    bool                        canOpenForEdit;
    UnityEngine.Object[]        targets;
    MetadataEditorInstance[]    metadataEditors;

    public MetadataEditor(UnityEngine.Object[] targets)
    {
        this.targets = targets;
        canOpenForEdit = true;
        foreach (var target in targets)
        {
            if (!AssetDatabase.CanOpenForEdit(target))
                canOpenForEdit = false;
        }
    }

    public void Dispose()
    {
        DestroyEditors(ref metadataEditors);
    }


    static void DestroyEditors(ref MetadataEditorInstance[] metadataEditors)
    {
        if (metadataEditors != null)
        {
            for (int i = 0; i < metadataEditors.Length; i++)
            {
                metadataEditors[i].Dispose();
            }
            metadataEditors = null;
        }
    }

    public void AddMetadata(Type type)
    {
        if (type == null || targets == null || targets.Length == 0)
            return;

        for (int i = 0; i < targets.Length; i++)
        {
            AssetMetadataUtility.Add(targets[i], type);
        }
    }

    static MetadataEditorInstance[] CreateEditors(UnityEngine.Object[] targets)
    {
        if (targets == null)
            return null;

        var types               = new List<Type>();
        var typeMetadata        = new List<List<CustomAssetMetadata>>();
        var temporaryMetadata   = new List<CustomAssetMetadata>();

        AssetMetadataUtility.GetAll(targets[0], temporaryMetadata);
        for (int i = 0; i < temporaryMetadata.Count; i++)
        {
            var metadataItem = temporaryMetadata[i];
            if (metadataItem == null)
                continue;
            types.Add(metadataItem.GetType());
            typeMetadata.Add(new List<CustomAssetMetadata>
            {
                metadataItem
            });
        }

        for (int i = 1; i < targets.Length; i++)
        {
            AssetMetadataUtility.GetAll(targets[i], temporaryMetadata);

            // TODO: do something clever to support metadata w/ multiple targets
        }

        var metadataEditors = new MetadataEditorInstance[typeMetadata.Count];
        for (int i = 0; i < typeMetadata.Count; i++)
        {
            metadataEditors[i] = MetadataEditorInstance.Create(targets, typeMetadata[i].ToArray(), types[i]);
        }

        return metadataEditors;
    }

    public void OnInspectorGUI()
    {
        using (new EditorGUI.DisabledScope(!canOpenForEdit))
        {
            if (metadataEditors == null)
                metadataEditors = CreateEditors(targets);
            var editors = new MetadataEditorInstance[metadataEditors.Length];
            if (targets == null || !AssetMetadataUtility.HaveMetadataTypes)
                return;


            // TODO: make it possible to re-order metadata like Components on GameObjects

            GUILayout.BeginVertical();
            if (metadataEditors != null)
            {
                // We seem to get the metadata in reverse order from the assetdatabase, compared to the order we add them
                // So to make things feel more consistent, we reverse the order
                for (int i = metadataEditors.Length - 1; i >= 0; i--)
                {
                    metadataEditors[i].OnInspectorGUI();
                }
            }

            EditorGUILayout.Space();

            using (new EditorGUI.DisabledScope(metadataEditors == null || !AssetMetadataUtility.HaveMetadataTypes))
            {
                if (GUILayout.Button("Add Metadata"))
                {
                    // TODO: have a nicer menu, more like the "add components" menu
                    // TODO: support putting attribute on metadata for name in menu
                    var menu = new GenericMenu();
                    foreach (var type in AssetMetadataUtility.AllMetadataTypes)
                    {
                        menu.AddItem(new GUIContent(ObjectNames.NicifyVariableName(type.Name)), false, delegate ()
                        {
                            AddMetadata(type);
                            DestroyEditors(ref metadataEditors);
                            metadataEditors = CreateEditors(targets);
                        });
                    }
                    menu.ShowAsContext();
                }
            }

            GUILayout.EndVertical();
        }
    }
}