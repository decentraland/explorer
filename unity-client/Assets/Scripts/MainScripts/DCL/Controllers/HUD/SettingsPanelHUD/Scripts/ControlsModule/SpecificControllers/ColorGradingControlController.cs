using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Color Grading", fileName = "ColorGradingControlController")]
    public class ColorGradingControlController : SettingsControlController
    {
        public override object GetStoredValue()
        {
            return currentQualitySetting.colorGrading;
        }

        public override void OnControlChanged(object newValue)
        {
            bool newBoolValue = (bool)newValue;

            currentQualitySetting.colorGrading = newBoolValue;
            qualitySettingsController.UpdateColorGrading(newBoolValue);
        }
    }
}