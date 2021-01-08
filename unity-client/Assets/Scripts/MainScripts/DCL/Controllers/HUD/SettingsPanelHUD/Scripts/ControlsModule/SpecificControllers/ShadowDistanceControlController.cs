using DCL.SettingsPanelHUD.Common;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DCL.SettingsPanelHUD.Controls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Shadow Distance", fileName = "ShadowDistanceControlController")]
    public class ShadowDistanceControlController : SettingsControlController
    {
        const string SHADOW_DISTANCE_SETTINGS_KEY = "Settings.ShadowDistance";

        private UniversalRenderPipelineAsset lightweightRenderPipelineAsset = null;

        public override void Initialize(ISettingsControlView settingsControlView)
        {
            base.Initialize(settingsControlView);

            if (lightweightRenderPipelineAsset == null)
            {
                lightweightRenderPipelineAsset = GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset;
            }
        }

        public override object GetStoredValue()
        {
            float storedValue = PlayerPrefs.GetFloat(SHADOW_DISTANCE_SETTINGS_KEY, -1);
            if (storedValue != -1)
                return storedValue;
            else
                return Settings.i.qualitySettingsPresets.defaultPreset.shadowDistance;
        }

        public override void OnControlChanged(object newValue)
        {
            float newFloatValue = (float)newValue;

            if (lightweightRenderPipelineAsset)
                lightweightRenderPipelineAsset.shadowDistance = newFloatValue;

            PlayerPrefs.SetFloat(SHADOW_DISTANCE_SETTINGS_KEY, newFloatValue);
        }

        public override void PostApplySettings()
        {
            base.PostApplySettings();

            CommonSettingsEvents.RaiseSetQualityPresetAsCustom();
        }
    }
}