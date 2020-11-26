using DCL.SettingsPanelHUD.Controls;
using UnityEngine;

namespace DCL.SettingsPanelHUD.Widgets
{
    [System.Serializable]
    public class SettingsWidgetModel
    {
        [Header("Widget configuration")]
        public string title;
        public SettingsWidgetView widgetPrefab;
        public SettingsWidgetController widgetController;

        [Header("Controls configuration")]
        public SettingsControlsConfig controls;

        public SettingsWidgetModel(
            string title,
            SettingsWidgetView widgetPrefab,
            SettingsWidgetController widgetController,
            SettingsControlsConfig controls)
        {
            this.title = title;
            this.widgetPrefab = widgetPrefab;
            this.widgetController = widgetController;
            this.controls = controls;
        }
    }
}