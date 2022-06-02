using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

internal static class MetadataLookup
{
    public const string kResourcePath = "AssetMetadata";
    public const string kAssetName = "list";

    readonly static Dictionary<UnityEngine.Object, List<CustomAssetMetadata>> table = new Dictionary<UnityEngine.Object, List<CustomAssetMetadata>>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IReadOnlyList<CustomAssetMetadata> GetAllMetadata(UnityEngine.Object asset)
    {
        Init();
        if (!table.TryGetValue(asset, out var metadataList))
            return null;
        return metadataList;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool GetAllMetadataOfType<Metadata>(UnityEngine.Object asset, List<Metadata> result)
        where Metadata : CustomAssetMetadata
    {
        result.Clear();
        if (asset == null)
            return false;

            Init();

        if (!table.TryGetValue(asset, out var metadataList))
            return false;

        for (int i = 0; i < metadataList.Count; i++)
        {
            if (metadataList[i] is Metadata metadata)
                result.Add(metadata);
        }
        return result.Count > 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Metadata GetMetadataOfType<Metadata>(UnityEngine.Object asset)
        where Metadata : CustomAssetMetadata
    {
        if (asset == null)
            return null;

        Init();

        if (!table.TryGetValue(asset, out var metadataList))
            return null;

        for (int i = 0; i < metadataList.Count; i++)
        {
            if (metadataList[i] is Metadata metadata)
                return metadata;
        }
        return null;
    }



    internal static void Register(UnityEngine.Object asset, CustomAssetMetadata metadata)
    {
        if (object.ReferenceEquals(asset, null) ||
            object.ReferenceEquals(metadata, null))
            return;

        if (!table.TryGetValue(asset, out var metadataList))
        {
            metadataList = new List<CustomAssetMetadata>();
            table[asset] = metadataList;
        }

        if (!metadataList.Contains(metadata))
            metadataList.Add(metadata);
    } 

    internal static void Unregister(UnityEngine.Object asset, CustomAssetMetadata metadata)
    {
        if (object.ReferenceEquals(asset, null) ||
            object.ReferenceEquals(metadata, null))
            return;

        if (!table.TryGetValue(asset, out var metadataList))
            return;

        if (asset == null)
        {
            table.Remove(asset);
            return;
        }

        metadataList.Remove(metadata);
    }

    static IReadOnlyList<CustomAssetMetadata> GetAllMetadata<Asset>()
        where Asset : UnityEngine.Object
    {
        var result = new List<CustomAssetMetadata>();
#if UNITY_EDITOR
        // TODO: figure out if there's a more efficient way of doing this??
        var guids = UnityEditor.AssetDatabase.FindAssets("t:" + typeof(Asset).Name);
        for (int i = 0; i < guids.Length; i++)
        {
            var assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[i]);            
            if (assetPath.EndsWith(".unity")) // Cannot handle assets that are stored within a scene file
                continue;

            foreach (var item in UnityEditor.AssetDatabase.LoadAllAssetsAtPath(assetPath))
            {
                if (item is CustomAssetMetadata metadata && metadata.asset is Asset)
                {
                    result.Add(metadata);
                }
            }
        }
#else
        var lookupAsset = Resources.Load($"{kResourcePath}/{kAssetName}") as MetadataLookupAsset;
        foreach(var item in lookupAsset.allMetadata)
        {
            if (item is CustomAssetMetadata metadata && metadata.asset is Asset)
            {
                result.Add(metadata);
            }
        }
#endif
        return result;
    }

    static bool initialized = false;

    static void Init()
    {
        if (initialized)
            return;

        // TODO: use addressables (if available?) instead to avoid loading every asset into memory
        foreach (var metadata in GetAllMetadata<Material>()) // TODO: figure out a way to efficiently support more types
            MetadataLookup.Register(metadata.asset, metadata);

        initialized = true;
    }
}