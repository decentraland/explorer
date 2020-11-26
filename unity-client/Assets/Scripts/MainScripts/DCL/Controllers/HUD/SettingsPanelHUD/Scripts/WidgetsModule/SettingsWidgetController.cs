using DCL.SettingsPanelHUD.Controls;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.SettingsPanelHUD.Widgets
{
    public interface ISettingsWidgetController
    {
        void AddControl(SettingsControlView newControl, ISettingsControlController newControlController, SettingsControlModel controlConfig);
    }

    [CreateAssetMenu(menuName = "Settings/Controllers/Widget Controller", fileName = "SettingsWidgetController")]
    public class SettingsWidgetController : ScriptableObject, ISettingsWidgetController
    {
        private List<SettingsControlView> controlsWidgets = new List<SettingsControlView>();

        public void AddControl(
            SettingsControlView newControl,
            ISettingsControlController newControlController,
            SettingsControlModel controlConfig)
        {
            newControl.Initialize(controlConfig.title, newControlController);
            controlsWidgets.Add(newControl);
        }
    }
}