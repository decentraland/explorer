using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Mute Audio Controller", fileName = "MuteAudioControlController")]
    public class MuteAudioControlController : SettingsControlController
    {
        public override void Initialize()
        {
            base.Initialize();
        }

        public override object GetStoredValue()
        {
            return currentGeneralSettings.sfxVolume > 0 ? true : false;
        }

        public override void OnControlChanged(object newValue)
        {
            currentGeneralSettings.sfxVolume = (bool)newValue ? 1 : 0;
            Settings.i.ApplyGeneralSettings(currentGeneralSettings);
            Settings.i.SaveSettings();
        }
    }
}