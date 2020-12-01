using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    public class SpinBoxSettingsControlView : SettingsControlView
    {
        [SerializeField] private SpinBoxPresetted spinBox = null;

        public override void Initialize(string title, SettingsControlController settingsControlController)
        {
            base.Initialize(title, settingsControlController);

            spinBox.onValueChanged.AddListener(sliderValue =>
            {
                settingsControlController.OnControlChanged(sliderValue);
            });

            spinBox.value = (int)settingsControlController.GetStoredValue();
        }
    }
}