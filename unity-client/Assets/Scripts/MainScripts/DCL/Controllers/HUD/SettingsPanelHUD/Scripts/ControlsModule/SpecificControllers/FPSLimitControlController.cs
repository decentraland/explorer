﻿using DCL.SettingsPanelHUD.Common;
using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/FPS Limit", fileName = "FPSLimitControlController")]
    public class FPSLimitControlController : SettingsControlController
    {
        public override object GetInitialValue()
        {
            return currentQualitySetting.fpsCap;
        }

        public override void OnControlChanged(object newValue)
        {
            currentQualitySetting.fpsCap = (bool)newValue;
        }

        public override void PostApplySettings()
        {
            base.PostApplySettings();

            CommonSettingsVariables.shouldSetQualityPresetAsCustom.Set(true);
        }
    }
}