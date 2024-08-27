using System;

/// <summary> Restricts a CustomAssetMetadata to only be allowed to be added to certain asset types. </summary>
[System.AttributeUsage(System.AttributeTargets.Class, Inherited = false)]
public sealed class RestrictMetadataAssetTypesAttribute : System.Attribute
{
	private readonly Type[] m_Types;

	/// <summary> The types of assets we are allowed to add our CustomAssetMetadata to. </summary>
	public Type[] Types => m_Types;

	/// <summary> Restrict the asset types we're allowed to add our CustomAssetMetadata to. </summary>
	/// <param name="types"> The types of assets we are allowed to add our CustomAssetMetadata to. </param>
	public RestrictMetadataAssetTypesAttribute(params Type[] types)
	{
		m_Types = types;
	}
}