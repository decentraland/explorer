using DCL.SettingsPanelHUD.Common;
using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Shadow", fileName = "ShadowControlController")]
    public class ShadowControlController : SettingsControlController
    {
        public override object GetStoredValue()
        {
            return currentQualitySetting.shadows;
        }

        public override void OnControlChanged(object newValue)
        {
            bool newBoolValue = (bool)newValue;

            currentQualitySetting.shadows = newBoolValue;
            qualitySettingsController.UpdateShadows(newBoolValue);

            CommonSettingsScriptableObjects.shadowsDisabled.Set(!currentQualitySetting.shadows);
        }
    }
}