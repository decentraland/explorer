﻿using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.LWRP;
using UnityEngine.Rendering.PostProcessing;
using System.Reflection;

using UnitySettings = UnityEngine.QualitySettings;

namespace DCL.SettingsHUD
{
    public class QualitySettingsController : MonoBehaviour
    {
        private LightweightRenderPipelineAsset lightweightRenderPipelineAsset = null;

        private FieldInfo lwrpaShadowField = null;
        private FieldInfo lwrpaSoftShadowField = null;
        private FieldInfo lwrpaShadowResolutionField = null;

        [SerializeField] Light environmentLight = null;
        [SerializeField] PostProcessVolume postProcessVolume = null;
        [SerializeField] new Camera camera = null;

        void Awake()
        {
            if (lightweightRenderPipelineAsset == null)
            {
                lightweightRenderPipelineAsset = GraphicsSettings.renderPipelineAsset as LightweightRenderPipelineAsset;

                // NOTE: LightweightRenderPipelineAsset doesn't expose properties to set any of the following fields
                lwrpaShadowField = lightweightRenderPipelineAsset.GetType().GetField("m_MainLightShadowsSupported", BindingFlags.NonPublic | BindingFlags.Instance);
                lwrpaSoftShadowField = lightweightRenderPipelineAsset.GetType().GetField("m_SoftShadowsSupported", BindingFlags.NonPublic | BindingFlags.Instance);
                lwrpaShadowResolutionField = lightweightRenderPipelineAsset.GetType().GetField("m_MainLightShadowmapResolution", BindingFlags.NonPublic | BindingFlags.Instance);
            }

            ApplyQualitySettings(Settings.i.qualitySettings);
        }

        void OnEnable()
        {
            Settings.i.OnQualitySettingsChanged += ApplyQualitySettings;
        }

        void OnDisable()
        {
            Settings.i.OnQualitySettingsChanged -= ApplyQualitySettings;
        }

        void ApplyQualitySettings(QualitySettings qualitySettings)
        {
            UnitySettings.masterTextureLimit = (int)qualitySettings.textureQuality;

            if (lightweightRenderPipelineAsset)
            {
                lightweightRenderPipelineAsset.msaaSampleCount = (int)qualitySettings.antiAliasing;
                lightweightRenderPipelineAsset.renderScale = qualitySettings.renderScale;

                lwrpaShadowField?.SetValue(lightweightRenderPipelineAsset, qualitySettings.shadows);
                lwrpaSoftShadowField?.SetValue(lightweightRenderPipelineAsset, qualitySettings.softShadows);
                lwrpaShadowResolutionField?.SetValue(lightweightRenderPipelineAsset, qualitySettings.shadowResolution);
            }

            if (environmentLight)
            {
                LightShadows shadowType = LightShadows.None;
                if (qualitySettings.shadows)
                {
                    shadowType = qualitySettings.softShadows ? LightShadows.Soft : LightShadows.Hard;
                }
                environmentLight.shadows = shadowType;
            }

            if (postProcessVolume)
            {
                Bloom bloom;
                if (postProcessVolume.profile.TryGetSettings(out bloom))
                {
                    bloom.enabled.value = qualitySettings.bloom;
                }
                ColorGrading colorGrading;
                if (postProcessVolume.profile.TryGetSettings(out colorGrading))
                {
                    colorGrading.enabled.value = qualitySettings.colorGrading;
                }
            }

            if (camera)
            {
                camera.farClipPlane = qualitySettings.cameraDrawDistance;
            }
        }
    }
}