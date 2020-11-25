namespace DCL.SettingsPanelHUD.Widgets
{
    [System.Serializable]
    public class SettingsWidgetModel
    {
        public string title;
        public SettingsWidgetView widgetPrefab;

        public SettingsWidgetModel(string title, SettingsWidgetView widgetPrefab)
        {
            this.title = title;
            this.widgetPrefab = widgetPrefab;
        }
    }
}