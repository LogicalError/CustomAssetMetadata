using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.Rendering;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public static class AssetMetadataUtility
{
    static IList<Type> allCustomAssetMetadataTypes;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static void Initialize()
    {
        if (allCustomAssetMetadataTypes != null)
            return;

        allCustomAssetMetadataTypes = TypeCache.GetTypesDerivedFrom<CustomAssetMetadata>();
    }

    public static bool HaveMetadataTypes
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            Initialize();
            return AllMetadataTypes.Count > 0;
        }
    }

    public static IList<Type> AllMetadataTypes
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            Initialize();
            return allCustomAssetMetadataTypes;
        }
    }

    public static void GetAll(UnityEngine.Object target, List<CustomAssetMetadata> metadata)
    {
        var assetPath = AssetDatabase.GetAssetPath(target);
        if (assetPath == null ||
            string.IsNullOrEmpty(assetPath))
            return;

        var assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
        foreach (var asset in assets)
        {
            if (asset is CustomAssetMetadata additionalDataAsset)
            {
                metadata.Add(additionalDataAsset);
            }
        }
    }

    public static CustomAssetMetadata Add(UnityEngine.Object target, Type type)
    {
        var assetPath = AssetDatabase.GetAssetPath(target);
        if (assetPath == null)
            return null;
        
        // TODO: could try to make non unity assets work by putting a file next to it?

        var instance = ScriptableObject.CreateInstance(type);
        instance.hideFlags = HideFlags.HideInHierarchy;
        if (instance is CustomAssetMetadata assetMetadata)
        {
            assetMetadata.name = type.Name;
            assetMetadata.asset = target;
            AssetDatabase.AddObjectToAsset(assetMetadata, target);
            AssetDatabase.SetMainObject(target, assetPath);
            AssetDatabase.ImportAsset(assetPath);
            AssetDatabase.Refresh();
            return assetMetadata;
        }

        UnityEngine.Object.DestroyImmediate(instance);
        return null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static CustomAssetMetadata Add<Metadata>(UnityEngine.Object target)
        where Metadata : CustomAssetMetadata
    {
        return Add(target, typeof(Metadata));
    }

    public static void Destroy<Metadata>(Metadata metadata)
        where Metadata : CustomAssetMetadata
    {
        var assetPath = AssetDatabase.GetAssetPath(metadata);
        if (assetPath == null)
            return;

        AssetDatabase.RemoveObjectFromAsset(metadata);
        UnityEngine.Object.DestroyImmediate(metadata); // we need to destroy it otherwise it'll be saved to the scene
        AssetDatabase.ImportAsset(assetPath);
        AssetDatabase.Refresh();
    }
}
