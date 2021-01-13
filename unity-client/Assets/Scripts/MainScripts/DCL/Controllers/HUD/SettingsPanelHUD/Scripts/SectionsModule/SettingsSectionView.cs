using DCL.SettingsController;
using DCL.SettingsPanelHUD.Widgets;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.SettingsPanelHUD.Sections
{
    /// <summary>
    /// Interface to implement a view for a SECTION.
    /// </summary>
    public interface ISettingsSectionView
    {
        /// <summary>
        /// All the needed logic to initializes the SECTION view and put its WIDGETS factory into operation.
        /// </summary>
        /// <param name="settingsSectionController">Controller that will be associated to this view.</param>
        /// <param name="widgets">List of WIDGETS associated to this SECTION.</param>
        void Initialize(
            ISettingsSectionController settingsSectionController,
            List<SettingsWidgetModel> widgets,
            IGeneralSettingsReferences generalSettingsController,
            IQualitySettingsReferences qualitySettingsController);

        /// <summary>
        /// Activates/deactivates the SECTION.
        /// </summary>
        /// <param name="active">True for SECTION activation.</param>
        void SetActive(bool active);
    }

    /// <summary>
    /// MonoBehaviour that represents a SECTION view and will act as a factory of WIDGETS.
    /// </summary>
    public class SettingsSectionView : MonoBehaviour, ISettingsSectionView
    {
        [SerializeField] private Transform widgetsContainer;
        [SerializeField] private ScrollRect scrollRect;

        private ISettingsSectionController settingsSectionController;
        private List<SettingsWidgetModel> widgets;
        private bool isOpen = false;

        public void Initialize(
            ISettingsSectionController settingsSectionController,
            List<SettingsWidgetModel> widgets,
            IGeneralSettingsReferences generalSettingsController,
            IQualitySettingsReferences qualitySettingsController)
        {
            this.settingsSectionController = settingsSectionController;
            this.widgets = widgets;

            CreateWidgets(generalSettingsController, qualitySettingsController);
        }

        public void SetActive(bool active)
        {
            gameObject.SetActive(active);

            if (!isOpen && active)
                scrollRect.verticalNormalizedPosition = 1f;

            isOpen = active;
        }

        private void CreateWidgets(IGeneralSettingsReferences generalSettingsController, IQualitySettingsReferences qualitySettingsController)
        {
            foreach (SettingsWidgetModel widgetConfig in widgets)
            {
                var newWidget = Instantiate(widgetConfig.widgetPrefab, widgetsContainer);
                newWidget.gameObject.name = $"Widget_{widgetConfig.title}";
                var newWidgetController = Instantiate(widgetConfig.widgetController);
                settingsSectionController.AddWidget(newWidget, newWidgetController, widgetConfig, generalSettingsController, qualitySettingsController);
            }
        }
    }
}