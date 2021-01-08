using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Allow Voice Chat", fileName = "AllowVoiceChatControlController")]
    public class AllowVoiceChatControlController : SettingsControlController
    {
        const string ALLOW_VOICE_CHAT_SETTINGS_KEY = "Settings.AllowVoiceChat";
        const string VOICE_CHAT_VOLUME_SETTINGS_KEY = "Settings.VoiceChatVolume";

        public override object GetStoredValue()
        {
            int storedValue = PlayerPrefs.GetInt(ALLOW_VOICE_CHAT_SETTINGS_KEY, -1);
            if (storedValue != -1)
                return storedValue;
            else
                return (int)Settings.i.GetDefaultGeneralSettings().voiceChatAllow;
        }

        public override void OnControlChanged(object newValue, bool fromInitialize)
        {
            int newAllowVoiceChatValue = (int)newValue;
            PlayerPrefs.SetInt(ALLOW_VOICE_CHAT_SETTINGS_KEY, newAllowVoiceChatValue);

            float voiceChatVolumeStoredValue = PlayerPrefs.GetFloat(VOICE_CHAT_VOLUME_SETTINGS_KEY, -1);
            Interface.WebInterface.ApplySettings(
                voiceChatVolumeStoredValue != -1 ? voiceChatVolumeStoredValue : Settings.i.GetDefaultGeneralSettings().voiceChatVolume,
                newAllowVoiceChatValue);
        }
    }
}