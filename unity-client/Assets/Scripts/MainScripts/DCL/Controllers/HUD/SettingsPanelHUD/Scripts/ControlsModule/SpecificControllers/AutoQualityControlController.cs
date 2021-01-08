using System;
using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Auto Quality", fileName = "AutoQualityControlController")]
    public class AutoQualityControlController : SettingsControlController
    {
        //const string AUTO_QUALITY_SETTINGS_KEY = "Settings.AutoQuality";

        //private BooleanVariable autosettingsEnabled = null;

        //public override void Initialize(ISettingsControlView settingsControlView)
        //{
        //    base.Initialize(settingsControlView);

        //    if (autosettingsEnabled == null)
        //        autosettingsEnabled = Resources.Load<BooleanVariable>("ScriptableObjects/AutoQualityEnabled");
        //}

        public override object GetStoredValue()
        {
            return currentGeneralSettings.autoqualityOn;

            //string storedValue = PlayerPrefs.GetString(AUTO_QUALITY_SETTINGS_KEY);
            //if (!String.IsNullOrEmpty(storedValue))
            //    return Convert.ToBoolean(storedValue);
            //else
            //    return Settings.i.GetDefaultGeneralSettings().autoqualityOn;
        }

        public override void OnControlChanged(object newValue, bool fromInitialize)
        {
            bool autoQualityValue = (bool)newValue;
            //bool newBoolValue = (bool)newValue;

            currentGeneralSettings.autoqualityOn = autoQualityValue;
            //if (autosettingsEnabled != null)
            //{
            //    autosettingsEnabled.Set(newBoolValue);
            //    PlayerPrefs.SetString(AUTO_QUALITY_SETTINGS_KEY, newBoolValue.ToString());
            //}

            if (autoQualityValue)
            //if (newBoolValue)
            {
                SettingsData.QualitySettings.BaseResolution currentBaseResolution = currentQualitySetting.baseResolution;
                bool currentFpsCap = currentQualitySetting.fpsCap;
                currentQualitySetting = Settings.i.lastValidAutoqualitySet;
                currentQualitySetting.baseResolution = currentBaseResolution;
                currentQualitySetting.fpsCap = currentFpsCap;
            }
        }
    }
}