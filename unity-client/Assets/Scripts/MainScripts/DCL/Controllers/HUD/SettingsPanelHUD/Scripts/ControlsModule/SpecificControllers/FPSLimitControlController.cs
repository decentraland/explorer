using DCL.SettingsPanelHUD.Common;
using System;
using UnityEngine;

#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

namespace DCL.SettingsPanelHUD.Controls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/FPS Limit", fileName = "FPSLimitControlController")]
    public class FPSLimitControlController : SettingsControlController
    {
        const string FPS_LIMIT_SETTINGS_KEY = "Settings.FPSLimit";

        public override object GetStoredValue()
        {
            string storedValue = PlayerPrefs.GetString(FPS_LIMIT_SETTINGS_KEY);
            if (!String.IsNullOrEmpty(storedValue))
                return Convert.ToBoolean(storedValue);
            else
                return Settings.i.qualitySettingsPresets.defaultPreset.fpsCap;
        }

        public override void OnControlChanged(object newValue, bool fromInitialize)
        {
            bool newBoolValue = (bool)newValue;
            ToggleFPSCap(newBoolValue);
            PlayerPrefs.SetString(FPS_LIMIT_SETTINGS_KEY, newBoolValue.ToString());
        }

        public override void PostApplySettings()
        {
            base.PostApplySettings();

            CommonSettingsEvents.RaiseSetQualityPresetAsCustom();
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] public static extern void ToggleFPSCap(bool useFPSCap);
#else
        public static void ToggleFPSCap(bool useFPSCap)
        {
        }
#endif
    }
}