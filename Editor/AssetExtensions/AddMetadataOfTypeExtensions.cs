﻿using System.Runtime.CompilerServices;
using UnityEngine;

public static class AddMetadataOfTypeExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Metadata AddMetadataOfType<Metadata>(this Material asset)
        where Metadata : CustomAssetMetadata
    {
        return AssetMetadataUtility.Add<Metadata>(asset) as Metadata;
    }
}
