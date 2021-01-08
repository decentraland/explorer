using DCL.SettingsPanelHUD.Common;
using System;
using System.Collections.Generic;
using UnityEngine;
using static DCL.SettingsData.QualitySettings;

namespace DCL.SettingsPanelHUD.Controls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Quality Preset", fileName = "QualityPresetControlController")]
    public class QualityPresetControlController : SettingsControlController
    {
        const string TEXT_QUALITY_CUSTOM = "Custom";
        const string ANTI_ALIASING_SETTINGS_KEY = "Settings.AntiAliasing";
        const string BASE_RESOLUTION_SETTINGS_KEY = "Settings.BaseResolution";
        const string BLOOM_SETTINGS_KEY = "Settings.Bloom";
        const string COLOR_GRADING_SETTINGS_KEY = "Settings.ColorGrading";
        const string DETAIL_OBJECT_CULLING_SETTINGS_KEY = "Settings.DetailObjectCulling";
        const string DETAIL_OBJECT_CULLING_SIZE_SETTINGS_KEY = "Settings.DetailObjectCullingSize";
        const string DRAW_DISTANCE_SETTINGS_KEY = "Settings.DrawDistance";
        const string FPS_LIMIT_SETTINGS_KEY = "Settings.FPSLimit";
        const string RENDERING_SCALE_SETTINGS_KEY = "Settings.RenderingScale";
        const string SHADOWS_SETTINGS_KEY = "Settings.Shadows";
        const string SHADOW_DISTANCE_SETTINGS_KEY = "Settings.ShadowDistance";
        const string SHADOW_RESOLUTION_SETTINGS_KEY = "Settings.ShadowResolution";
        const string SOFT_SHADOWS_SETTINGS_KEY = "Settings.SoftShadows";

        private SpinBoxSettingsControlView qualityPresetControlView;

        public override void Initialize(ISettingsControlView settingsControlView)
        {
            base.Initialize(settingsControlView);

            qualityPresetControlView = (SpinBoxSettingsControlView)settingsControlView;

            SetupQualityPresetLabels();

            CommonSettingsEvents.OnSetQualityPresetAsCustom += CommonSettingsEvents_OnSetQualityPresetAsCustom;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            CommonSettingsEvents.OnSetQualityPresetAsCustom -= CommonSettingsEvents_OnSetQualityPresetAsCustom;
        }

        public override object GetStoredValue()
        {
            return GetCurrentStoredValue();
        }

        public override void OnControlChanged(object newValue, bool fromInitialize)
        {
            if (fromInitialize)
                return;

            SettingsData.QualitySettings preset = Settings.i.qualitySettingsPresets[(int)newValue];

            PlayerPrefs.SetInt(ANTI_ALIASING_SETTINGS_KEY, (int)preset.antiAliasing);
            PlayerPrefs.SetInt(BASE_RESOLUTION_SETTINGS_KEY, (int)preset.baseResolution);
            PlayerPrefs.SetString(BLOOM_SETTINGS_KEY, preset.bloom.ToString());
            PlayerPrefs.SetString(COLOR_GRADING_SETTINGS_KEY, preset.colorGrading.ToString());
            PlayerPrefs.SetString(DETAIL_OBJECT_CULLING_SETTINGS_KEY, preset.enableDetailObjectCulling.ToString());
            PlayerPrefs.SetFloat(DETAIL_OBJECT_CULLING_SIZE_SETTINGS_KEY, preset.detailObjectCullingThreshold);
            PlayerPrefs.SetFloat(DRAW_DISTANCE_SETTINGS_KEY, preset.cameraDrawDistance);
            PlayerPrefs.SetString(FPS_LIMIT_SETTINGS_KEY, preset.fpsCap.ToString());
            PlayerPrefs.SetFloat(RENDERING_SCALE_SETTINGS_KEY, preset.renderScale);
            PlayerPrefs.SetString(SHADOWS_SETTINGS_KEY, preset.shadows.ToString());
            PlayerPrefs.SetFloat(SHADOW_DISTANCE_SETTINGS_KEY, preset.shadowDistance);
            PlayerPrefs.SetInt(SHADOW_RESOLUTION_SETTINGS_KEY, (int)preset.shadowResolution);
            PlayerPrefs.SetString(SOFT_SHADOWS_SETTINGS_KEY, preset.softShadows.ToString());
        }

        public override void PostApplySettings()
        {
            base.PostApplySettings();

            CommonSettingsEvents.RaiseRefreshAllSettings(this);
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
            SettingsData.QualitySettings currentSettings = GetCurrentQualitySettings();

            SettingsData.QualitySettings preset;
            for (int i = 0; i < Settings.i.qualitySettingsPresets.Length; i++)
            {
                preset = Settings.i.qualitySettingsPresets[i];
                if (preset.Equals(currentSettings))
                {
                    qualityPresetControlView.spinBoxControl.OverrideCurrentLabel(preset.displayName);
                    return i;
                }
            }

            qualityPresetControlView.spinBoxControl.OverrideCurrentLabel(TEXT_QUALITY_CUSTOM);
            return qualityPresetControlView.spinBoxControl.value;
        }

        private SettingsData.QualitySettings GetCurrentQualitySettings()
        {
            SettingsData.QualitySettings currentSettings = new SettingsData.QualitySettings();
            currentSettings.antiAliasing = (UnityEngine.Rendering.Universal.MsaaQuality)PlayerPrefs.GetInt(ANTI_ALIASING_SETTINGS_KEY, (int)Settings.i.qualitySettingsPresets.defaultPreset.antiAliasing);
            currentSettings.baseResolution = (BaseResolution)PlayerPrefs.GetInt(BASE_RESOLUTION_SETTINGS_KEY, (int)Settings.i.qualitySettingsPresets.defaultPreset.baseResolution);
            currentSettings.bloom = Convert.ToBoolean(PlayerPrefs.GetString(BLOOM_SETTINGS_KEY, Settings.i.qualitySettingsPresets.defaultPreset.bloom.ToString()));
            currentSettings.colorGrading = Convert.ToBoolean(PlayerPrefs.GetString(COLOR_GRADING_SETTINGS_KEY, Settings.i.qualitySettingsPresets.defaultPreset.colorGrading.ToString()));
            currentSettings.enableDetailObjectCulling = Convert.ToBoolean(PlayerPrefs.GetString(DETAIL_OBJECT_CULLING_SETTINGS_KEY, Settings.i.qualitySettingsPresets.defaultPreset.enableDetailObjectCulling.ToString()));
            currentSettings.detailObjectCullingThreshold = PlayerPrefs.GetFloat(DETAIL_OBJECT_CULLING_SIZE_SETTINGS_KEY, Settings.i.qualitySettingsPresets.defaultPreset.detailObjectCullingThreshold);
            currentSettings.cameraDrawDistance = PlayerPrefs.GetFloat(DRAW_DISTANCE_SETTINGS_KEY, Settings.i.qualitySettingsPresets.defaultPreset.cameraDrawDistance);
            currentSettings.fpsCap = Convert.ToBoolean(PlayerPrefs.GetString(FPS_LIMIT_SETTINGS_KEY, Settings.i.qualitySettingsPresets.defaultPreset.fpsCap.ToString()));
            currentSettings.renderScale = PlayerPrefs.GetFloat(RENDERING_SCALE_SETTINGS_KEY, Settings.i.qualitySettingsPresets.defaultPreset.renderScale);
            currentSettings.shadows = Convert.ToBoolean(PlayerPrefs.GetString(SHADOWS_SETTINGS_KEY, Settings.i.qualitySettingsPresets.defaultPreset.shadows.ToString()));
            currentSettings.shadowDistance = PlayerPrefs.GetFloat(SHADOW_DISTANCE_SETTINGS_KEY, Settings.i.qualitySettingsPresets.defaultPreset.shadowDistance);
            currentSettings.shadowResolution = (UnityEngine.Rendering.Universal.ShadowResolution)PlayerPrefs.GetInt(SHADOW_RESOLUTION_SETTINGS_KEY, (int)Settings.i.qualitySettingsPresets.defaultPreset.shadowResolution);
            currentSettings.softShadows = Convert.ToBoolean(PlayerPrefs.GetString(SOFT_SHADOWS_SETTINGS_KEY, Settings.i.qualitySettingsPresets.defaultPreset.softShadows.ToString()));

            return currentSettings;
        }

        private void CommonSettingsEvents_OnSetQualityPresetAsCustom()
        {
            qualityPresetControlView.spinBoxControl.OverrideCurrentLabel(TEXT_QUALITY_CUSTOM);
        }
    }
}