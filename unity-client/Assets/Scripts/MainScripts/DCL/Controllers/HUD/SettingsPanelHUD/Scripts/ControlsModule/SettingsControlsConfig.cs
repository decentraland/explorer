using System.Collections.Generic;
using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    [CreateAssetMenu(menuName = "Settings/Configuration/Controls")]
    public class SettingsControlsConfig : ScriptableObject
    {
        public List<SettingsControlList> columns;
    }
}