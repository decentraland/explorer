using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Mute Sound", fileName = "MuteSoundControlController")]
    public class MuteSoundControlController : SettingsControlController
    {
        public override object GetStoredValue()
        {
            return currentGeneralSettings.sfxVolume > 0 ? true : false;
        }

        public override void OnControlChanged(object newValue)
        {
            float newFloatValue = (bool)newValue ? 1f : 0f;

            currentGeneralSettings.sfxVolume = newFloatValue;
            generalSettingsController.UpdateSfxVolume(newFloatValue);
        }
    }
}