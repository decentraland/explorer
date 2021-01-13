using Cinemachine;
using DCL.Interface;
using DCL.Rendering;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

namespace DCL.SettingsController
{
    public interface IQualitySettingsReferences
    {
        void UpdateBloom(bool isActive);
        void UpdateAntiAliasing(int newValue);
        void UpdateBaseResolution(SettingsData.QualitySettings.BaseResolution newValue);
        void UpdateColorGrading(bool isActive);
        void UpdateDetailObjectCulling(bool isActive);
        void UpdateDetailObjectCullingSize(float newValue);
        void UpdateDrawDistance(float newValue);
        void UpdateFPSLimit(bool useFPSCap);
        void UpdateRenderingScale(float newValue);
        void UpdateShadows(bool isActive);
        void UpdateShadowDistance(float newValue);
        void UpdateShadowResolution(UnityEngine.Rendering.Universal.ShadowResolution newValue);
        void UpdateSoftShadows(bool isActive);
    }

    public class QualitySettingsReferences : MonoBehaviour, IQualitySettingsReferences
    {
        public Light environmentLight = null;
        public Volume postProcessVolume = null;
        public CinemachineFreeLook thirdPersonCamera = null;
        public CinemachineVirtualCamera firstPersonCamera = null;
        public CullingControllerSettingsData cullingControllerSettingsData = null;

        public static QualitySettingsReferences i { get; private set; }

        private UniversalRenderPipelineAsset lightweightRenderPipelineAsset = null;
        private FieldInfo lwrpaShadowField = null;
        private FieldInfo lwrpaSoftShadowField = null;
        private FieldInfo lwrpaShadowResolutionField = null;

        private void Awake()
        {
            i = this;

            if (lightweightRenderPipelineAsset == null)
            {
                lightweightRenderPipelineAsset = GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset;

                if (lightweightRenderPipelineAsset == null) return;

                // NOTE: LightweightRenderPipelineAsset doesn't expose properties to set any of the following fields	
                lwrpaShadowField = lightweightRenderPipelineAsset.GetType().GetField("m_MainLightShadowsSupported", BindingFlags.NonPublic | BindingFlags.Instance);
                lwrpaSoftShadowField = lightweightRenderPipelineAsset.GetType().GetField("m_SoftShadowsSupported", BindingFlags.NonPublic | BindingFlags.Instance);
                lwrpaShadowResolutionField = lightweightRenderPipelineAsset.GetType().GetField("m_MainLightShadowmapResolution", BindingFlags.NonPublic | BindingFlags.Instance);
            }
        }

        public void UpdateBloom(bool isActive)
        {
            if (postProcessVolume)
            {
                if (postProcessVolume.profile.TryGet<Bloom>(out Bloom bloom))
                {
                    bloom.active = isActive;
                }
            }
        }

        public void UpdateAntiAliasing(int newValue)
        {
            if (lightweightRenderPipelineAsset != null)
                lightweightRenderPipelineAsset.msaaSampleCount = newValue;
        }

        public void UpdateBaseResolution(SettingsData.QualitySettings.BaseResolution newValue)
        {
            switch (newValue)
            {
                case SettingsData.QualitySettings.BaseResolution.BaseRes_720:
                    WebInterface.SetBaseResolution(720);
                    break;
                case SettingsData.QualitySettings.BaseResolution.BaseRes_1080:
                    WebInterface.SetBaseResolution(1080);
                    break;
                case SettingsData.QualitySettings.BaseResolution.BaseRes_Unlimited:
                    WebInterface.SetBaseResolution(9999);
                    break;
            }
        }

        public void UpdateColorGrading(bool isActive)
        {
            Tonemapping toneMapping;
            if (QualitySettingsReferences.i.postProcessVolume.profile.TryGet<Tonemapping>(out toneMapping))
            {
                toneMapping.active = isActive;
            }
        }

        public void UpdateDetailObjectCulling(bool isActive)
        {
            Environment.i.platform.cullingController.SetObjectCulling(isActive);
            Environment.i.platform.cullingController.SetShadowCulling(isActive);
            Environment.i.platform.cullingController.MarkDirty();
        }

        public void UpdateDetailObjectCullingSize(float newValue)
        {
            if (Settings.i.qualitySettings.enableDetailObjectCulling)
            {
                var settings = Environment.i.platform.cullingController.GetSettingsCopy();

                settings.rendererProfile = CullingControllerProfile.Lerp(
                    QualitySettingsReferences.i.cullingControllerSettingsData.rendererProfileMin,
                    QualitySettingsReferences.i.cullingControllerSettingsData.rendererProfileMax,
                    newValue / 100.0f);

                settings.skinnedRendererProfile = CullingControllerProfile.Lerp(
                    QualitySettingsReferences.i.cullingControllerSettingsData.skinnedRendererProfileMin,
                    QualitySettingsReferences.i.cullingControllerSettingsData.skinnedRendererProfileMax,
                    newValue / 100.0f);

                Environment.i.platform.cullingController.SetSettings(settings);
            }
        }

        public void UpdateDrawDistance(float newValue)
        {
            if (QualitySettingsReferences.i.thirdPersonCamera)
                QualitySettingsReferences.i.thirdPersonCamera.m_Lens.FarClipPlane = newValue;

            if (QualitySettingsReferences.i.firstPersonCamera)
                QualitySettingsReferences.i.firstPersonCamera.m_Lens.FarClipPlane = newValue;

            RenderSettings.fogEndDistance = newValue;
            RenderSettings.fogStartDistance = newValue * 0.8f;
        }

        public void UpdateFPSLimit(bool useFPSCap)
        {
            ToggleFPSCap(useFPSCap);
        }

        public void UpdateRenderingScale(float newValue)
        {
            if (lightweightRenderPipelineAsset != null)
                lightweightRenderPipelineAsset.renderScale = newValue;
        }

        public void UpdateShadows(bool isActive)
        {
            if (lightweightRenderPipelineAsset != null)
                lwrpaShadowField?.SetValue(lightweightRenderPipelineAsset, isActive);

            if (QualitySettingsReferences.i.environmentLight)
            {
                LightShadows shadowType = LightShadows.None;

                if (isActive)
                    shadowType = isActive ? LightShadows.Soft : LightShadows.Hard;

                QualitySettingsReferences.i.environmentLight.shadows = shadowType;
            }
        }

        public void UpdateShadowDistance(float newValue)
        {
            if (lightweightRenderPipelineAsset)
                lightweightRenderPipelineAsset.shadowDistance = newValue;
        }

        public void UpdateShadowResolution(UnityEngine.Rendering.Universal.ShadowResolution newValue)
        {
            if (lightweightRenderPipelineAsset != null)
                lwrpaShadowResolutionField?.SetValue(lightweightRenderPipelineAsset, newValue);
        }

        public void UpdateSoftShadows(bool isActive)
        {
            if (lightweightRenderPipelineAsset != null)
                lwrpaSoftShadowField?.SetValue(lightweightRenderPipelineAsset, isActive);

            if (QualitySettingsReferences.i.environmentLight)
            {
                LightShadows shadowType = LightShadows.None;

                if (Settings.i.qualitySettings.shadows)
                    shadowType = isActive ? LightShadows.Soft : LightShadows.Hard;

                QualitySettingsReferences.i.environmentLight.shadows = shadowType;
            }
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] public static extern void ToggleFPSCap(bool useFPSCap);
#else
        public static void ToggleFPSCap(bool useFPSCap)
        {
        }
#endif
    }
}