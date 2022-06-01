using System.Diagnostics;
using UnityEngine;

/// <summary>Base class for all metadata</summary>
public abstract class CustomAssetMetadata : ScriptableObject 
{
    // Keep a reference to our original asset so we can hook them up the moment we find the asset
    // TODO: this should probably be an AssetReference (Addressables)
    [HideInInspector, SerializeField] internal UnityEngine.Object asset;

    // These methods are to handle creating and destroying metadata in the editor and not leave the asset dangling
    [Conditional("UNITY_EDITOR")] private void OnEnable() { if (asset != null) MetadataLookup.Register(asset, this); }    
    [Conditional("UNITY_EDITOR")] private void OnDisable() { MetadataLookup.Unregister(asset, this); }
} 