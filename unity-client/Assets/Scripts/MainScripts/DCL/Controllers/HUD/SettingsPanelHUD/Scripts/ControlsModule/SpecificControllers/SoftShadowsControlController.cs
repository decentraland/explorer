using DCL.SettingsPanelHUD.Common;
using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/SoftShadows", fileName = "SoftShadowsControlController")]
    public class SoftShadowsControlController : SettingsControlController
    {
        public override void Initialize(ISettingsControlView settingsControlView)
        {
            base.Initialize(settingsControlView);

            CommonSettingsVariables.shouldSetSoftShadowsAsFalse.OnChange += ShouldSetSoftShadowsAsFalse_OnChange;
        }

        public override void OnDisable()
        {
            base.OnDisable();

            if (CommonSettingsVariables.shouldSetSoftShadowsAsFalse != null)
                CommonSettingsVariables.shouldSetSoftShadowsAsFalse.OnChange -= ShouldSetSoftShadowsAsFalse_OnChange;
        }

        public override object GetInitialValue()
        {
            return currentQualitySetting.softShadows;
        }

        public override void OnControlChanged(object newValue)
        {
            currentQualitySetting.softShadows = (bool)newValue;
        }

        public override void PostApplySettings()
        {
            base.PostApplySettings();

            CommonSettingsVariables.shouldSetQualityPresetAsCustom.Set(true);
        }

        private void ShouldSetSoftShadowsAsFalse_OnChange(bool current, bool previous)
        {
            if (current)
            {
                ((ToggleSettingsControlView)view).toggleControl.isOn = false;
                CommonSettingsVariables.shouldSetSoftShadowsAsFalse.Set(false);
            }
        }
    }
}