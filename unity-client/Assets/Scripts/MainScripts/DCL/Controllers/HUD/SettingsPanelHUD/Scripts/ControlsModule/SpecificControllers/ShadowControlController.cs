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
            currentQualitySetting.shadows = (bool)newValue;
        }

        public override void PostApplySettings()
        {
            base.PostApplySettings();

            if (!currentQualitySetting.shadows)
                CommonSettingsVariables.shouldSetSoftShadowsAsFalse.Set(true);

            CommonSettingsVariables.shouldSetQualityPresetAsCustom.Set(true);
        }
    }
}