using DCL.SettingsPanelHUD.Common;
using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Bloom", fileName = "BloomControlController")]
    public class BloomControlController : SettingsControlController
    {
        public override object GetInitialValue()
        {
            return currentQualitySetting.bloom;
        }

        public override void OnControlChanged(object newValue)
        {
            currentQualitySetting.bloom = (bool)newValue;
        }

        public override void PostApplySettings()
        {
            base.PostApplySettings();

            CommonSettingsVariables.shouldSetQualityPresetAsCustom.Set(true);
        }
    }
}