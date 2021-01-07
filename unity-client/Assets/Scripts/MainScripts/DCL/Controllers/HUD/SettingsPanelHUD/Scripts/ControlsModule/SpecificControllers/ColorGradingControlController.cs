using DCL.SettingsController;
using DCL.SettingsPanelHUD.Common;
using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace DCL.SettingsPanelHUD.Controls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Color Grading", fileName = "ColorGradingControlController")]
    public class ColorGradingControlController : SettingsControlController
    {
        const string COLOR_GRADING_SETTINGS_KEY = "Settings.ColorGrading";

        private QualitySettingsController qualitySettings; // TODO (Santi): Refactorize!

        public override void Initialize(ISettingsControlView settingsControlView)
        {
            base.Initialize(settingsControlView);

            qualitySettings = GameObject.FindObjectOfType<QualitySettingsController>();
        }

        public override object GetStoredValue()
        {
            string storedValue = PlayerPrefs.GetString(COLOR_GRADING_SETTINGS_KEY);
            if (!String.IsNullOrEmpty(storedValue))
                return Convert.ToBoolean(storedValue);
            else
                return Settings.i.qualitySettingsPresets.defaultPreset.colorGrading;
        }

        public override void OnControlChanged(object newValue)
        {
            bool newMuteSoundValue = (bool)newValue;

            Tonemapping toneMapping;
            if (qualitySettings.postProcessVolume.profile.TryGet<Tonemapping>(out toneMapping))
            {
                toneMapping.active = newMuteSoundValue;
                PlayerPrefs.SetString(COLOR_GRADING_SETTINGS_KEY, newValue.ToString());
            }
        }

        public override void PostApplySettings()
        {
            base.PostApplySettings();

            CommonSettingsEvents.RaiseSetQualityPresetAsCustom();
        }
    }
}