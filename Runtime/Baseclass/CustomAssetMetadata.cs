using System.Diagnostics;
using UnityEngine;

/// <summary>Base class for all metadata</summary>
public abstract class CustomAssetMetadata : ScriptableObject 
{
    // Keep a reference to our original asset so we can hook them up the moment we find the asset    
	[HideInInspector, SerializeField] internal LazyLoadReference<UnityEngine.Object> reference;

	// These methods are to handle creating and destroying metadata in the editor and not leave the asset dangling
	[Conditional("UNITY_EDITOR")] private void OnEnable() { if (Application.isEditor && reference.isSet && !reference.isBroken) MetadataLookup.Register(reference, this); }    
    [Conditional("UNITY_EDITOR")] private void OnDisable() { if (Application.isEditor && reference.isSet && !reference.isBroken) MetadataLookup.Unregister(reference, this); }
} 