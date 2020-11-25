using System.Collections.Generic;
using UnityEngine;

namespace DCL.SettingsPanelHUD.Sections
{
    [CreateAssetMenu(menuName = "Settings/Configuration/Sections")]
    public class SettingsSectionsConfig : ScriptableObject
    {
        public List<SettingsSectionModel> sections;
    }
}