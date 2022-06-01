using System;
using System.Collections.Generic;
using UnityEngine;

public class TestComponent : MonoBehaviour
{
    public Material material;
    
    void Start()
    {
        var textMesh = GetComponent<TextMesh>();
        var testMetaData1 = material.GetMetadataOfType<TestMetadata1>();
        if (testMetaData1 != null)
        {
            textMesh.text = testMetaData1.myText;
        } else
        {
            textMesh.text = $"Could not find {nameof(TestMetadata1)}";
        }
    }
}
