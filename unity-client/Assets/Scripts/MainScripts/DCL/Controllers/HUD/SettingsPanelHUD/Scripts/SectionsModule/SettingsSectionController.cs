using DCL.SettingsPanelHUD.Widgets;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.SettingsPanelHUD.Sections
{
    public interface ISettingsSectionController
    {
        void AddWidget(SettingsWidgetView newWidget, SettingsWidgetModel widgetConfig);
    }

    [CreateAssetMenu(menuName = "Settings/Controllers/Section", fileName = "SettingsSectionController")]
    public class SettingsSectionController : ScriptableObject, ISettingsSectionController
    {
        private List<SettingsWidgetView> settingsWidgets = new List<SettingsWidgetView>();

        public void AddWidget(SettingsWidgetView newWidget, SettingsWidgetModel widgetConfig)
        {
            settingsWidgets.Add(newWidget);
        }
    }
}