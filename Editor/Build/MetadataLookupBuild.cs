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
		var allMetadata = Resources.FindObjectsOfTypeAll<CustomAssetMetadata>();
        int index = 0;
        for (int i = 0; i < allMetadata.Length; i++)
		{
			var clone = UnityEngine.Object.Instantiate(allMetadata[i]);
			var name = $"{allMetadata[i].GetInstanceID()}-{index}"; index++;
			AssetDatabase.CreateAsset(clone, $"{basePath}/{name}.asset");
            allMetadata[i] = clone;
        }
        list.allMetadata = allMetadata;
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