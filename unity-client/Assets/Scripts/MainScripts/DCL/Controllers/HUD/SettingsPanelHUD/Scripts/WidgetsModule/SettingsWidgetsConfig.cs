using System.Collections.Generic;
using UnityEngine;

namespace DCL.SettingsPanelHUD.Widgets
{
    [CreateAssetMenu(menuName = "Settings/Configuration/Widgets")]
    public class SettingsWidgetsConfig : ScriptableObject
    {
        public List<SettingsWidgetModel> widgets;
    }
}