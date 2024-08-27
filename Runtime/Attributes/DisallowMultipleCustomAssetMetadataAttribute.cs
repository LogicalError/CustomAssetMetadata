/// <summary> Prevents CustomAssetMetadata of same type (or subtype) to be added more than once to an Asset. </summary>
[System.AttributeUsage(System.AttributeTargets.Class, Inherited = false)]
public sealed class DisallowMultipleCustomAssetMetadataAttribute : System.Attribute
{
}