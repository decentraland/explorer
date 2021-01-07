using System;
using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Mute Sound", fileName = "MuteSoundControlController")]
    public class MuteSoundControlController : SettingsControlController
    {
        const string MUTE_SOUND_SETTINGS_KEY = "Settings.MuteSound";

        public override object GetStoredValue()
        {
            string storedValue = PlayerPrefs.GetString(MUTE_SOUND_SETTINGS_KEY);
            if (!String.IsNullOrEmpty(storedValue))
                return Convert.ToBoolean(storedValue);
            else
                return Settings.i.GetDefaultGeneralSettings().sfxVolume > 0;
        }

        public override void OnControlChanged(object newValue)
        {
            bool newMuteSoundValue = (bool)newValue;
            AudioListener.volume = newMuteSoundValue ? 1 : 0;
            PlayerPrefs.SetString(MUTE_SOUND_SETTINGS_KEY, newValue.ToString());
        }
    }
}