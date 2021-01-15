using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.SettingsPanelHUD.Controls
{
    /// <summary>
    /// MonoBehaviour that represents the view of a SLIDER type CONTROL.
    /// </summary>
    public class SliderSettingsControlView : SettingsControlView
    {
        [SerializeField] private Slider slider;
        [SerializeField] private TextMeshProUGUI indicatorLabel;

        public Slider sliderControl => slider;

        private SliderControlModel sliderControlConfig;
        private SliderSettingsControlController sliderController;

        public override void Initialize(SettingsControlModel controlConfig, SettingsControlController settingsControlController)
        {
            this.sliderControlConfig = (SliderControlModel)controlConfig;
            slider.maxValue = this.sliderControlConfig.sliderMaxValue;
            slider.minValue = this.sliderControlConfig.sliderMinValue;
            slider.wholeNumbers = this.sliderControlConfig.sliderWholeNumbers;

            sliderController = (SliderSettingsControlController)settingsControlController;
            sliderController.OnOverrideIndicatorLabel += OverrideIndicatorLabel;

            base.Initialize(controlConfig, sliderController);
            OverrideIndicatorLabel(slider.value.ToString());
            sliderController.OnControlChanged(slider.value);

            slider.onValueChanged.AddListener(sliderValue =>
            {
                OverrideIndicatorLabel(sliderValue.ToString());
                ApplySetting(sliderValue);
            });
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (sliderController != null)
                sliderController.OnOverrideIndicatorLabel -= OverrideIndicatorLabel;
        }

        /// <summary>
        /// Overrides the text of the label associated to the slider.
        /// </summary>
        /// <param name="text">New label text.</param>
        public void OverrideIndicatorLabel(string text)
        {
            indicatorLabel.text = text;
        }

        public override void RefreshControl()
        {
            base.RefreshControl();

            float newValue = (float)sliderController.GetStoredValue();
            if (slider.value != newValue)
                slider.value = newValue;
        }
    }
}