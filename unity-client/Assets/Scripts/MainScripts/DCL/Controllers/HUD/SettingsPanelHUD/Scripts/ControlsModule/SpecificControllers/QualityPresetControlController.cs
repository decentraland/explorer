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

        public override object GetInitialValue()
        {
            return currentQualityPresetIndex;
        }

        public override void OnControlChanged(object newValue)
        {
            SettingsData.QualitySettings preset = Settings.i.qualitySettingsPresets[(int)newValue];
            currentQualitySetting = preset;
            UpdateQualitySettings();
            currentQualityPresetIndex = (int)newValue;
            Settings.i.ApplyQualitySettings(currentQualitySetting);
        }

        private void UpdateQualitySettings()
        {
        //    baseResSpinBox.value = (int)tempQualitySetting.baseResolution;
        //    shadowResSpinBox.value = (int)Mathf.Log((int)tempQualitySetting.shadowResolution, 2) - 8;
        //    colorGradingToggle.isOn = tempQualitySetting.colorGrading;
        //    softShadowToggle.isOn = tempQualitySetting.softShadows;
        //    shadowToggle.isOn = tempQualitySetting.shadows;
        //    bloomToggle.isOn = tempQualitySetting.bloom;
        //    fpsCapToggle.isOn = tempQualitySetting.fpsCap;
        //    antiAliasingSlider.value = tempQualitySetting.antiAliasing == UnityEngine.Rendering.Universal.MsaaQuality.Disabled ? 0 : ((int)currentQualitySetting.antiAliasing >> 2) + 1;
        //    renderingScaleSlider.value = tempQualitySetting.renderScale;
        //    drawDistanceSlider.value = tempQualitySetting.cameraDrawDistance;
        //    shadowDistanceSlider.value = tempQualitySetting.shadowDistance;
        //    cullingSlider.value = tempQualitySetting.detailObjectCullingThreshold;
        //    cullingToggle.isOn = tempQualitySetting.enableDetailObjectCulling;
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