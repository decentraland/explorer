using System.Collections.Generic;
using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Quality Preset", fileName = "QualityPresetControlController")]
    public class QualityPresetControlController : SpinBoxSettingsControlController
    {
        public const string TEXT_QUALITY_CUSTOM = "Custom";

        public override void Initialize(SettingsControlModel controlConfig)
        {
            base.Initialize(controlConfig);

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

            RaiseOnOverrideIndicatorLabel(presetNames.ToArray());
        }

        private int GetCurrentStoredValue()
        {
            SettingsData.QualitySettings preset;
            for (int i = 0; i < Settings.i.qualitySettingsPresets.Length; i++)
            {
                preset = Settings.i.qualitySettingsPresets[i];
                if (preset.Equals(currentQualitySetting))
                {
                    RaiseOnOverrideCurrentLabel(preset.displayName);
                    return i;
                }
            }

            RaiseOnOverrideCurrentLabel(TEXT_QUALITY_CUSTOM);
            return 0;
        }
    }
}