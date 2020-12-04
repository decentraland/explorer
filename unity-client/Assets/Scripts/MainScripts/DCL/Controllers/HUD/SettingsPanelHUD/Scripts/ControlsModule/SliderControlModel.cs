using System.Collections.Generic;
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

        public SliderControlModel(
            string title,
            SettingsControlView controlPrefab,
            SettingsControlController controlController,
            List<BooleanVariable> flagsThatDeactivateMe,
            bool refreshAllSettingsOnChange,
            float sliderMinValue,
            float sliderMaxValue) : base(title, controlPrefab, controlController, flagsThatDeactivateMe)
        {
            this.sliderMinValue = sliderMinValue;
            this.sliderMaxValue = sliderMaxValue;
            this.sliderWholeNumbers = sliderWholeNumbers;
        }
    }
}