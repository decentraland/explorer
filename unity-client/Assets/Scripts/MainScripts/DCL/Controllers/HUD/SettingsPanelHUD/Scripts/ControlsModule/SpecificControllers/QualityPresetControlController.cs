using DCL.SettingsPanelHUD.Common;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Quality Preset", fileName = "QualityPresetControlController")]
    public class QualityPresetControlController : SettingsControlController
    {
        public const string TEXT_QUALITY_CUSTOM = "Custom";

        public override void Initialize(ISettingsControlView settingsControlView)
        {
            base.Initialize(settingsControlView);

            SetupQualityPresetLabels();

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
            return GetCurrentStoredValue();
        }

        public override void OnControlChanged(object newValue)
        {
            SettingsData.QualitySettings preset = Settings.i.qualitySettingsPresets[(int)newValue];
            currentQualitySetting = preset;
        }

        public override void PostApplySettings()
        {
            base.PostApplySettings();

            RefreshAllSettings();
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

            ((SpinBoxSettingsControlView)view).SetLabels(presetNames.ToArray());
        }

        private int GetCurrentStoredValue()
        {
            SettingsData.QualitySettings preset;
            for (int i = 0; i < Settings.i.qualitySettingsPresets.Length; i++)
            {
                preset = Settings.i.qualitySettingsPresets[i];
                if (preset.Equals(currentQualitySetting))
                {
                    return i;
                }
            }

            return 0;
        }

        private void RefreshAllSettings()
        {
            if (!CommonSettingsVariables.refreshAllSettings.Get())
                CommonSettingsVariables.refreshAllSettings.Set(true);
            else
                CommonSettingsVariables.refreshAllSettings.Set(false);
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