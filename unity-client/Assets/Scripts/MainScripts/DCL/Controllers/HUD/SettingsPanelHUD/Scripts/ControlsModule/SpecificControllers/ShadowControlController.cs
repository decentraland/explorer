using DCL.SettingsController;
using DCL.SettingsPanelHUD.Common;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DCL.SettingsPanelHUD.Controls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Shadow", fileName = "ShadowControlController")]
    public class ShadowControlController : SettingsControlController
    {
        const string SHADOWS_SETTINGS_KEY = "Settings.Shadows";

        private QualitySettingsController qualitySettings; // TODO (Santi): Refactorize!
        private UniversalRenderPipelineAsset lightweightRenderPipelineAsset = null;
        private FieldInfo lwrpaShadowField = null;

        public override void Initialize(ISettingsControlView settingsControlView)
        {
            base.Initialize(settingsControlView);

            qualitySettings = GameObject.FindObjectOfType<QualitySettingsController>();

            if (lightweightRenderPipelineAsset == null)
            {
                lightweightRenderPipelineAsset = GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset;

                if (lightweightRenderPipelineAsset == null) return;

                lwrpaShadowField = lightweightRenderPipelineAsset.GetType().GetField("m_MainLightShadowsSupported", BindingFlags.NonPublic | BindingFlags.Instance);
            }
        }

        public override object GetStoredValue()
        {
            string storedValue = PlayerPrefs.GetString(SHADOWS_SETTINGS_KEY);
            if (!String.IsNullOrEmpty(storedValue))
                return Convert.ToBoolean(storedValue);
            else
                return Settings.i.qualitySettingsPresets.defaultPreset.shadows;
        }

        public override void OnControlChanged(object newValue, bool fromInitialize)
        {
            bool newBoolValue = (bool)newValue;

            if (lightweightRenderPipelineAsset != null)
                lwrpaShadowField?.SetValue(lightweightRenderPipelineAsset, newBoolValue);

            if (qualitySettings.environmentLight)
            {
                LightShadows shadowType = LightShadows.None;

                if (newBoolValue)
                    shadowType = newBoolValue ? LightShadows.Soft : LightShadows.Hard;

                qualitySettings.environmentLight.shadows = shadowType;
            }

            CommonSettingsScriptableObjects.shadowsDisabled.Set(!newBoolValue);
            PlayerPrefs.SetString(SHADOWS_SETTINGS_KEY, newBoolValue.ToString());
        }

        public override void PostApplySettings()
        {
            base.PostApplySettings();

            CommonSettingsEvents.RaiseSetQualityPresetAsCustom();
        }
    }
}