using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Mouse Sensivity", fileName = "MouseSensivityControlController")]
    public class MouseSensivityControlController : SettingsControlController
    {
        public override object GetInitialValue()
        {
            return currentGeneralSettings.mouseSensitivity;
        }

        public override void OnControlChanged(object newValue)
        {
            currentGeneralSettings.mouseSensitivity = (float)newValue;

            Settings.i.ApplyGeneralSettings(currentGeneralSettings);
            Settings.i.SaveSettings();
        }
    }
}