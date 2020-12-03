using UnityEngine;

namespace DCL.SettingsPanelHUD.Common
{
    public static class CommonSettingsVariables
    {
        public static readonly BaseVariable<bool> shouldSetQualityPresetAsCustom = new BaseVariable<bool>();
        public static readonly BaseVariable<bool> shadowState = new BaseVariable<bool>();
    }
}