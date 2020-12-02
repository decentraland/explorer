using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    [CreateAssetMenu(menuName = "Settings/Configuration/Controls/Slider Control", fileName = "SliderControlConfiguration")]
    public class SliderControlModel : SettingsControlModel
    {
        public float sliderMinValue;
        public float sliderMaxValue;
        public bool sliderWholeNumbers;

        public SliderControlModel(
            string title,
            SettingsControlView controlPrefab,
            SettingsControlController controlController,
            float sliderMinValue,
            float sliderMaxValue,
            bool sliderWholeNumbers) : base(title, controlPrefab, controlController)
        {
            this.sliderMinValue = sliderMinValue;
            this.sliderMaxValue = sliderMaxValue;
            this.sliderWholeNumbers = sliderWholeNumbers;
        }
    }
}