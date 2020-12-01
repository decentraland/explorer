using UnityEngine;
using UnityEngine.UI;

namespace DCL.SettingsPanelHUD.Controls
{
    public class ToggleSettingsControlView : SettingsControlView
    {
        [SerializeField] private Toggle toggle = null;

        public override void Initialize(string title, ISettingsControlController settingsControlController)
        {
            base.Initialize(title, settingsControlController);

            toggle.onValueChanged.AddListener(isOn =>
            {
                settingsControlController.OnControlChanged(isOn);
            });

            toggle.isOn = (bool)settingsControlController.GetStoredValue();
        }
    }
}