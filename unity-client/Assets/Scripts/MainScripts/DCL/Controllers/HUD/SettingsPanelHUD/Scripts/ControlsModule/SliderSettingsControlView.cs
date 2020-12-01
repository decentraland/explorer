using UnityEngine;
using UnityEngine.UI;

namespace DCL.SettingsPanelHUD.Controls
{
    public class SliderSettingsControlView : SettingsControlView
    {
        [SerializeField] private Slider slider = null;

        public override void Initialize(SettingsControlModel controlConfig, SettingsControlController settingsControlController)
        {
            base.Initialize(controlConfig, settingsControlController);

            slider.onValueChanged.AddListener(sliderValue =>
            {
                settingsControlController.OnControlChanged(sliderValue);
            });

            slider.value = (float)settingsControlController.GetStoredValue();
        }
    }
}