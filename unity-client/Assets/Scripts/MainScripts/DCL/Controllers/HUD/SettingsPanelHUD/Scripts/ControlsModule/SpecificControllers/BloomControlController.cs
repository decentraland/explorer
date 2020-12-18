using DCL.SettingsPanelHUD.Common;
using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DCL.SettingsPanelHUD.Controls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Bloom", fileName = "BloomControlController")]
    public class BloomControlController : SettingsControlController
    {
        const string BLOOM_SETTINGS_KEY = "Settings.Bloom";

        private Volume postProcessVolume = null;

        public override void Initialize(ISettingsControlView settingsControlView)
        {
            base.Initialize(settingsControlView);

            postProcessVolume = GameObject.FindObjectOfType<Volume>();
        }

        public override object GetStoredValue()
        {
            string storedValue = PlayerPrefs.GetString(BLOOM_SETTINGS_KEY);
            if (!String.IsNullOrEmpty(storedValue))
                return Convert.ToBoolean(storedValue);
            else
                return Settings.i.qualitySettingsPresets.defaultPreset.bloom;
        }

        public override void OnControlChanged(object newValue)
        {
            bool newBloomValue = (bool)newValue;

            if (postProcessVolume)
            {
                Bloom bloom;
                if (postProcessVolume.profile.TryGet<Bloom>(out bloom))
                {
                    bloom.active = newBloomValue;
                }
            }

            PlayerPrefs.SetString(BLOOM_SETTINGS_KEY, newBloomValue.ToString());
        }

        public override void PostApplySettings()
        {
            base.PostApplySettings();

            CommonSettingsEvents.RaiseSetQualityPresetAsCustom();
        }
    }
}