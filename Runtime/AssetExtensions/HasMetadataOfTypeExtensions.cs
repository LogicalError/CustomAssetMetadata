using System.Runtime.CompilerServices;
using UnityEngine;

public static class HasMetadataOfTypeExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasMetadataOfType<Metadata>(this Material asset)
        where Metadata : CustomAssetMetadata
    {
        return MetadataLookup.HasMetadataOfType<Metadata>(asset);
    }
}
