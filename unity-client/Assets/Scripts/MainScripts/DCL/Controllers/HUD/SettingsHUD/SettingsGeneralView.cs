using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace DCL.SettingsHUD
{
    public class SettingsGeneralView : MonoBehaviour
    {
        public const string TEXT_QUALITY_CUSTOM = "CUSTOM";
        public const string TEXT_OFF = "OFF";

        public SpinBoxPresetted qualityPresetSpinBox = null;
        public SpinBoxPresetted textureResSpinBox = null;
        public SpinBoxPresetted shadowResSpinBox = null;
        public Toggle soundToggle = null;
        public Toggle colorGradingToggle = null;
        public Toggle shadowToggle = null;
        public Toggle softShadowToggle = null;
        public Toggle bloomToggle = null;
        public Slider mouseSensitivitySlider = null;
        public Slider antiAliasingSlider = null;
        public Slider renderingScaleSlider = null;
        public Slider drawDistanceSlider = null;
        public TextMeshProUGUI mouseSensitivityValueLabel = null;
        public TextMeshProUGUI antiAliasingValueLabel = null;
        public TextMeshProUGUI renderingScaleValueLabel = null;
        public TextMeshProUGUI drawDistanceValueLabel = null;

        private DCL.SettingsHUD.QualitySettings currentQualitySetting;
        private DCL.SettingsHUD.GeneralSettings currentGeneralSetting;

        private bool shouldSetAsCustom = false;

        void Awake()
        {
            currentQualitySetting = Settings.i.qualitySettings;
            currentGeneralSetting = Settings.i.generalSettings;

            qualityPresetSpinBox.onValueChanged.AddListener(value =>
            {
                DCL.SettingsHUD.QualitySettings preset = Settings.i.qualitySettingsPresets[value];
                currentQualitySetting = preset;
                UpdateQualitySettings();
                shouldSetAsCustom = false;
            });

            textureResSpinBox.onValueChanged.AddListener(value =>
            {
                currentQualitySetting.textureQuality = (DCL.SettingsHUD.QualitySettings.TextureQuality)value;
                shouldSetAsCustom = true;
            });

            shadowResSpinBox.onValueChanged.AddListener(value =>
            {
                currentQualitySetting.shadowResolution = (UnityEngine.Rendering.LWRP.ShadowResolution)(256 << value);
                shouldSetAsCustom = true;
            });

            colorGradingToggle.onValueChanged.AddListener(isOn =>
            {
                currentQualitySetting.colorGrading = isOn;
                shouldSetAsCustom = true;
            });

            soundToggle.onValueChanged.AddListener(isOn =>
            {
                currentGeneralSetting.sfxVolume = isOn ? 1 : 0;
            });

            shadowToggle.onValueChanged.AddListener(isOn =>
            {
                currentQualitySetting.shadows = isOn;
                if (!isOn)
                {
                    softShadowToggle.isOn = false;
                }
                shouldSetAsCustom = true;
            });

            softShadowToggle.onValueChanged.AddListener(isOn =>
            {
                currentQualitySetting.softShadows = isOn;
                shouldSetAsCustom = true;
            });

            bloomToggle.onValueChanged.AddListener(isOn =>
            {
                currentQualitySetting.bloom = isOn;
                shouldSetAsCustom = true;
            });

            mouseSensitivitySlider.onValueChanged.AddListener(value =>
            {
                currentGeneralSetting.mouseSensitivity = value;
                mouseSensitivityValueLabel.text = value.ToString("0.0");
            });

            antiAliasingSlider.onValueChanged.AddListener(value =>
            {
                int antiAliasingValue = 1 << (int)value;
                currentQualitySetting.antiAliasing = (UnityEngine.Rendering.LWRP.MsaaQuality)antiAliasingValue;
                if (value == 0)
                {
                    antiAliasingValueLabel.text = TEXT_OFF;
                }
                else
                {
                    antiAliasingValueLabel.text = antiAliasingValue.ToString("0x");
                }
                shouldSetAsCustom = true;
            });

            renderingScaleSlider.onValueChanged.AddListener(value =>
            {
                currentQualitySetting.renderScale = value;
                renderingScaleValueLabel.text = value.ToString("0.0");
                shouldSetAsCustom = true;
            });

            drawDistanceSlider.onValueChanged.AddListener(value =>
            {
                currentQualitySetting.cameraDrawDistance = value;
                drawDistanceValueLabel.text = value.ToString();
                shouldSetAsCustom = true;
            });

            SetupQualityPreset(currentQualitySetting);
        }

        void Update()
        {
            if (shouldSetAsCustom)
            {
                qualityPresetSpinBox.OverrideCurrentLabel(TEXT_QUALITY_CUSTOM);
                shouldSetAsCustom = false;
            }
        }

        void SetupQualityPreset(DCL.SettingsHUD.QualitySettings savedSetting)
        {
            List<string> presetNames = new List<string>();
            int presetIndex = 0;
            bool presetIndexFound = false;

            DCL.SettingsHUD.QualitySettings preset;
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
                currentQualitySetting = savedSetting;
                UpdateQualitySettings();
            }
            else
            {
                qualityPresetSpinBox.value = presetIndex;
            }
        }

        void UpdateQualitySettings()
        {
            textureResSpinBox.value = (int)currentQualitySetting.textureQuality;
            shadowResSpinBox.value = (int)Mathf.Log((int)currentQualitySetting.shadowResolution, 2) - 8;
            soundToggle.isOn = currentGeneralSetting.sfxVolume > 0 ? true : false;
            colorGradingToggle.isOn = currentQualitySetting.colorGrading;
            softShadowToggle.isOn = currentQualitySetting.softShadows;
            shadowToggle.isOn = currentQualitySetting.shadows;
            bloomToggle.isOn = currentQualitySetting.bloom;
            mouseSensitivitySlider.value = currentGeneralSetting.mouseSensitivity;
            antiAliasingSlider.value = currentQualitySetting.antiAliasing == UnityEngine.Rendering.LWRP.MsaaQuality.Disabled ? 0 : ((int)currentQualitySetting.antiAliasing >> 2) + 1;
            renderingScaleSlider.value = currentQualitySetting.renderScale;
            drawDistanceSlider.value = currentQualitySetting.cameraDrawDistance;
        }

        public void Apply()
        {
            Settings.i.ApplyQualitySettings(currentQualitySetting);
            Settings.i.ApplyGeneralSettings(currentGeneralSetting);
            Settings.i.SaveSettings();
        }
    }
}