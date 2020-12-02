using UnityEngine;
using UnityEngine.UI;

namespace DCL.SettingsPanelHUD.Controls
{
    public class ToggleSettingsControlView : SettingsControlView
    {
        [SerializeField] private Toggle toggle;

        public Toggle spinBoxControl { get => toggle; }

        public override void Initialize(SettingsControlModel controlConfig, SettingsControlController settingsControlController)
        {
            base.Initialize(controlConfig, settingsControlController);

            toggle.onValueChanged.AddListener(isOn =>
            {
                settingsControlController.OnControlChanged(isOn);
            });

            toggle.isOn = (bool)settingsControlController.GetInitialValue();
        }
    }
}