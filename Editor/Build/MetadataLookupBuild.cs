using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

// This is a bit of a hack, but we need to ensure that our metadata is not stripped on build,
// so at build time we clone them into a temporary resources directory. Afterwards we delete the directory.
// Cloning works since each metadata object holds a reference to the thing it belongs to and we can
// just use the clone at runtime instead.
public class MetadataLookupPreprocessBuild : IPreprocessBuildWithReport
{
    public int callbackOrder { get { return 0; } }

    public static List<(string, CustomAssetMetadata)> GetAllMetadata<Asset>()
        where Asset : UnityEngine.Object
    {
        // TODO: figure out if there's a more efficient way of doing this??
        var guids = AssetDatabase.FindAssets($"t:{typeof(Asset).Name}");
        var result = new List<(string, CustomAssetMetadata)>();
        for (int i = 0; i < guids.Length; i++)
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);            
            // Sadly any asset included in a scene file cannot be loaded
            if (assetPath.EndsWith(".unity"))
                continue;

            int index = 0;
            foreach (var item in AssetDatabase.LoadAllAssetsAtPath(assetPath))
            {
                if (item is CustomAssetMetadata metadata && metadata.asset is Asset)
                {
                    var name = $"{guids[i]}-{index}";
                    result.Add((name, metadata));
                    index++;
                }
            }
        }
        return result;
    }

    public static bool HadResourcesDirectory = false;

    public void OnPreprocessBuild(BuildReport report)
    {
        const string basePath = "Assets/Resources/" + MetadataLookup.kResourcePath;
        AssetDatabase.StartAssetEditing();
        HadResourcesDirectory = System.IO.Directory.Exists("Assets/Resources/");
        if (System.IO.Directory.Exists(basePath))
            System.IO.Directory.Delete(basePath, true);
        System.IO.Directory.CreateDirectory(basePath);
        var list = ScriptableObject.CreateInstance<MetadataLookupAsset>();
        var allMetadata = new List<CustomAssetMetadata>();
        // TODO: use addressables (if available?) instead to avoid loading every asset into memory
        foreach (var (name, metadata) in GetAllMetadata<Material>()) // TODO: figure out a way to efficiently support more types
        {
            var clone = UnityEngine.Object.Instantiate(metadata);
            AssetDatabase.CreateAsset(clone, $"{basePath}/{name}.asset");
            allMetadata.Add(clone);
        }
        list.allMetadata = allMetadata.ToArray();
        AssetDatabase.CreateAsset(list, $"{basePath}/{MetadataLookup.kAssetName}.asset");
        AssetDatabase.StopAssetEditing();
    }

}


public class MetadataLookupPostprocessBuild : IPostprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPostprocessBuild(BuildReport report)
    {
        AssetDatabase.StartAssetEditing();
        if (!MetadataLookupPreprocessBuild.HadResourcesDirectory)
        {
            const string basePath = "Assets/Resources";
            if (System.IO.Directory.Exists(basePath))
                System.IO.Directory.Delete(basePath, true);

            const string metaFile = basePath + ".meta";
            if (System.IO.File.Exists(metaFile))
                System.IO.File.Delete(metaFile);
        } else
        {
            const string basePath = "Assets/Resources/" + MetadataLookup.kResourcePath;
            if (System.IO.Directory.Exists(basePath))
                System.IO.Directory.Delete(basePath, true);

            const string metaFile = basePath + ".meta";
            if (System.IO.File.Exists(metaFile))
                System.IO.File.Delete(metaFile);
        }
        AssetDatabase.StopAssetEditing();
    }
}