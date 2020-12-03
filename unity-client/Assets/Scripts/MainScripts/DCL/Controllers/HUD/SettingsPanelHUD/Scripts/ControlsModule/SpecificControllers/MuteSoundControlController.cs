using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Mute Sound", fileName = "MuteSoundControlController")]
    public class MuteSoundControlController : SettingsControlController
    {
        public override object GetInitialValue()
        {
            return currentGeneralSettings.sfxVolume > 0 ? true : false;
        }

        public override void OnControlChanged(object newValue)
        {
            currentGeneralSettings.sfxVolume = (bool)newValue ? 1 : 0;
            Settings.i.ApplyGeneralSettings(currentGeneralSettings);
        }
    }
}