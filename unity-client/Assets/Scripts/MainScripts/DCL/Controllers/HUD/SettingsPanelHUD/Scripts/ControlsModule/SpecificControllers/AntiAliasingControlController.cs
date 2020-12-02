using DCL.SettingsPanelHUD.Common;
using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/AntiAliasing", fileName = "AntiAliasingControlController")]
    public class AntiAliasingControlController : SettingsControlController
    {
        public const string TEXT_OFF = "OFF";

        public override object GetInitialValue()
        {
            float antiAliasingValue = currentQualitySetting.antiAliasing == UnityEngine.Rendering.Universal.MsaaQuality.Disabled ? 0 : ((int)currentQualitySetting.antiAliasing >> 2) + 1;
            return antiAliasingValue;
        }

        public override void OnControlChanged(object newValue)
        {
            float receivedValue = (float)newValue;

            int antiAliasingValue = 1 << (int)receivedValue;
            currentQualitySetting.antiAliasing = (UnityEngine.Rendering.Universal.MsaaQuality)antiAliasingValue;
            if ((int)receivedValue == 0)
            {
                ((SliderSettingsControlView)view).OverrideIndicatorLabel(TEXT_OFF);
            }
            else
            {
                ((SliderSettingsControlView)view).OverrideIndicatorLabel(antiAliasingValue.ToString("0x"));
            }

            CommonSettingsVariables.shouldSetQualityPresetAsCustom.Set(true);

            Settings.i.ApplyGeneralSettings(currentGeneralSettings);
            Settings.i.SaveSettings();
        }
    }
}