using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    [CreateAssetMenu(menuName = "Settings/Configuration/Controls/Slider Control", fileName = "SliderControlConfiguration")]
    public class SliderControlModel : SettingsControlModel
    {
        [Header("SLIDER CONFIGURATION")]
        public float sliderMinValue;
        public float sliderMaxValue;
        public bool sliderWholeNumbers;
    }
}