using DCL.SettingsPanelHUD.Sections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.SettingsPanelHUD
{
    [CreateAssetMenu(menuName = "Settings/Configuration/MainPanel")]
    public class SettingsPanelModel : ScriptableObject
    {
        public List<SettingsSectionModel> sections;
    }
}