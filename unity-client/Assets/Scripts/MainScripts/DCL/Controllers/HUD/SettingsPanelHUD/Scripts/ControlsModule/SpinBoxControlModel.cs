using System.Collections.Generic;
using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    [CreateAssetMenu(menuName = "Settings/Configuration/Controls/SpinBox Control", fileName = "SpinBoxControlConfiguration")]
    public class SpinBoxControlModel : SettingsControlModel
    {
        [Header("SPIN-BOX CONFIGURATION")]
        public string[] spinBoxLabels;

        public SpinBoxControlModel(
            string title,
            SettingsControlView controlPrefab,
            SettingsControlController controlController,
            List<BooleanVariable> flagsThatDeactivateMe,
            bool isBeta,
            string[] spinBoxLabels) : base(title, controlPrefab, controlController, flagsThatDeactivateMe, isBeta)
        {
            this.spinBoxLabels = spinBoxLabels;
        }
    }
}