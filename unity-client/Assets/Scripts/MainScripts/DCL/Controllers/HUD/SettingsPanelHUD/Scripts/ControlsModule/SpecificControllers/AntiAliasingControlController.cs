using DCL.SettingsController;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace DCL.SettingsPanelHUD.Controls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/AntiAliasing", fileName = "AntiAliasingControlController")]
    public class AntiAliasingControlController : SettingsControlController
    {
        public const string TEXT_OFF = "OFF";

        private SliderSettingsControlView sliderView;

        public override void Initialize(
            ISettingsControlView settingsControlView,
            IGeneralSettingsReferences generalSettingsController,
            IQualitySettingsReferences qualitySettingsController)
        {
            base.Initialize(settingsControlView, generalSettingsController, qualitySettingsController);

            sliderView = (SliderSettingsControlView)view;
        }

        public override object GetStoredValue()
        {
            float antiAliasingValue =
                currentQualitySetting.antiAliasing == MsaaQuality.Disabled
                    ? 0
                    : ((int)currentQualitySetting.antiAliasing >> 2) + 1;

            return antiAliasingValue;
        }

        public override void OnControlChanged(object newValue)
        {
            float newFloatValue = (float)newValue;
            int antiAliasingValue = 1 << (int)newFloatValue;

            currentQualitySetting.antiAliasing = (MsaaQuality)antiAliasingValue;
            qualitySettingsController.UpdateAntiAliasing(antiAliasingValue);

            if (newFloatValue == 0)
                sliderView.OverrideIndicatorLabel(TEXT_OFF);
            else
                sliderView.OverrideIndicatorLabel(antiAliasingValue.ToString("0x"));
        }
    }
}