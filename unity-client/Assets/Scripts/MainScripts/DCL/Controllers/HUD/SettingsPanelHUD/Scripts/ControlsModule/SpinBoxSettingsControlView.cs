using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    public class SpinBoxSettingsControlView : SettingsControlView
    {
        [SerializeField] private SpinBoxPresetted spinBox;
        [SerializeField] private CanvasGroup canvasGroup;

        public SpinBoxPresetted spinBoxControl { get => spinBox; }

        public override void Initialize(SettingsControlModel controlConfig, SettingsControlController settingsControlController)
        {
            SetLabels(((SpinBoxControlModel)controlConfig).spinBoxLabels);

            base.Initialize(controlConfig, settingsControlController);

            RefreshControl();
            settingsControlController.OnControlChanged(spinBox.value);

            spinBox.onValueChanged.AddListener(sliderValue =>
            {
                settingsControlController.OnControlChanged(sliderValue);
                settingsControlController.ApplySettings();
                if (!skipPostApplySettings)
                    settingsControlController.PostApplySettings();
                skipPostApplySettings = false;
            });
        }

        public override void SetEnabled(bool enabled)
        {
            canvasGroup.alpha = enabled ? 1 : 0.5f;
            canvasGroup.interactable = enabled;
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
            spinBox.SetLabels(labels);
        }
    }
}