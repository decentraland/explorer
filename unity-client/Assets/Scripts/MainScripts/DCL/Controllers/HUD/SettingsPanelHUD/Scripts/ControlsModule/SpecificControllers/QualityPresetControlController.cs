using DCL.SettingsController;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Quality Preset", fileName = "QualityPresetControlController")]
    public class QualityPresetControlController : SettingsControlController
    {
        public const string TEXT_QUALITY_CUSTOM = "Custom";

        private SpinBoxSettingsControlView qualityPresetControlView;

        public override void Initialize(
            ISettingsControlView settingsControlView,
            IGeneralSettingsController generalSettingsController,
            IQualitySettingsController qualitySettingsController)
        {
            base.Initialize(settingsControlView, generalSettingsController, qualitySettingsController);

            qualityPresetControlView = (SpinBoxSettingsControlView)settingsControlView;

            SetupQualityPresetLabels();
        }

        public override object GetStoredValue()
        {
            return GetCurrentStoredValue();
        }

        public override void OnControlChanged(object newValue)
        {
            SettingsData.QualitySettings preset = Settings.i.qualitySettingsPresets[(int)newValue];
            currentQualitySetting = preset;
        }

        private void SetupQualityPresetLabels()
        {
            List<string> presetNames = new List<string>();
            SettingsData.QualitySettings preset;
            for (int i = 0; i < Settings.i.qualitySettingsPresets.Length; i++)
            {
                preset = Settings.i.qualitySettingsPresets[i];
                presetNames.Add(preset.displayName);
            }

            qualityPresetControlView.SetLabels(presetNames.ToArray());
        }

        private int GetCurrentStoredValue()
        {
            SettingsData.QualitySettings preset;
            for (int i = 0; i < Settings.i.qualitySettingsPresets.Length; i++)
            {
                preset = Settings.i.qualitySettingsPresets[i];
                if (preset.Equals(currentQualitySetting))
                {
                    qualityPresetControlView.spinBoxControl.OverrideCurrentLabel(preset.displayName);
                    return i;
                }
            }

            qualityPresetControlView.spinBoxControl.OverrideCurrentLabel(TEXT_QUALITY_CUSTOM);
            return qualityPresetControlView.spinBoxControl.value;
        }
    }
}