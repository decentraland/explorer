using DCL.SettingsPanelHUD.Widgets;
using UnityEngine;

namespace DCL.SettingsPanelHUD.Sections
{
    public interface ISettingsSectionView
    {
        void Initialize(ISettingsSectionController settingsSectionController, SettingsWidgetsConfig widgetsConfig);
        void SetActive(bool active);
    }

    public class SettingsSectionView : MonoBehaviour, ISettingsSectionView
    {
        [SerializeField] private Transform widgetsContainer;

        private ISettingsSectionController settingsSectionController;
        private SettingsWidgetsConfig widgetsConfig;

        public void Initialize(ISettingsSectionController settingsSectionController, SettingsWidgetsConfig widgetsConfig)
        {
            this.settingsSectionController = settingsSectionController;
            this.widgetsConfig = widgetsConfig;

            CreateWidgets();
        }

        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }

        private void CreateWidgets()
        {
            foreach (SettingsWidgetModel widgetConfig in widgetsConfig.widgets)
            {
                var newWidget = Instantiate(widgetConfig.widgetPrefab, widgetsContainer);
                newWidget.gameObject.name = $"Widget_{widgetConfig.title}";
                var newWidgetController = Instantiate(widgetConfig.widgetController);
                settingsSectionController.AddWidget(newWidget, newWidgetController, widgetConfig);
            }
        }
    }
}