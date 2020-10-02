using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Helpers;
using UnityEngine;

[CreateAssetMenu(menuName = "DCL/Rendering/Create RenderProfileAvatar", fileName = "RenderProfileAvatar", order = 0)]
public class RenderProfileAvatar : ScriptableObject
{
    [System.Serializable]
    private class MaterialProfile
    {
        public Vector3 lightDirection;

        public void Apply()
        {
            Shader.SetGlobalVector(ShaderUtils._LightDir, lightDirection);
        }
    }

    public enum ProfileID
    {
        AVATAR_EDITOR,
        IN_WORLD,
    }

    [SerializeField] private MaterialProfile avatarEditorProfile;
    [SerializeField] private MaterialProfile inWorldProfile;

    public void Apply(ProfileID id)
    {
        switch (id)
        {
            case ProfileID.AVATAR_EDITOR:
                avatarEditorProfile.Apply();
                break;
            case ProfileID.IN_WORLD:
                inWorldProfile.Apply();
                break;
        }
    }
}