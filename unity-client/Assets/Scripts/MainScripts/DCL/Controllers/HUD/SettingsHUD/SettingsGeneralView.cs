﻿using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using QualitySettings = DCL.SettingsData.QualitySettings;

namespace DCL.SettingsHUD
{
    public class SettingsGeneralView : MonoBehaviour
    {
        public const string TEXT_QUALITY_CUSTOM = "Custom";
        public const string TEXT_OFF = "OFF";

        public SpinBoxPresetted qualityPresetSpinBox = null;
        public SpinBoxPresetted baseResSpinBox = null;
        public SpinBoxPresetted shadowResSpinBox = null;
        public Toggle soundToggle = null;
        public Toggle colorGradingToggle = null;
        public Toggle shadowToggle = null;
        public Toggle softShadowToggle = null;
        public Toggle bloomToggle = null;
        public Toggle fpsCapToggle = null;
        public Slider mouseSensitivitySlider = null;
        public Slider antiAliasingSlider = null;
        public Slider renderingScaleSlider = null;
        public Slider drawDistanceSlider = null;
        public Slider shadowDistanceSlider = null;
        public TextMeshProUGUI mouseSensitivityValueLabel = null;
        public TextMeshProUGUI antiAliasingValueLabel = null;
        public TextMeshProUGUI renderingScaleValueLabel = null;
        public TextMeshProUGUI drawDistanceValueLabel = null;
        public TextMeshProUGUI shadowDistanceValueLabel = null;
        public Slider voiceChatVolumeSlider = null;
        public TextMeshProUGUI voiceChatVolumeValueLabel = null;
        public SpinBoxPresetted voiceChatAllowSpinBox = null;

        public Toggle cullingToggle = null;

        public Slider cullingSlider = null;
        public TextMeshProUGUI cullingSliderValueLabel = null;

        [SerializeField] private Toggle autosettingsToggle;
        [SerializeField] private CanvasGroup advancedCanvasGroup;
        [SerializeField] private GameObject advancedBlocker;

        private DCL.SettingsData.QualitySettings currentQualitySetting;
        private DCL.SettingsData.GeneralSettings currentGeneralSetting;

        private DCL.SettingsData.QualitySettings tempQualitySetting;
        private DCL.SettingsData.GeneralSettings tempGeneralSetting;

        private bool shouldSetAsCustom = false;
        private bool isDirty = false;

        void Awake()
        {
            qualityPresetSpinBox.onValueChanged.AddListener(value =>
            {
                DCL.SettingsData.QualitySettings preset = Settings.i.qualitySettingsPresets[value];
                tempQualitySetting = preset;
                UpdateQualitySettings();
                shouldSetAsCustom = false;
            });

            baseResSpinBox.onValueChanged.AddListener(value =>
            {
                tempQualitySetting.baseResolution = (DCL.SettingsData.QualitySettings.BaseResolution) value;
                shouldSetAsCustom = true;
                isDirty = true;
            });

            shadowResSpinBox.onValueChanged.AddListener(value =>
            {
                tempQualitySetting.shadowResolution = (UnityEngine.Rendering.Universal.ShadowResolution) (256 << value);
                shouldSetAsCustom = true;
                isDirty = true;
            });

            colorGradingToggle.onValueChanged.AddListener(isOn =>
            {
                tempQualitySetting.colorGrading = isOn;
                shouldSetAsCustom = true;
                isDirty = true;
            });

            soundToggle.onValueChanged.AddListener(isOn =>
            {
                tempGeneralSetting.sfxVolume = isOn ? 1 : 0;
                isDirty = true;
            });

            shadowToggle.onValueChanged.AddListener(isOn =>
            {
                tempQualitySetting.shadows = isOn;
                if (!isOn)
                {
                    softShadowToggle.isOn = false;
                }

                shouldSetAsCustom = true;
                isDirty = true;
            });

            softShadowToggle.onValueChanged.AddListener(isOn =>
            {
                tempQualitySetting.softShadows = isOn;
                shouldSetAsCustom = true;
                isDirty = true;
            });

            bloomToggle.onValueChanged.AddListener(isOn =>
            {
                tempQualitySetting.bloom = isOn;
                shouldSetAsCustom = true;
                isDirty = true;
            });

            fpsCapToggle.onValueChanged.AddListener(isOn =>
            {
                tempQualitySetting.fpsCap = isOn;
                shouldSetAsCustom = true;
                isDirty = true;
            });

            mouseSensitivitySlider.onValueChanged.AddListener(value =>
            {
                tempGeneralSetting.mouseSensitivity = value;
                mouseSensitivityValueLabel.text = value.ToString("0.0");
                isDirty = true;
            });

            antiAliasingSlider.onValueChanged.AddListener(value =>
            {
                int antiAliasingValue = 1 << (int) value;
                tempQualitySetting.antiAliasing = (UnityEngine.Rendering.Universal.MsaaQuality) antiAliasingValue;
                if (value == 0)
                {
                    antiAliasingValueLabel.text = TEXT_OFF;
                }
                else
                {
                    antiAliasingValueLabel.text = antiAliasingValue.ToString("0x");
                }

                shouldSetAsCustom = true;
                isDirty = true;
            });

            renderingScaleSlider.onValueChanged.AddListener(value =>
            {
                tempQualitySetting.renderScale = value;
                renderingScaleValueLabel.text = value.ToString("0.0");
                shouldSetAsCustom = true;
                isDirty = true;
            });

            drawDistanceSlider.onValueChanged.AddListener(value =>
            {
                tempQualitySetting.cameraDrawDistance = value;
                drawDistanceValueLabel.text = value.ToString();
                shouldSetAsCustom = true;
                isDirty = true;
            });

            shadowDistanceSlider.onValueChanged.AddListener(value =>
            {
                tempQualitySetting.shadowDistance = value;
                shadowDistanceValueLabel.text = value.ToString();
                shouldSetAsCustom = true;
                isDirty = true;
            });

            voiceChatVolumeSlider.onValueChanged.AddListener(value =>
            {
                tempGeneralSetting.voiceChatVolume = value * 0.01f;
                voiceChatVolumeValueLabel.text = value.ToString();
                isDirty = true;
            });

            voiceChatAllowSpinBox.onValueChanged.AddListener(value =>
            {
                tempGeneralSetting.voiceChatAllow = (DCL.SettingsData.GeneralSettings.VoiceChatAllow) value;
                isDirty = true;
            });

            cullingToggle.onValueChanged.AddListener(value =>
                {
                    tempQualitySetting.enableDetailObjectCulling = value;
                    cullingSlider.enabled = value;
                    isDirty = true;
                }
            );

            cullingSlider.onValueChanged.AddListener(value =>
            {
                tempQualitySetting.detailObjectCullingThreshold = value;
                cullingSliderValueLabel.text = value.ToString();
                isDirty = true;
            });

            autosettingsToggle.onValueChanged.AddListener(SetAutoQualityActive);
            autosettingsToggle.isOn = false;
        }

        private void SetAutoQualityActive(bool active)
        {
            advancedCanvasGroup.interactable = !active;
            tempGeneralSetting.autoqualityOn = active;
            advancedBlocker.SetActive(active);
            if (active)
            {
                QualitySettings.BaseResolution currentBaseResolution = tempQualitySetting.baseResolution;
                tempQualitySetting = Settings.i.lastValidAutoqualitySet;
                tempQualitySetting.baseResolution = currentBaseResolution;
                isDirty = true;
            }
        }

        void OnEnable()
        {
            currentQualitySetting = Settings.i.qualitySettings;
            currentGeneralSetting = Settings.i.generalSettings;

            tempQualitySetting = currentQualitySetting;
            tempGeneralSetting = currentGeneralSetting;

            SetupQualityPreset(currentQualitySetting);
            UpdateGeneralSettings();
        }

        void Update()
        {
            if (shouldSetAsCustom)
            {
                qualityPresetSpinBox.OverrideCurrentLabel(TEXT_QUALITY_CUSTOM);
                shouldSetAsCustom = false;
            }

            if (isDirty)
            {
                Settings.i.ApplyQualitySettings(tempQualitySetting);
                Settings.i.ApplyGeneralSettings(tempGeneralSetting);
                isDirty = false;
            }
        }

        void SetupQualityPreset(DCL.SettingsData.QualitySettings savedSetting)
        {
            List<string> presetNames = new List<string>();
            int presetIndex = 0;
            bool presetIndexFound = false;

            DCL.SettingsData.QualitySettings preset;

            for (int i = 0; i < Settings.i.qualitySettingsPresets.Length; i++)
            {
                preset = Settings.i.qualitySettingsPresets[i];
                presetNames.Add(preset.displayName);

                if (!presetIndexFound && preset.Equals(savedSetting))
                {
                    presetIndexFound = true;
                    presetIndex = i;
                }
            }

            qualityPresetSpinBox.SetLabels(presetNames.ToArray());

            if (!presetIndexFound)
            {
                tempQualitySetting = savedSetting;
                UpdateQualitySettings();
            }
            else
            {
                qualityPresetSpinBox.value = presetIndex;
            }
        }

        void UpdateQualitySettings()
        {
            baseResSpinBox.value = (int) tempQualitySetting.baseResolution;
            shadowResSpinBox.value = (int) Mathf.Log((int) tempQualitySetting.shadowResolution, 2) - 8;
            colorGradingToggle.isOn = tempQualitySetting.colorGrading;
            softShadowToggle.isOn = tempQualitySetting.softShadows;
            shadowToggle.isOn = tempQualitySetting.shadows;
            bloomToggle.isOn = tempQualitySetting.bloom;
            fpsCapToggle.isOn = tempQualitySetting.fpsCap;
            antiAliasingSlider.value = tempQualitySetting.antiAliasing == UnityEngine.Rendering.Universal.MsaaQuality.Disabled ? 0 : ((int)currentQualitySetting.antiAliasing >> 2) + 1;
            renderingScaleSlider.value = tempQualitySetting.renderScale;
            drawDistanceSlider.value = tempQualitySetting.cameraDrawDistance;
            shadowDistanceSlider.value = tempQualitySetting.shadowDistance;
        }

        void UpdateGeneralSettings()
        {
            soundToggle.isOn = tempGeneralSetting.sfxVolume > 0 ? true : false;
            mouseSensitivitySlider.value = tempGeneralSetting.mouseSensitivity;
            voiceChatVolumeSlider.value = tempGeneralSetting.voiceChatVolume * 100;
            voiceChatAllowSpinBox.value = (int) tempGeneralSetting.voiceChatAllow;
            autosettingsToggle.isOn = tempGeneralSetting.autoqualityOn;
        }

        public void Apply()
        {
            Settings.i.ApplyQualitySettings(tempQualitySetting);
            Settings.i.ApplyGeneralSettings(tempGeneralSetting);
            Settings.i.SaveSettings();
        }

        public void OnDismiss()
        {
            Settings.i.ApplyQualitySettings(currentQualitySetting);
            Settings.i.ApplyGeneralSettings(currentGeneralSetting);
        }
    }
}