using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Voice Chat Volume", fileName = "VoiceChatVolumeControlController")]
    public class VoiceChatVolumeControlController : SettingsControlController
    {
        const string VOICE_CHAT_VOLUME_SETTINGS_KEY = "Settings.VoiceChatVolume";
        const string ALLOW_VOICE_CHAT_SETTINGS_KEY = "Settings.AllowVoiceChat";

        public override object GetStoredValue()
        {
            float storedValue = PlayerPrefs.GetFloat(VOICE_CHAT_VOLUME_SETTINGS_KEY, -1);
            if (storedValue != -1)
                return storedValue * 100;
            else
                return Settings.i.GetDefaultGeneralSettings().voiceChatVolume * 100;
        }

        public override void OnControlChanged(object newValue)
        {
            float newVoiceChatVolumeValue = (float)newValue * 0.01f;
            PlayerPrefs.SetFloat(VOICE_CHAT_VOLUME_SETTINGS_KEY, newVoiceChatVolumeValue);

            int allowVoiceChatStoredValue = PlayerPrefs.GetInt(ALLOW_VOICE_CHAT_SETTINGS_KEY, -1);
            Interface.WebInterface.ApplySettings(
                newVoiceChatVolumeValue,
                allowVoiceChatStoredValue != -1 ? allowVoiceChatStoredValue : (int)Settings.i.GetDefaultGeneralSettings().voiceChatAllow);
        }
    }
}