using DCL.SettingsPanelHUD.Widgets;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.SettingsPanelHUD.Sections
{
    public interface ISettingsSectionController
    {
        List<ISettingsWidgetView> widgets { get; }
        void AddWidget(ISettingsWidgetView newWidget, ISettingsWidgetController newWidgetController, SettingsWidgetModel widgetConfig);
    }

    [CreateAssetMenu(menuName = "Settings/Controllers/Section Controller", fileName = "SettingsSectionController")]
    public class SettingsSectionController : ScriptableObject, ISettingsSectionController
    {
        public List<ISettingsWidgetView> widgets { get => widgetList; }
        private List<ISettingsWidgetView> widgetList = new List<ISettingsWidgetView>();

        public void AddWidget(
            ISettingsWidgetView newWidget,
            ISettingsWidgetController newWidgetController,
            SettingsWidgetModel widgetConfig)
        {
            newWidget.Initialize(widgetConfig.title, newWidgetController, widgetConfig.controlColumns);
            widgetList.Add(newWidget);
        }
    }
}