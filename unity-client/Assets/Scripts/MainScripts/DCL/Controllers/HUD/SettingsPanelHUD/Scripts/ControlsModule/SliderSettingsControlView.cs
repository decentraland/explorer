using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.SettingsPanelHUD.Controls
{
    public class SliderSettingsControlView : SettingsControlView
    {
        [SerializeField] private Slider slider;
        [SerializeField] private TextMeshProUGUI indicatorLabel;

        public Slider spinBoxControl { get => slider; }

        public override void Initialize(SettingsControlModel controlConfig, SettingsControlController settingsControlController)
        {
            base.Initialize(controlConfig, settingsControlController);

            slider.onValueChanged.AddListener(sliderValue =>
            {
                settingsControlController.OnControlChanged(sliderValue);
                indicatorLabel.text = sliderValue.ToString("0.0");
            });

            slider.value = (float)settingsControlController.GetInitialValue();
        }
    }
}