using System;
using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.Helpers;
using UnityEngine;

/// <summary>
/// RenderProfileWorld allows us to toggle between several global rendering configuration presets.
/// This is useful for events, setting day/night cycles, lerping between any of those, etc.
///
/// All the presets are stored in the RenderProfileManifest object.
/// </summary>
[CreateAssetMenu(menuName = "DCL/Rendering/Create World Profile", fileName = "RenderProfileWorld", order = 0)]
public class RenderProfileWorld : ScriptableObject
{
    public GameObject loadingBlockerPrefab;
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

        if (RenderProfileManifest.currentProfile == this)
            Apply(false);
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

    public void Apply(bool verbose = true)
    {
        RenderSettings.skybox = skyboxMaterial;
        RenderSettings.ambientEquatorColor = equatorColor;
        RenderSettings.ambientSkyColor = skyColor;
        RenderSettings.ambientGroundColor = groundColor;
        RenderSettings.fogColor = fogColor;

        if (verbose)
            Debug.Log("Applying profile... " + name);
    }
}