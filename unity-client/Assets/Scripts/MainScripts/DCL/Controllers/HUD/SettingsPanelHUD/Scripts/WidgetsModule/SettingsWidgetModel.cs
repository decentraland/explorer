using DCL.SettingsPanelHUD.Controls;
using ReorderableList;
using UnityEngine;

namespace DCL.SettingsPanelHUD.Widgets
{
    [CreateAssetMenu(menuName = "Settings/Configuration/Widget")]
    public class SettingsWidgetModel : ScriptableObject
    {
        public string title;
        public SettingsWidgetView widgetPrefab;
        public SettingsWidgetController widgetController;

        [Reorderable]
        public SettingsControlGroupList controlColumns;

        public SettingsWidgetModel(
            string title,
            SettingsWidgetView widgetPrefab,
            SettingsWidgetController widgetController,
            SettingsControlGroupList controlGroups)
        {
            this.title = title;
            this.widgetPrefab = widgetPrefab;
            this.widgetController = widgetController;
            this.controlColumns = controlGroups;
        }
    }
}