using DCL.SettingsPanelHUD.Widgets;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.SettingsPanelHUD.Sections
{
    public interface ISettingsSectionView
    {
        void Initialize(ISettingsSectionController settingsSectionController, List<SettingsWidgetModel> widgets);
        void SetActive(bool active);
    }

    public class SettingsSectionView : MonoBehaviour, ISettingsSectionView
    {
        [SerializeField] private Transform widgetsContainer;

        private ISettingsSectionController settingsSectionController;
        private List<SettingsWidgetModel> widgets;

        public void Initialize(ISettingsSectionController settingsSectionController, List<SettingsWidgetModel> widgets)
        {
            this.settingsSectionController = settingsSectionController;
            this.widgets = widgets;

            CreateWidgets();
        }

        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }

        private void CreateWidgets()
        {
            foreach (SettingsWidgetModel widgetConfig in widgets)
            {
                var newWidget = Instantiate(widgetConfig.widgetPrefab, widgetsContainer);
                newWidget.gameObject.name = $"Widget_{widgetConfig.title}";
                var newWidgetController = Instantiate(widgetConfig.widgetController);
                settingsSectionController.AddWidget(newWidget, newWidgetController, widgetConfig);
            }
        }
    }
}