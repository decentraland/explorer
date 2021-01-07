using DCL.Rendering;
using DCL.SettingsController;
using System;
using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Detail Object Culling Size", fileName = "DetailObjectCullingSizeControlController")]
    public class DetailObjectCullingSizeControlController : SettingsControlController
    {
        const string DETAIL_OBJECT_CULLING_SIZE_SETTINGS_KEY = "Settings.DetailObjectCullingSize";
        const string DETAIL_OBJECT_CULLING_SETTINGS_KEY = "Settings.DetailObjectCulling";

        private QualitySettingsController qualitySettings; // TODO (Santi): Refactorize!

        public override void Initialize(ISettingsControlView settingsControlView)
        {
            base.Initialize(settingsControlView);

            qualitySettings = GameObject.FindObjectOfType<QualitySettingsController>();
        }

        public override object GetStoredValue()
        {
            float storedValue = PlayerPrefs.GetFloat(DETAIL_OBJECT_CULLING_SIZE_SETTINGS_KEY, -1);
            if (storedValue != -1)
                return storedValue;
            else
                return Settings.i.qualitySettingsPresets.defaultPreset.detailObjectCullingThreshold;
        }

        public override void OnControlChanged(object newValue)
        {
            float newFloatValue = (float)newValue;

            string detailObjectCullingStoredValue = PlayerPrefs.GetString(DETAIL_OBJECT_CULLING_SETTINGS_KEY);
            bool detailObjectCullingIsEnabled;
            if (!string.IsNullOrEmpty(detailObjectCullingStoredValue))
                detailObjectCullingIsEnabled = Convert.ToBoolean(detailObjectCullingStoredValue);
            else
                detailObjectCullingIsEnabled = Settings.i.qualitySettingsPresets.defaultPreset.enableDetailObjectCulling;

            if (detailObjectCullingIsEnabled)
            {
                var settings = Environment.i.platform.cullingController.GetSettingsCopy();

                settings.rendererProfile = CullingControllerProfile.Lerp(
                    qualitySettings.cullingControllerSettingsData.rendererProfileMin,
                    qualitySettings.cullingControllerSettingsData.rendererProfileMax,
                    newFloatValue / 100.0f);

                settings.skinnedRendererProfile = CullingControllerProfile.Lerp(
                    qualitySettings.cullingControllerSettingsData.skinnedRendererProfileMin,
                    qualitySettings.cullingControllerSettingsData.skinnedRendererProfileMax,
                    newFloatValue / 100.0f);

                Environment.i.platform.cullingController.SetSettings(settings);
                PlayerPrefs.SetFloat(DETAIL_OBJECT_CULLING_SIZE_SETTINGS_KEY, newFloatValue);
            }
        }
    }
}