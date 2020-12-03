using DCL.SettingsPanelHUD.Common;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Quality Preset", fileName = "QualityPresetControlController")]
    public class QualityPresetControlController : SettingsControlController
    {
        public const string TEXT_QUALITY_CUSTOM = "Custom";

        private int currentQualityPresetIndex = 0;

        public override void Initialize(ISettingsControlView settingsControlView)
        {
            base.Initialize(settingsControlView);

            SetupQualityPreset();

            CommonSettingsVariables.shouldSetQualityPresetAsCustom.OnChange += ShouldSetQualityPresetAsCustom_OnChange;
        }

        public override void OnDisable()
        {
            base.OnDisable();

            if (CommonSettingsVariables.shouldSetQualityPresetAsCustom != null)
                CommonSettingsVariables.shouldSetQualityPresetAsCustom.OnChange -= ShouldSetQualityPresetAsCustom_OnChange;
        }

        public override object GetStoredValue()
        {
            return currentQualityPresetIndex;
        }

        public override void OnControlChanged(object newValue)
        {
            int qualityPresetValue = (int)newValue;

            SettingsData.QualitySettings preset = Settings.i.qualitySettingsPresets[qualityPresetValue];
            currentQualitySetting = preset;
            currentQualityPresetIndex = qualityPresetValue;
        }

        public override void PostApplySettings()
        {
            base.PostApplySettings();

            UpdateQualitySettings();
        }

        private void UpdateQualitySettings()
        {
            if (!CommonSettingsVariables.refreshAllSettings.Get())
                CommonSettingsVariables.refreshAllSettings.Set(true);
            else
                CommonSettingsVariables.refreshAllSettings.Set(false);
        }

        private void SetupQualityPreset()
        {
            List<string> presetNames = new List<string>();
            bool presetIndexFound = false;
            SettingsData.QualitySettings preset;

            for (int i = 0; i < Settings.i.qualitySettingsPresets.Length; i++)
            {
                preset = Settings.i.qualitySettingsPresets[i];
                presetNames.Add(preset.displayName);

                if (!presetIndexFound && preset.Equals(currentQualitySetting))
                {
                    presetIndexFound = true;
                    currentQualityPresetIndex = i;
                }
            }

            ((SpinBoxSettingsControlView)view).SetLabels(presetNames.ToArray());

            if (!presetIndexFound)
                UpdateQualitySettings();
        }

        private void ShouldSetQualityPresetAsCustom_OnChange(bool current, bool previous)
        {
            if (current)
            {
                ((SpinBoxSettingsControlView)view).spinBoxControl.OverrideCurrentLabel(TEXT_QUALITY_CUSTOM);
                CommonSettingsVariables.shouldSetQualityPresetAsCustom.Set(false);
            }
        }
    }
}