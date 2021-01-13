using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/SoftShadows", fileName = "SoftShadowsControlController")]
    public class SoftShadowsControlController : SettingsControlController
    {
        public override object GetStoredValue()
        {
            return currentQualitySetting.softShadows;
        }

        public override void OnControlChanged(object newValue)
        {
            bool newBoolValue = (bool)newValue;

            currentQualitySetting.softShadows = newBoolValue;
            qualitySettingsController.UpdateSoftShadows(newBoolValue);
        }
    }
}