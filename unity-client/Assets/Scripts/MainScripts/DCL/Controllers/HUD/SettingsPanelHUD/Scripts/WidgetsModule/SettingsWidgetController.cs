using DCL.SettingsPanelHUD.Controls;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.SettingsPanelHUD.Widgets
{
    public interface ISettingsWidgetController
    {
        List<ISettingsControlView> controls { get; }
        void AddControl(ISettingsControlView newControl, SettingsControlController newControlController, SettingsControlModel controlConfig);
    }

    [CreateAssetMenu(menuName = "Settings/Controllers/Widget", fileName = "SettingsWidgetController")]
    public class SettingsWidgetController : ScriptableObject, ISettingsWidgetController
    {
        public List<ISettingsControlView> controls { get; } = new List<ISettingsControlView>();

        public void AddControl(
            ISettingsControlView newControl,
            SettingsControlController newControlController,
            SettingsControlModel controlConfig)
        {
            newControl.Initialize(controlConfig, newControlController);
            controls.Add(newControl);
        }
    }
}