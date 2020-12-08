using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    [CreateAssetMenu(menuName = "Settings/Configuration/Controls/SpinBox Control", fileName = "SpinBoxControlConfiguration")]
    public class SpinBoxControlModel : SettingsControlModel
    {
        [Header("SPIN-BOX CONFIGURATION")]
        public string[] spinBoxLabels;
    }
}