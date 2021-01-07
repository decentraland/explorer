using DCL.SettingsPanelHUD.Common;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DCL.SettingsPanelHUD.Controls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/AntiAliasing", fileName = "AntiAliasingControlController")]
    public class AntiAliasingControlController : SettingsControlController
    {
        const string ANTI_ALIASING_SETTINGS_KEY = "Settings.AntiAliasing";
        public const string TEXT_OFF = "OFF";

        private SliderSettingsControlView sliderView;
        private UniversalRenderPipelineAsset lightweightRenderPipelineAsset = null;

        public override void Initialize(ISettingsControlView settingsControlView)
        {
            base.Initialize(settingsControlView);

            sliderView = (SliderSettingsControlView)view;

            if (lightweightRenderPipelineAsset == null)
                lightweightRenderPipelineAsset = GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset;
        }

        public override object GetStoredValue()
        {
            int storedValue = PlayerPrefs.GetInt(ANTI_ALIASING_SETTINGS_KEY, -1);
            if (storedValue != -1)
                return AntiAliasingToSliderValue((MsaaQuality)storedValue);
            else
                return AntiAliasingToSliderValue(Settings.i.qualitySettingsPresets.defaultPreset.antiAliasing);
        }

        private float AntiAliasingToSliderValue(MsaaQuality antiAliasingValue)
        {
            return antiAliasingValue == MsaaQuality.Disabled
                ? 0
                : ((int)antiAliasingValue >> 2) + 1;
        }

        public override void OnControlChanged(object newValue)
        {
            float newFloatValue = (float)newValue;

            int antiAliasingValue = 1 << (int)newFloatValue;
            if (lightweightRenderPipelineAsset != null)
            {
                lightweightRenderPipelineAsset.msaaSampleCount = antiAliasingValue;
                PlayerPrefs.SetInt(ANTI_ALIASING_SETTINGS_KEY, antiAliasingValue);
            }

            if (newFloatValue == 0)
                sliderView.OverrideIndicatorLabel(TEXT_OFF);
            else
                sliderView.OverrideIndicatorLabel(antiAliasingValue.ToString("0x"));
        }

        public override void PostApplySettings()
        {
            base.PostApplySettings();

            CommonSettingsEvents.RaiseSetQualityPresetAsCustom();
        }
    }
}