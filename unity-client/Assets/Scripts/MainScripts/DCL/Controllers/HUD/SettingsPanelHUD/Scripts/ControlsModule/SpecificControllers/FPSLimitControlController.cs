using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/FPS Limit", fileName = "FPSLimitControlController")]
    public class FPSLimitControlController : SettingsControlController
    {
        public override object GetStoredValue()
        {
            return currentQualitySetting.fpsCap;
        }

        public override void OnControlChanged(object newValue)
        {
            bool newBoolValue = (bool)newValue;

            currentQualitySetting.fpsCap = newBoolValue;
            qualitySettingsController.UpdateFPSLimit(newBoolValue);
        }
    }
}