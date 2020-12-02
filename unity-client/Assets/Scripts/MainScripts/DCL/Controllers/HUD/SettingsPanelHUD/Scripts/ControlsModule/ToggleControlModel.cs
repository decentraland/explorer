using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    [CreateAssetMenu(menuName = "Settings/Configuration/Controls/Toggle Control", fileName = "ToggleControlConfiguration")]
    public class ToggleControlModel : SettingsControlModel
    {
        public ToggleControlModel(
            string title,
            SettingsControlView controlPrefab,
            SettingsControlController controlController) : base(title, controlPrefab, controlController)
        {
        }
    }
}