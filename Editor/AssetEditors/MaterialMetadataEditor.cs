using System;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Material)), CanEditMultipleObjects]
public class MaterialMetadataEditor : MaterialEditor
{
    MetadataEditor metadataEditor;

    public override void OnEnable()
    {
        base.OnEnable();
        metadataEditor = new MetadataEditor(targets);
    }

    public override void OnDisable()
    {
        base.OnDisable();
        metadataEditor.Dispose();
        metadataEditor = null;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        metadataEditor.OnInspectorGUI();
    }
}