using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Toggle Sound Controller", fileName = "ToggleSoundControlController")]
    public class ToggleSoundControlController : SettingsControlController
    {
        public override object GetStoredValue()
        {
            return currentGeneralSettings.sfxVolume > 0 ? true : false;
        }

        public override void OnControlChanged(object newValue)
        {
            base.OnControlChanged(newValue);

            currentGeneralSettings.sfxVolume = (bool)newValue ? 1 : 0;
            Settings.i.ApplyGeneralSettings(currentGeneralSettings);
            Settings.i.SaveSettings();
        }
    }
}