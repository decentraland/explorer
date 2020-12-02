using TMPro;
using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    public interface ISettingsControlView
    {
        void Initialize(SettingsControlModel controlConfig, SettingsControlController settingsControlController);
    }

    public class SettingsControlView : MonoBehaviour, ISettingsControlView
    {
        [SerializeField] private TextMeshProUGUI title;

        protected SettingsControlController settingsControlController;

        public virtual void Initialize(SettingsControlModel controlConfig, SettingsControlController settingsControlController)
        {
            this.settingsControlController = settingsControlController;
            this.settingsControlController.Initialize(this);

            this.title.text = controlConfig.title;
        }
    }
}