using DCL.SettingsController;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.SettingsPanelHUD.Controls
{
    /// <summary>
    /// MonoBehaviour that represents the view of a TOGGLE type CONTROL.
    /// </summary>
    public class ToggleSettingsControlView : SettingsControlView
    {
        [SerializeField] private Toggle toggle;

        public Toggle toggleControl { get => toggle; }

        public override void Initialize(
            SettingsControlModel controlConfig,
            SettingsControlController settingsControlController,
            IGeneralSettingsReferences generalSettingsController,
            IQualitySettingsReferences qualitySettingsController)
        {
            base.Initialize(controlConfig, settingsControlController, generalSettingsController, qualitySettingsController);
            settingsControlController.OnControlChanged(toggle.isOn);

            toggle.onValueChanged.AddListener(isOn =>
            {
                ApplySetting(isOn);
            });
        }

        public override void RefreshControl()
        {
            base.RefreshControl();

            bool newValue = (bool)settingsControlController.GetStoredValue();
            if (toggle.isOn != newValue)
                toggle.isOn = newValue;
        }
    }
}