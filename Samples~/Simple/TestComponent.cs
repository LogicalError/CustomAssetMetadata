using UnityEngine;

public class TestComponent : MonoBehaviour
{
    public Material material;
    
    void Start()
    {
        var textMesh = GetComponent<TextMesh>();
        var CanOnlyBeAddedOnce = material.GetMetadataOfType<CanOnlyBeAddedOnce>();
        if (CanOnlyBeAddedOnce != null)
        {
            textMesh.text = CanOnlyBeAddedOnce.myText;
        } else
        {
            textMesh.text = $"Could not find {nameof(CanOnlyBeAddedOnce)}";
        }
    }
}
