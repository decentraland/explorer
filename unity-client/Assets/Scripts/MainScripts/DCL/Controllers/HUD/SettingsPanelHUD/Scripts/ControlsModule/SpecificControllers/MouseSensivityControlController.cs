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
            ((SliderSettingsControlView)view).OverrideIndicatorLabel(((float)newValue).ToString("0.0"));

            Settings.i.ApplyGeneralSettings(currentGeneralSettings);
        }
    }
}