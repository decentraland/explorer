using System;

namespace DCL.SettingsPanelHUD.Common
{
    public static class CommonSettingsEvents
    {
        public static event Action OnResetAllSettings;
        public static void RaiseResetAllSettings()
        {
            OnResetAllSettings?.Invoke();
        }

        public static event Action OnRefreshAllWidgetsSize;
        public static void RaiseRefreshAllWidgetsSize()
        {
            OnRefreshAllWidgetsSize?.Invoke();
        }
    }
}