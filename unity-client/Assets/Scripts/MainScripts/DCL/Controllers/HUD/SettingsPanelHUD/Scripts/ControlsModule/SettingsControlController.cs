using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    public class SettingsControlController : ScriptableObject
    {
        protected SettingsData.GeneralSettings currentGeneralSettings;
        protected SettingsData.QualitySettings currentQualitySetting;
        protected ISettingsControlView view;

        public virtual void Initialize(ISettingsControlView settingsControlView)
        {
            view = settingsControlView;

            currentGeneralSettings = Settings.i.generalSettings;
            currentQualitySetting = Settings.i.qualitySettings;

            Settings.i.OnGeneralSettingsChanged += OnGeneralSettingsChanged;
            Settings.i.OnQualitySettingsChanged += OnQualitySettingsChanged;
        }

        public virtual void OnDisable()
        {
            Settings.i.OnGeneralSettingsChanged -= OnGeneralSettingsChanged;
            Settings.i.OnQualitySettingsChanged -= OnQualitySettingsChanged;
        }

        public virtual object GetStoredValue()
        {
            return null;
        }

        public virtual void OnControlChanged(object newValue)
        {
        }

        public virtual void ApplySettings()
        {
            Settings.i.ApplyGeneralSettings(currentGeneralSettings);
            Settings.i.ApplyQualitySettings(currentQualitySetting);
        }

        public virtual void PostApplySettings()
        {
        }

        private void OnGeneralSettingsChanged(SettingsData.GeneralSettings newGeneralSettings)
        {
            currentGeneralSettings = newGeneralSettings;
        }

        private void OnQualitySettingsChanged(SettingsData.QualitySettings newQualitySettings)
        {
            currentQualitySetting = newQualitySettings;
        }
    }
}