using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Helpers;
using UnityEngine;
using UnityEngine.Serialization;

namespace DCL
{
    [CreateAssetMenu(menuName = "DCL/Rendering/Create Avatar Profile", fileName = "RenderProfileAvatar", order = 0)]
    public class RenderProfileAvatar : ScriptableObject
    {
        [System.Serializable]
        public class MaterialProfile
        {
            [SerializeField] private Color lightColor;
            [SerializeField] private Vector3 lightDirection;

            public void Apply()
            {
                Shader.SetGlobalVector(ShaderUtils._LightDir, lightDirection);
                Shader.SetGlobalColor(ShaderUtils._LightColor, lightColor);
            }
        }

        [FormerlySerializedAs("avatarEditorProfile")] [SerializeField]
        public MaterialProfile avatarEditor;

        [FormerlySerializedAs("inWorldProfile")] [SerializeField]
        public MaterialProfile inWorld;
    }
}