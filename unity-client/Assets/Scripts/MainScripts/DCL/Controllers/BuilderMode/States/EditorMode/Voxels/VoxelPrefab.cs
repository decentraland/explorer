using DCL.Helpers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelPrefab : MonoBehaviour
{
    public Material editMaterial, errorMaterial;
    public Renderer meshRenderer;

    bool isAvailable = true;
    public void SetAvailability(bool _isAvailable)
    {
        if (_isAvailable)
        {
            if (meshRenderer.material != editMaterial) meshRenderer.material = editMaterial;
        }
        else
        {
            if (meshRenderer.material != errorMaterial) meshRenderer.material = errorMaterial;
        }
        isAvailable = _isAvailable;
    }

    public bool IsAvailable()
    {
        return isAvailable;
    }
}
