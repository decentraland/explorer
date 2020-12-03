using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.SettingsPanelHUD.Controls
{
    public class SliderSettingsControlView : SettingsControlView
    {
        [SerializeField] private Slider slider;
        [SerializeField] private TextMeshProUGUI indicatorLabel;
        [SerializeField] private CanvasGroup canvasGroup;

        public Slider sliderControl { get => slider; }

        public override void Initialize(SettingsControlModel controlConfig, SettingsControlController settingsControlController)
        {
            slider.maxValue = ((SliderControlModel)controlConfig).sliderMaxValue;
            slider.minValue = ((SliderControlModel)controlConfig).sliderMinValue;
            slider.wholeNumbers = ((SliderControlModel)controlConfig).sliderWholeNumbers;

            base.Initialize(controlConfig, settingsControlController);

            RefreshControl();
            indicatorLabel.text = slider.value.ToString();
            settingsControlController.OnControlChanged(slider.value);

            slider.onValueChanged.AddListener(sliderValue =>
            {
                indicatorLabel.text = sliderValue.ToString();
                settingsControlController.OnControlChanged(sliderValue);
                settingsControlController.ApplySettings();
                settingsControlController.PostApplySettings();
            });
        }

        public void OverrideIndicatorLabel(string text)
        {
            indicatorLabel.text = text;
        }

        public override void SetEnabled(bool enabled)
        {
            canvasGroup.alpha = enabled ? 1 : 0.5f;
            canvasGroup.interactable = enabled;
        }

        public override void RefreshControl()
        {
            slider.value = (float)settingsControlController.GetStoredValue();
        }
    }
}