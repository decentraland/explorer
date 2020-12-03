using UnityEngine;
using UnityEngine.UI;

namespace DCL.SettingsPanelHUD.Controls
{
    public class ToggleSettingsControlView : SettingsControlView
    {
        [SerializeField] private Toggle toggle;
        [SerializeField] private CanvasGroup canvasGroup;

        public Toggle toggleControl { get => toggle; }

        public override void Initialize(SettingsControlModel controlConfig, SettingsControlController settingsControlController)
        {
            base.Initialize(controlConfig, settingsControlController);

            RefreshControl();
            settingsControlController.OnControlChanged(toggle.isOn);

            toggle.onValueChanged.AddListener(isOn =>
            {
                settingsControlController.OnControlChanged(isOn);
                settingsControlController.ApplySettings();
                settingsControlController.PostApplySettings();
            });
        }

        public override void SetEnabled(bool enabled)
        {
            canvasGroup.alpha = enabled ? 1 : 0.5f;
            canvasGroup.interactable = enabled;
        }

        public override void RefreshControl()
        {
            toggle.isOn = (bool)settingsControlController.GetStoredValue();
        }
    }
}