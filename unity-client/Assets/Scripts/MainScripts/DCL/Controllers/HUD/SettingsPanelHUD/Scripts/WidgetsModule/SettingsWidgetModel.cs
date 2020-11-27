using DCL.SettingsPanelHUD.Controls;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.SettingsPanelHUD.Widgets
{
    [CreateAssetMenu(menuName = "Settings/Configuration/Widget")]
    public class SettingsWidgetModel : ScriptableObject
    {
        public string title;
        public SettingsWidgetView widgetPrefab;
        public SettingsWidgetController widgetController;
        public List<SettingsControlGroup> controlColumns;

        public SettingsWidgetModel(
            string title,
            SettingsWidgetView widgetPrefab,
            SettingsWidgetController widgetController,
            List<SettingsControlGroup> controlGroups)
        {
            this.title = title;
            this.widgetPrefab = widgetPrefab;
            this.widgetController = widgetController;
            this.controlColumns = controlGroups;
        }
    }
}