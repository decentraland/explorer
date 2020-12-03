using DCL.SettingsPanelHUD.Common;
using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Shadow", fileName = "ShadowControlController")]
    public class ShadowControlController : SettingsControlController
    {
        public override object GetInitialValue()
        {
            return currentQualitySetting.shadows;
        }

        public override void OnControlChanged(object newValue)
        {
            bool isOn = (bool)newValue;

            currentQualitySetting.shadows = isOn;
            CommonSettingsVariables.shadowState.Set(isOn);
            CommonSettingsVariables.shouldSetQualityPresetAsCustom.Set(true);
            Settings.i.ApplyQualitySettings(currentQualitySetting);
        }
    }
}