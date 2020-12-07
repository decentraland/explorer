using DCL.SettingsPanelHUD.Controls;
using System;

namespace DCL.SettingsPanelHUD.Common
{
    public static class CommonSettingsVariables
    {
        public static readonly BaseVariable<bool> shouldSetQualityPresetAsCustom = new BaseVariable<bool>();
        public static readonly BaseVariable<bool> shouldSetSoftShadowsAsFalse = new BaseVariable<bool>();
    }

    public static class CommonSettingsEvents
    {
        public static event Action<SettingsControlController> OnRefreshAllSettings;

        public static void RaiseRefreshAllSettings(SettingsControlController sender)
        {
            OnRefreshAllSettings?.Invoke(sender);
        }
    }
}