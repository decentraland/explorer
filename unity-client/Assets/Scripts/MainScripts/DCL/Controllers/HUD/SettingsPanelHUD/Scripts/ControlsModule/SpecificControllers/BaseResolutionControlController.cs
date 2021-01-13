using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Base Resolution", fileName = "BaseResolutionControlController")]
    public class BaseResolutionControlController : SettingsControlController
    {
        public override object GetStoredValue()
        {
            return (int)currentQualitySetting.baseResolution;
        }

        public override void OnControlChanged(object newValue)
        {
            SettingsData.QualitySettings.BaseResolution newBaseResValue = (SettingsData.QualitySettings.BaseResolution)newValue;

            currentQualitySetting.baseResolution = newBaseResValue;
            qualitySettingsController.UpdateBaseResolution(newBaseResValue);
        }
    }
}