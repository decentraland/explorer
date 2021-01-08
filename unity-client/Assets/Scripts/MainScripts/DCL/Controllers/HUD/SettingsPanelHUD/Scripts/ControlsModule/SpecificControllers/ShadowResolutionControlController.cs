using DCL.SettingsPanelHUD.Common;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DCL.SettingsPanelHUD.Controls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Shadow Resolution", fileName = "ShadowResolutionControlController")]
    public class ShadowResolutionControlController : SettingsControlController
    {
        const string SHADOW_RESOLUTION_SETTINGS_KEY = "Settings.ShadowResolution";

        private UniversalRenderPipelineAsset lightweightRenderPipelineAsset = null;
        private FieldInfo lwrpaShadowResolutionField = null;

        public override void Initialize(ISettingsControlView settingsControlView)
        {
            base.Initialize(settingsControlView);

            if (lightweightRenderPipelineAsset == null)
            {
                lightweightRenderPipelineAsset = GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset;

                if (lightweightRenderPipelineAsset == null) return;

                lwrpaShadowResolutionField = lightweightRenderPipelineAsset.GetType().GetField("m_MainLightShadowmapResolution", BindingFlags.NonPublic | BindingFlags.Instance);
            }
        }

        public override object GetStoredValue()
        {
            int storedValue = PlayerPrefs.GetInt(SHADOW_RESOLUTION_SETTINGS_KEY, -1);
            if (storedValue != -1)
                return ShadowResolutionToSpinBoxValue(storedValue);
            else
                return ShadowResolutionToSpinBoxValue((int)Settings.i.qualitySettingsPresets.defaultPreset.shadowResolution);
        }

        private int ShadowResolutionToSpinBoxValue(int shadowResolutionValue)
        {
            return (int)Mathf.Log(shadowResolutionValue, 2) - 8;
        }

        public override void OnControlChanged(object newValue)
        {
            int newIntValue = 256 << (int)newValue;

            if (lightweightRenderPipelineAsset != null)
                lwrpaShadowResolutionField?.SetValue(lightweightRenderPipelineAsset, (UnityEngine.Rendering.Universal.ShadowResolution)(newIntValue));

            PlayerPrefs.SetInt(SHADOW_RESOLUTION_SETTINGS_KEY, newIntValue);
        }

        public override void PostApplySettings()
        {
            base.PostApplySettings();

            CommonSettingsEvents.RaiseSetQualityPresetAsCustom();
        }
    }
}