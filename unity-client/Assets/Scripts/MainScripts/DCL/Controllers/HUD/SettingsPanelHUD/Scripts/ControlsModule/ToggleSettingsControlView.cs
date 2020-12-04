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

            settingsControlController.OnControlChanged(toggle.isOn);
            settingsControlController.ApplySettings();

            toggle.onValueChanged.AddListener(isOn =>
            {
                settingsControlController.OnControlChanged(isOn);
                settingsControlController.ApplySettings();

                if (!skipPostApplySettings)
                    settingsControlController.PostApplySettings();
                skipPostApplySettings = false;
            });
        }

        public override void RefreshControl()
        {
            bool newValue = (bool)settingsControlController.GetStoredValue();
            if (toggle.isOn != newValue)
                toggle.isOn = newValue;
            else
                skipPostApplySettings = false;
        }
    }
}