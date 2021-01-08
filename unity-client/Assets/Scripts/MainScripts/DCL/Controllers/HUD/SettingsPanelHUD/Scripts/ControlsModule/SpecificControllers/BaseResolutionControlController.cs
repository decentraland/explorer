using DCL.Interface;
using DCL.SettingsPanelHUD.Common;
using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Base Resolution", fileName = "BaseResolutionControlController")]
    public class BaseResolutionControlController : SettingsControlController
    {
        const string BASE_RESOLUTION_SETTINGS_KEY = "Settings.BaseResolution";

        public override object GetStoredValue()
        {
            int storedValue = PlayerPrefs.GetInt(BASE_RESOLUTION_SETTINGS_KEY, -1);
            if (storedValue != -1)
                return storedValue;
            else
                return (int)Settings.i.qualitySettingsPresets.defaultPreset.baseResolution;
        }

        public override void OnControlChanged(object newValue, bool fromInitialize)
        {
            int newIntValue = (int)newValue;

            switch ((SettingsData.QualitySettings.BaseResolution)newIntValue)
            {
                case SettingsData.QualitySettings.BaseResolution.BaseRes_720:
                    WebInterface.SetBaseResolution(720);
                    break;
                case SettingsData.QualitySettings.BaseResolution.BaseRes_1080:
                    WebInterface.SetBaseResolution(1080);
                    break;
                case SettingsData.QualitySettings.BaseResolution.BaseRes_Unlimited:
                    WebInterface.SetBaseResolution(9999);
                    break;
            }

            PlayerPrefs.SetInt(BASE_RESOLUTION_SETTINGS_KEY, newIntValue);
        }

        public override void PostApplySettings()
        {
            base.PostApplySettings();

            CommonSettingsEvents.RaiseSetQualityPresetAsCustom();
        }
    }
}