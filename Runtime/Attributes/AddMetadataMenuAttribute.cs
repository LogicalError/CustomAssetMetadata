
/// <summary> The AddMetadataMenuAttribute allows you to place a CustomAssetMetadata anywhere in the "Metadata" menu. </summary>
[System.AttributeUsage(System.AttributeTargets.Class, Inherited = false)]
public sealed class AddMetadataMenuAttribute : System.Attribute
{
	private readonly string m_AddMetadataMenu;
	private readonly int m_Ordering;

	public string MetadataMenu => m_AddMetadataMenu;

	/// <summary> The order of the metadata in the metadata menu (lower values appear higher in the menu). </summary>
	public int MetadataOrder => m_Ordering;

	/// <summary> Add an item in the Metadata menu. </summary>
	/// <param name="menuName"> The path to the metadata. </param>
	public AddMetadataMenuAttribute(string menuName)
	{
		m_AddMetadataMenu = menuName;
		m_Ordering = 0;
	}

	/// <summary> Add an item in the Metadata menu. </summary>
	/// <param name="menuName"> The path to the metadata. </param>
	/// <param name="order"> Where in the metadata menu to add the new item. </param>
	public AddMetadataMenuAttribute(string menuName, int order)
	{
		m_AddMetadataMenu = menuName;
		m_Ordering = order;
	}
}