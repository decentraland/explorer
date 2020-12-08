using DCL.SettingsPanelHUD.Controls;
using ReorderableList;
using UnityEngine;

namespace DCL.SettingsPanelHUD.Widgets
{
    [CreateAssetMenu(menuName = "Settings/Configuration/Widget", fileName = "WidgetConfiguration")]
    public class SettingsWidgetModel : ScriptableObject
    {
        public string title;
        public SettingsWidgetView widgetPrefab;
        public SettingsWidgetController widgetController;

        [Reorderable]
        public SettingsControlGroupList controlColumns;
    }
}