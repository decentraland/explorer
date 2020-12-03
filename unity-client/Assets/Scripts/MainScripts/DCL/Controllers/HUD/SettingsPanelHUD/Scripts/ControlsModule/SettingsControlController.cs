using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    public abstract class SettingsControlController : ScriptableObject
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

        public abstract object GetInitialValue();

        public abstract void OnControlChanged(object newValue);

        public virtual void ApplySettings()
        {
            if (!currentGeneralSettings.Equals(Settings.i.generalSettings))
                Settings.i.ApplyGeneralSettings(currentGeneralSettings);

            if (!currentQualitySetting.Equals(Settings.i.qualitySettings))
                Settings.i.ApplyQualitySettings(currentQualitySetting);
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