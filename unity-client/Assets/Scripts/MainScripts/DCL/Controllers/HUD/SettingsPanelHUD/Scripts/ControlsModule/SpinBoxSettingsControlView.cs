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

            spinBox.onValueChanged.AddListener(spinBoxValue =>
            {
                ApplySetting(spinBoxValue);
            });
        }

        public override void RefreshControl()
        {
            int newValue = (int)settingsControlController.GetStoredValue();
            if (spinBox.value != newValue)
                spinBox.value = newValue;
            else
                skipPostApplySettings = false;
        }

        public void SetLabels(string[] labels)
        {
            if (labels.Length == 0)
                return;

            spinBox.SetLabels(labels);
            spinBox.SetValue(0);
        }
    }
}