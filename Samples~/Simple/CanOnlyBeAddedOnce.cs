[AddMetadataMenu("Group1/Can only be addedOnce")]
[DisallowMultipleCustomAssetMetadata]
[RestrictMetadataAssetTypes(typeof(UnityEngine.Material))]
public class CanOnlyBeAddedOnce : CustomAssetMetadata
{
    public string myText;
}
