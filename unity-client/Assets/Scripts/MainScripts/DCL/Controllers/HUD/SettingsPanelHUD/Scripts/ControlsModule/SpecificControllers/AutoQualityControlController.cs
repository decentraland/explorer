using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Auto Quality", fileName = "AutoQualityControlController")]
    public class AutoQualityControlController : SettingsControlController
    {
        private SettingsData.QualitySettings currentQualitySetting;
        private SettingsData.GeneralSettings currentGeneralSettings;

        public override void Initialize()
        {
            currentQualitySetting = Settings.i.qualitySettings;
            currentGeneralSettings = Settings.i.generalSettings;
        }

        public override object GetStoredValue()
        {
            return currentGeneralSettings.autoqualityOn;
        }

        public override void OnControlChanged(object newValue)
        {
            //autoqualityBlockCanvasGroup.interactable = !(bool)newValue;
            currentGeneralSettings.autoqualityOn = (bool)newValue;
            //autoqualityBlocker.SetActive((bool)newValue);
            if ((bool)newValue)
            {
                SettingsData.QualitySettings.BaseResolution currentBaseResolution = currentQualitySetting.baseResolution;
                bool currentFpsCap = currentQualitySetting.fpsCap;
                currentQualitySetting = Settings.i.lastValidAutoqualitySet;
                currentQualitySetting.baseResolution = currentBaseResolution;
                currentQualitySetting.fpsCap = currentFpsCap;
            }

            Settings.i.ApplyQualitySettings(currentQualitySetting);
            Settings.i.ApplyGeneralSettings(currentGeneralSettings);
            Settings.i.SaveSettings();
        }
    }
}