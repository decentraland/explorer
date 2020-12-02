using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Voice Chat Volume", fileName = "VoiceChatVolumeControlController")]
    public class VoiceChatVolumeControlController : SettingsControlController
    {
        public override object GetInitialValue()
        {
            return currentGeneralSettings.voiceChatVolume * 100;
        }

        public override void OnControlChanged(object newValue)
        {
            currentGeneralSettings.voiceChatVolume = (float)newValue * 0.01f;

            Settings.i.ApplyGeneralSettings(currentGeneralSettings);
            Settings.i.SaveSettings();
        }
    }
}