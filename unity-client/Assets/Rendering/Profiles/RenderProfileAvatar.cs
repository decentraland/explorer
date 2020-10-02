using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Helpers;
using UnityEngine;

[CreateAssetMenu(menuName = "DCL/Rendering/Create RenderProfileAvatar", fileName = "RenderProfileAvatar", order = 0)]
public class RenderProfileAvatar : ScriptableObject
{
    [System.Serializable]
    public class MaterialProfile
    {
        [SerializeField] private Vector3 lightDirection;

        public void Apply()
        {
            Debug.Log("Setting light direction to " + lightDirection);
            Shader.SetGlobalVector(ShaderUtils._LightDir, lightDirection);
        }
    }

    [SerializeField] public MaterialProfile avatarEditorProfile;
    [SerializeField] public MaterialProfile inWorldProfile;
}