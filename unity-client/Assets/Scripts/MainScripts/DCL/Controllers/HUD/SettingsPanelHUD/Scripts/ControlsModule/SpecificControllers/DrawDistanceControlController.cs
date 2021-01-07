using DCL.SettingsController;
using DCL.SettingsPanelHUD.Common;
using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Draw Distance", fileName = "DrawDistanceControlController")]
    public class DrawDistanceControlController : SettingsControlController
    {
        const string DRAW_DISTANCE_SETTINGS_KEY = "Settings.DrawDistance";

        private QualitySettingsController qualitySettings; // TODO (Santi): Refactorize!

        public override void Initialize(ISettingsControlView settingsControlView)
        {
            base.Initialize(settingsControlView);

            qualitySettings = GameObject.FindObjectOfType<QualitySettingsController>();
        }

        public override object GetStoredValue()
        {
            float storedValue = PlayerPrefs.GetFloat(DRAW_DISTANCE_SETTINGS_KEY, -1);
            if (storedValue != -1)
                return storedValue;
            else
                return Settings.i.qualitySettingsPresets.defaultPreset.cameraDrawDistance;
        }

        public override void OnControlChanged(object newValue)
        {
            float newFloatValue = (float)newValue;

            if (qualitySettings.thirdPersonCamera)
                qualitySettings.thirdPersonCamera.m_Lens.FarClipPlane = newFloatValue;

            if (qualitySettings.firstPersonCamera)
                qualitySettings.firstPersonCamera.m_Lens.FarClipPlane = newFloatValue;

            RenderSettings.fogEndDistance = newFloatValue;
            RenderSettings.fogStartDistance = newFloatValue * 0.8f;

            PlayerPrefs.SetFloat(DRAW_DISTANCE_SETTINGS_KEY, newFloatValue);
        }

        public override void PostApplySettings()
        {
            base.PostApplySettings();

            CommonSettingsEvents.RaiseSetQualityPresetAsCustom();
        }
    }
}