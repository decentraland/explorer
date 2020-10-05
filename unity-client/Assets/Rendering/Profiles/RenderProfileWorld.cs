using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.Helpers;
using UnityEngine;

[CreateAssetMenu(menuName = "DCL/Rendering/Create World Profile", fileName = "RenderProfileWorld", order = 0)]
public class RenderProfileWorld : ScriptableObject
{
    [SerializeField] private Material skyboxMaterial;
    [SerializeField] private Color skyColor;
    [SerializeField] private Color equatorColor;
    [SerializeField] private Color groundColor;
    [SerializeField] private Color fogColor;

    public RenderProfileAvatar avatarProfile;

    public void Apply()
    {
        RenderSettings.skybox = skyboxMaterial;
        RenderSettings.ambientEquatorColor = equatorColor;
        RenderSettings.ambientSkyColor = skyColor;
        RenderSettings.ambientGroundColor = groundColor;
        RenderSettings.fogColor = fogColor;
    }
}