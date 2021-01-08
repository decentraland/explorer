using DCL.SettingsPanelHUD.Common;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DCL.SettingsPanelHUD.Controls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Rendering Scale", fileName = "RenderingScaleControlController")]
    public class RenderingScaleControlController : SettingsControlController
    {
        const string RENDERING_SCALE_SETTINGS_KEY = "Settings.RenderingScale";

        private UniversalRenderPipelineAsset lightweightRenderPipelineAsset = null;

        public override void Initialize(ISettingsControlView settingsControlView)
        {
            base.Initialize(settingsControlView);

            if (lightweightRenderPipelineAsset == null)
                lightweightRenderPipelineAsset = GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset;
        }

        public override object GetStoredValue()
        {
            float storedValue = PlayerPrefs.GetFloat(RENDERING_SCALE_SETTINGS_KEY, -1);
            if (storedValue != -1)
                return storedValue;
            else
                return Settings.i.qualitySettingsPresets.defaultPreset.renderScale;
        }

        public override void OnControlChanged(object newValue)
        {
            float newFloatValue = (float)newValue;

            currentQualitySetting.renderScale = newFloatValue;
            ((SliderSettingsControlView)view).OverrideIndicatorLabel(newFloatValue.ToString("0.0"));

            if (lightweightRenderPipelineAsset != null)
            {
                lightweightRenderPipelineAsset.renderScale = newFloatValue;
                PlayerPrefs.SetFloat(RENDERING_SCALE_SETTINGS_KEY, newFloatValue);
            }
        }

        public override void PostApplySettings()
        {
            base.PostApplySettings();

            CommonSettingsEvents.RaiseSetQualityPresetAsCustom();
        }
    }
}