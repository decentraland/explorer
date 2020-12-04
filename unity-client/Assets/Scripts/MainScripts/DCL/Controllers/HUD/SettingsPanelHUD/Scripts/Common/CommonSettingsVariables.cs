using DCL.SettingsPanelHUD.Controls;

namespace DCL.SettingsPanelHUD.Common
{
    public static class CommonSettingsVariables
    {
        public static readonly BaseVariable<bool> refreshAllSettings = new BaseVariable<bool>();
        public static readonly BaseVariable<bool> shouldSetQualityPresetAsCustom = new BaseVariable<bool>();
        public static readonly BaseVariable<bool> shouldSetSoftShadowsAsFalse = new BaseVariable<bool>();
    }
}