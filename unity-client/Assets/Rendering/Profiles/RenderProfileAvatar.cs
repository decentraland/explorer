using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Helpers;
using UnityEngine;
using UnityEngine.Serialization;

namespace DCL
{
    /// <summary>
    /// This ScriptableObject is used to control toon shader and avatar rendering values
    /// assigned to any RenderProfileWorld.
    ///
    /// Values can change depending if we are in avatar editor mode or in-game world.
    /// </summary>
    [CreateAssetMenu(menuName = "DCL/Rendering/Create Avatar Profile", fileName = "RenderProfileAvatar", order = 0)]
    public class RenderProfileAvatar : ScriptableObject
    {
        [System.Serializable]
        public class MaterialProfile
        {
            [SerializeField] private Color tintColor;
            [SerializeField] private Color lightColor;
            [SerializeField] private Vector3 lightDirection;

            public void Apply()
            {
                Shader.SetGlobalVector(ShaderUtils._LightDir, lightDirection);
                Shader.SetGlobalColor(ShaderUtils._LightColor, lightColor);
            }
        }

        [SerializeField] public MaterialProfile avatarEditor;

        [SerializeField] public MaterialProfile inWorld;
    }
}