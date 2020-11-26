using DCL.SettingsPanelHUD.Controls;
using System;
using TMPro;
using UnityEngine;

namespace DCL.SettingsPanelHUD.Widgets
{
    public class SettingsWidgetView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private Transform controlsContainer;

        private ISettingsWidgetController settingsWidgetController;
        private SettingsControlsConfig controlsConfig;

        public void Initialize(string title, ISettingsWidgetController settingsWidgetController, SettingsControlsConfig controlsConfig)
        {
            this.settingsWidgetController = settingsWidgetController;
            this.controlsConfig = controlsConfig;

            this.title.text = title;

            CreateControls();
        }

        private void CreateControls()
        {
            foreach (SettingsControlModel controlConfig in controlsConfig.controls)
            {
                var newControl = Instantiate(controlConfig.controlPrefab, controlsContainer);
                var newWidgetController = Instantiate(controlConfig.controlController);
                settingsWidgetController.AddControl(newControl, newWidgetController, controlConfig);
            }
        }
    }
}