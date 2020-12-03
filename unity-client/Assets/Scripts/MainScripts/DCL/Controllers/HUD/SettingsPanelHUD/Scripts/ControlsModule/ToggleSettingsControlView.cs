using UnityEngine;
using UnityEngine.UI;

namespace DCL.SettingsPanelHUD.Controls
{
    public class ToggleSettingsControlView : SettingsControlView
    {
        [SerializeField] private Toggle toggle;

        public Toggle toggleControl { get => toggle; }

        public override void Initialize(SettingsControlModel controlConfig, SettingsControlController settingsControlController)
        {
            base.Initialize(controlConfig, settingsControlController);

            toggle.isOn = (bool)settingsControlController.GetInitialValue();
            settingsControlController.OnControlChanged(toggle.isOn);

            toggle.onValueChanged.AddListener(isOn =>
            {
                settingsControlController.OnControlChanged(isOn);
                settingsControlController.ApplySettings();
                settingsControlController.PostApplySettings();
            });
        }
    }
}