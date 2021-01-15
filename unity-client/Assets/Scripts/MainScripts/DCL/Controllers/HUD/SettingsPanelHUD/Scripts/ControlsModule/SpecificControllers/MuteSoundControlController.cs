using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Mute Sound", fileName = "MuteSoundControlController")]
    public class MuteSoundControlController : ToggleSettingsControlController
    {
        public override object GetStoredValue()
        {
            return currentGeneralSettings.sfxVolume > 0 ? true : false;
        }

        public override void OnControlChanged(object newValue)
        {
            bool newBoolValue = (bool)newValue;
            currentGeneralSettings.sfxVolume = newBoolValue ? 1 : 0;
            AudioListener.volume = currentGeneralSettings.sfxVolume;
        }
    }
}