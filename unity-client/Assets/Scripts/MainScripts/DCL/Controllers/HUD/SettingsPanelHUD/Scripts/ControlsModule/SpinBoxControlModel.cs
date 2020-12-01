using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    [CreateAssetMenu(menuName = "Settings/Configuration/Controls/SpinBox Control", fileName = "SpinBoxControlConfiguration")]
    public class SpinBoxControlModel : SettingsControlModel
    {
        public string[] spinBoxLabels;

        public SpinBoxControlModel(
            string title,
            SettingsControlView controlPrefab,
            SettingsControlController controlController,
            string[] spinBoxLabels) : base(title, controlPrefab, controlController)
        {
            this.spinBoxLabels = spinBoxLabels;
        }
    }
}