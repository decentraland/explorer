using DCL.SettingsPanelHUD.Widgets;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace DCL.SettingsPanelHUD.Sections
{
    public interface ISettingsSectionController
    {
        List<ISettingsWidgetView> widgets { get; }
        void AddWidget(ISettingsWidgetView newWidget, ISettingsWidgetController newWidgetController, SettingsWidgetModel widgetConfig);
    }

    [CreateAssetMenu(menuName = "Settings/Controllers/Section", fileName = "SettingsSectionController")]
    public class SettingsSectionController : ScriptableObject, ISettingsSectionController
    {
        public List<ISettingsWidgetView> widgets { get; } = new List<ISettingsWidgetView>();

        public void AddWidget(
            ISettingsWidgetView newWidget,
            ISettingsWidgetController newWidgetController,
            SettingsWidgetModel widgetConfig)
        {
            newWidget.Initialize(widgetConfig.title, newWidgetController, widgetConfig.controlColumns.ToList());
            widgets.Add(newWidget);
        }
    }
}