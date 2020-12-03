using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    public class SpinBoxSettingsControlView : SettingsControlView
    {
        [SerializeField] private SpinBoxPresetted spinBox;

        public SpinBoxPresetted spinBoxControl { get => spinBox; }

        public override void Initialize(SettingsControlModel controlConfig, SettingsControlController settingsControlController)
        {
            SetLabels(((SpinBoxControlModel)controlConfig).spinBoxLabels);

            base.Initialize(controlConfig, settingsControlController);

            spinBox.value = (int)settingsControlController.GetInitialValue();
            settingsControlController.OnControlChanged(spinBox.value);

            spinBox.onValueChanged.AddListener(sliderValue =>
            {
                settingsControlController.OnControlChanged(sliderValue);
                settingsControlController.ApplySettings();
                settingsControlController.PostApplySettings();
            });
        }

        public void SetLabels(string[] labels)
        {
            spinBox.SetLabels(labels);
        }
    }
}