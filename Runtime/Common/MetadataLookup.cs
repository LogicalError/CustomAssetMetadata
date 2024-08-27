
#if !UNITY_EDITOR
#define UNITY_RUNTIME
#endif
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

internal static class MetadataLookup
{
    public const string kResourcePath = "AssetMetadata";
    public const string kAssetName = "list.asset";

    readonly static Dictionary<int, List<CustomAssetMetadata>> table = new();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IReadOnlyList<CustomAssetMetadata> GetAllMetadata(UnityEngine.Object asset)
    {
        if (!GetAllMetadataForAsset(asset, out var metadataList))
            return null;
        return metadataList;
    }

    public static bool GetAllMetadataOfType<Metadata>(UnityEngine.Object asset, List<Metadata> result)
        where Metadata : CustomAssetMetadata
    {
        result.Clear();
        if (asset == null)
            return false;

        if (!GetAllMetadataForAsset(asset, out var metadataList))
            return false;

        for (int i = 0; i < metadataList.Count; i++)
        {
            if (metadataList[i] is Metadata metadata)
                result.Add(metadata);
        }
        return result.Count > 0;
    }

	public static bool HasMetadataOfType(UnityEngine.Object asset, System.Type type)
	{
		if (asset == null)
			return false;

		if (!GetAllMetadataForAsset(asset, out var metadataList))
			return false;

		for (int i = 0; i < metadataList.Count; i++)
		{
			if (metadataList[i] != null &&
				metadataList[i].GetType() == type)
				return true;
		}
		return false;
	}

	public static bool HasMetadataOfType<Metadata>(UnityEngine.Object asset)
		where Metadata : CustomAssetMetadata
	{
		if (asset == null)
			return false;

		if (!GetAllMetadataForAsset(asset, out var metadataList))
			return false;

		for (int i = 0; i < metadataList.Count; i++)
		{
			if (metadataList[i] is Metadata)
				return true;
		}
		return false;
	}

    public static Metadata GetMetadataOfType<Metadata>(UnityEngine.Object asset)
        where Metadata : CustomAssetMetadata
    {
        if (asset == null)
            return null;

        if (!GetAllMetadataForAsset(asset, out var metadataList))
            return null;

        for (int i = 0; i < metadataList.Count; i++)
        {
            if (metadataList[i] is Metadata metadata)
                return metadata;
        }
        return null;
    }

	internal static bool Register(UnityEngine.LazyLoadReference<UnityEngine.Object> reference, CustomAssetMetadata metadata)
	{
		if (reference.isBroken || !reference.isSet ||
			object.ReferenceEquals(metadata, null))
			return false;

		if (!table.TryGetValue(reference.instanceID, out var metadataList))
		{
			metadataList = new List<CustomAssetMetadata>();
			table[reference.instanceID] = metadataList;
		}

		if (!metadataList.Contains(metadata))
			metadataList.Add(metadata);
		return true;
	}

	internal static void Unregister(UnityEngine.LazyLoadReference<UnityEngine.Object> reference, CustomAssetMetadata metadata)
	{
		if (reference.isBroken || !reference.isSet ||
			object.ReferenceEquals(metadata, null))
			return;

		if (!table.TryGetValue(reference.instanceID, out var metadataList))
			return;

		metadataList.Remove(metadata);
        if (metadataList.Count == 0)
        {
            table.Remove(reference.instanceID);
        }
	}

#if UNITY_EDITOR
	static bool RegisterMetadataForAsset(string assetPath)
	{
		if (Application.isEditor && string.IsNullOrEmpty(assetPath))
			return false;

		bool foundAny = false;
		foreach (var item in UnityEditor.AssetDatabase.LoadAllAssetsAtPath(assetPath))
		{
			if (item is not CustomAssetMetadata metadata)
				continue;

			foundAny = MetadataLookup.Register(metadata.reference.asset, metadata) || foundAny;
		}
		return foundAny;
	}
#endif

	static bool GetAllMetadataForAsset(UnityEngine.Object asset, out List<CustomAssetMetadata> result)
    {
        EnsureInitialized();
        if (table.TryGetValue(asset.GetInstanceID(), out result))
            return true;
#if UNITY_EDITOR
        if (Application.isEditor)
        {
            var assetPath = UnityEditor.AssetDatabase.GetAssetPath(asset);
            RegisterMetadataForAsset(assetPath);
            if (table.TryGetValue(asset.GetInstanceID(), out result))
                return true;
        }
#endif
        result = null;
        return false;
    }

    [System.Diagnostics.Conditional("UNITY_RUNTIME")]
    static void InitializeRuntime()
	{
#if UNITY_EDITOR
		if (!Application.isEditor)
#endif
		{
            var assetpath = $"{kResourcePath}/{kAssetName}";
			var lookupAsset = Resources.Load(assetpath) as MetadataLookupAsset;
            if (lookupAsset == null)
			{
				Debug.LogError($"Failed to load {assetpath}");
                return;
			}

			foreach (var metadata in lookupAsset.allMetadata)
			{
				if (metadata != null)
					continue;
				MetadataLookup.Register(metadata.reference, metadata);
            }
        }
    }

	[System.Diagnostics.Conditional("UNITY_EDITOR")]
	static void InitializeEditor()
	{
#if UNITY_EDITOR
        if (Application.isEditor)
		{
			var allMetadata = Resources.FindObjectsOfTypeAll<CustomAssetMetadata>();
			foreach (var metadata in allMetadata)
			{
				if (metadata != null)
					continue;
				var assetPath = UnityEditor.AssetDatabase.GetAssetPath(metadata);
				RegisterMetadataForAsset(assetPath);
			}
        }
#endif
    }

    static bool s_Initialized = false;
    static void EnsureInitialized()
    {
        if (s_Initialized) 
            return;
        s_Initialized = true;
        InitializeRuntime();
        InitializeEditor();
	}
}