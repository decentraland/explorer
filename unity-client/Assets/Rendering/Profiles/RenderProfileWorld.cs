using System;
using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.Helpers;
using UnityEngine;

[CreateAssetMenu(menuName = "DCL/Rendering/Create World Profile", fileName = "RenderProfileWorld", order = 0)]
public class RenderProfileWorld : ScriptableObject
{
    [SerializeField] public GameObject loadingBlockerPrefab;
    [SerializeField] private Material skyboxMaterial;
    [SerializeField] private Color skyColor;
    [SerializeField] private Color equatorColor;
    [SerializeField] private Color groundColor;
    [SerializeField] private Color fogColor;

    public RenderProfileAvatar avatarProfile;

#if UNITY_EDITOR
    public bool fillWithRenderSettings;

    //NOTE(Brian): Workaround to make an editor-time action.
    //             ContextMenu doesn't seem to work with ScriptableObjects.
    private void OnValidate()
    {
        if (fillWithRenderSettings)
        {
            FillWithRenderSettings();
        }

        fillWithRenderSettings = false;
    }

    public void FillWithRenderSettings()
    {
        skyboxMaterial = RenderSettings.skybox;
        equatorColor = RenderSettings.ambientEquatorColor;
        skyColor = RenderSettings.ambientSkyColor;
        groundColor = RenderSettings.ambientGroundColor;
        fogColor = RenderSettings.fogColor;
    }
#endif

    public void Apply()
    {
        RenderSettings.skybox = skyboxMaterial;
        RenderSettings.ambientEquatorColor = equatorColor;
        RenderSettings.ambientSkyColor = skyColor;
        RenderSettings.ambientGroundColor = groundColor;
        RenderSettings.fogColor = fogColor;
    }
}