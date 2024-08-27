using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

public static class GetMetadataOfTypeExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Metadata GetMetadataOfType<Metadata>(this Material asset)
        where Metadata : CustomAssetMetadata
    {
        return MetadataLookup.GetMetadataOfType<Metadata>(asset);
    }


}
