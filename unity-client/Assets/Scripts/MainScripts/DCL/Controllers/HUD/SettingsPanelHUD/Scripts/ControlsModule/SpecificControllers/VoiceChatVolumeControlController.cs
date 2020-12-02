using DCL.SettingsData;
using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Voice Chat Volume", fileName = "VoiceChatVolumeControlController")]
    public class VoiceChatVolumeControlController : SettingsControlController
    {
        private GeneralSettings currentGeneralSettings;

        public override void Initialize()
        {
            currentGeneralSettings = Settings.i.generalSettings;
        }

        public override object GetStoredValue()
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