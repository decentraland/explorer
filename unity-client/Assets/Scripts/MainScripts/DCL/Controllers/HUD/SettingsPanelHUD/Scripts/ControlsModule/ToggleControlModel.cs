using System.Collections.Generic;
using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    [CreateAssetMenu(menuName = "Settings/Configuration/Controls/Toggle Control", fileName = "ToggleControlConfiguration")]
    public class ToggleControlModel : SettingsControlModel
    {
        public ToggleControlModel(
            string title,
            SettingsControlView controlPrefab,
            SettingsControlController controlController,
            List<BooleanVariable> flagsThatDeactivateMe,
            bool isBeta) : base(title, controlPrefab, controlController, flagsThatDeactivateMe, isBeta)
        {
        }
    }
}