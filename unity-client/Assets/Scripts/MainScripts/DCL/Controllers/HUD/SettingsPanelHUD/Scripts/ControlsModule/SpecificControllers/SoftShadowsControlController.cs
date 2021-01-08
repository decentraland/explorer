using DCL.SettingsController;
using DCL.SettingsPanelHUD.Common;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DCL.SettingsPanelHUD.Controls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/SoftShadows", fileName = "SoftShadowsControlController")]
    public class SoftShadowsControlController : SettingsControlController
    {
        const string SOFT_SHADOWS_SETTINGS_KEY = "Settings.SoftShadows";
        const string SHADOWS_SETTINGS_KEY = "Settings.Shadows";

        private QualitySettingsController qualitySettings; // TODO (Santi): Refactorize!
        private UniversalRenderPipelineAsset lightweightRenderPipelineAsset = null;
        private FieldInfo lwrpaSoftShadowField = null;

        public override void Initialize(ISettingsControlView settingsControlView)
        {
            base.Initialize(settingsControlView);

            qualitySettings = GameObject.FindObjectOfType<QualitySettingsController>();

            if (lightweightRenderPipelineAsset == null)
            {
                lightweightRenderPipelineAsset = GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset;

                if (lightweightRenderPipelineAsset == null) return;

                lwrpaSoftShadowField = lightweightRenderPipelineAsset.GetType().GetField("m_SoftShadowsSupported", BindingFlags.NonPublic | BindingFlags.Instance);
            }
        }

        public override object GetStoredValue()
        {
            string storedValue = PlayerPrefs.GetString(SOFT_SHADOWS_SETTINGS_KEY);
            if (!String.IsNullOrEmpty(storedValue))
                return Convert.ToBoolean(storedValue);
            else
                return Settings.i.qualitySettingsPresets.defaultPreset.softShadows;
        }

        public override void OnControlChanged(object newValue, bool fromInitialize)
        {
            bool newBoolValue = (bool)newValue;

            if (lightweightRenderPipelineAsset != null)
                lwrpaSoftShadowField?.SetValue(lightweightRenderPipelineAsset, newBoolValue);

            if (qualitySettings.environmentLight)
            {
                LightShadows shadowType = LightShadows.None;

                string shadowsStoredValue = PlayerPrefs.GetString(SHADOWS_SETTINGS_KEY);
                bool shadowsAreEnabled;
                if (!string.IsNullOrEmpty(shadowsStoredValue))
                    shadowsAreEnabled = Convert.ToBoolean(shadowsStoredValue);
                else
                    shadowsAreEnabled = Settings.i.qualitySettingsPresets.defaultPreset.enableDetailObjectCulling;

                if (shadowsAreEnabled)
                    shadowType = newBoolValue ? LightShadows.Soft : LightShadows.Hard;

                qualitySettings.environmentLight.shadows = shadowType;
            }

            PlayerPrefs.SetString(SOFT_SHADOWS_SETTINGS_KEY, newBoolValue.ToString());
        }

        public override void PostApplySettings()
        {
            base.PostApplySettings();

            CommonSettingsEvents.RaiseSetQualityPresetAsCustom();
        }
    }
}