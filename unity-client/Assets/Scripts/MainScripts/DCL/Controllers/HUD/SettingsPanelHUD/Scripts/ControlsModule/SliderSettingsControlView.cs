using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.SettingsPanelHUD.Controls
{
    public class SliderSettingsControlView : SettingsControlView
    {
        [SerializeField] private Slider slider;
        [SerializeField] private TextMeshProUGUI indicatorLabel;

        public Slider sliderControl { get => slider; }

        public override void Initialize(SettingsControlModel controlConfig, SettingsControlController settingsControlController)
        {
            slider.maxValue = ((SliderControlModel)controlConfig).sliderMaxValue;
            slider.minValue = ((SliderControlModel)controlConfig).sliderMinValue;
            slider.wholeNumbers = ((SliderControlModel)controlConfig).sliderWholeNumbers;

            base.Initialize(controlConfig, settingsControlController);

            slider.value = (float)settingsControlController.GetInitialValue();
            indicatorLabel.text = slider.value.ToString();
            settingsControlController.OnControlChanged(slider.value);

            slider.onValueChanged.AddListener(sliderValue =>
            {
                indicatorLabel.text = sliderValue.ToString();
                settingsControlController.OnControlChanged(sliderValue);
                settingsControlController.ApplySettings();
            });
        }

        public void OverrideIndicatorLabel(string text)
        {
            indicatorLabel.text = text;
        }
    }
}