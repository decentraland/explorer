using System;
using Cinemachine;
using System.Reflection;
using DCL.Interface;
using DCL.Rendering;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using QualitySettings = DCL.SettingsData.QualitySettings;
using UnitySettings = UnityEngine.QualitySettings;

namespace DCL.SettingsController
{
    public class QualitySettingsController : MonoBehaviour
    {
        private UniversalRenderPipelineAsset lightweightRenderPipelineAsset = null;

        private FieldInfo lwrpaShadowField = null;
        private FieldInfo lwrpaSoftShadowField = null;
        private FieldInfo lwrpaShadowResolutionField = null;

        public Light environmentLight = null;

        public Volume postProcessVolume = null;
        public CinemachineFreeLook thirdPersonCamera = null;
        public CinemachineVirtualCamera firstPersonCamera = null;

        public CullingControllerSettingsData cullingControllerSettingsData = null;

        void Start()
        {
            if (lightweightRenderPipelineAsset == null)
            {
                lightweightRenderPipelineAsset = GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset;

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
            switch (qualitySettings.baseResolution)
            {
                case QualitySettings.BaseResolution.BaseRes_720:
                    WebInterface.SetBaseResolution(720);
                    break;
                case QualitySettings.BaseResolution.BaseRes_1080:
                    WebInterface.SetBaseResolution(1080);
                    break;
                case QualitySettings.BaseResolution.BaseRes_Unlimited:
                    WebInterface.SetBaseResolution(9999);
                    break;
            }

            if (lightweightRenderPipelineAsset)
            {
                lightweightRenderPipelineAsset.msaaSampleCount = (int) qualitySettings.antiAliasing;
                lightweightRenderPipelineAsset.renderScale = qualitySettings.renderScale;
                lightweightRenderPipelineAsset.shadowDistance = qualitySettings.shadowDistance;

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
                if (postProcessVolume.profile.TryGet<Bloom>(out bloom))
                {
                    bloom.active = qualitySettings.bloom;
                }

                Tonemapping toneMapping;
                if (postProcessVolume.profile.TryGet<Tonemapping>(out toneMapping))
                {
                    toneMapping.active = qualitySettings.colorGrading;
                }
            }

            Environment.i.cullingController.SetObjectCulling(qualitySettings.limitSmallObjectsDetail);
            Environment.i.cullingController.SetShadowCulling(qualitySettings.limitSmallObjectsDetail);
            Environment.i.cullingController.SetDirty();

            if (qualitySettings.limitSmallObjectsDetail)
            {
                var settings = Environment.i.cullingController.GetSettings();

                settings.rendererProfile = CullingControllerProfile.Lerp(
                    cullingControllerSettingsData.rendererProfileMin,
                    cullingControllerSettingsData.rendererProfileMax,
                    qualitySettings.limitSmallObjectsRange);

                settings.skinnedRendererProfile = CullingControllerProfile.Lerp(
                    cullingControllerSettingsData.skinnedRendererProfileMin,
                    cullingControllerSettingsData.skinnedRendererProfileMax,
                    qualitySettings.limitSmallObjectsRange);

                Environment.i.cullingController.SetSettings(settings);
            }

            if (thirdPersonCamera)
            {
                thirdPersonCamera.m_Lens.FarClipPlane = qualitySettings.cameraDrawDistance;
            }

            if (firstPersonCamera)
            {
                firstPersonCamera.m_Lens.FarClipPlane = qualitySettings.cameraDrawDistance;
            }
        }
    }
}