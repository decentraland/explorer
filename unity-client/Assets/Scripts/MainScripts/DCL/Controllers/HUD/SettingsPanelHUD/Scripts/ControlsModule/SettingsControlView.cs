using TMPro;
using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    public interface ISettingsControlView
    {
        void Initialize(SettingsControlModel controlConfig, SettingsControlController settingsControlController);
        void SetEnabled(bool enabled);
    }

    public abstract class SettingsControlView : MonoBehaviour, ISettingsControlView
    {
        [SerializeField] private TextMeshProUGUI title;

        protected SettingsControlController settingsControlController;

        public virtual void Initialize(SettingsControlModel controlConfig, SettingsControlController settingsControlController)
        {
            this.settingsControlController = settingsControlController;
            this.settingsControlController.Initialize(this);

            this.title.text = controlConfig.title;

            foreach (BooleanVariable flag in controlConfig.flagsThatDeactivateMe)
            {
                flag.OnChange += OnAnyDeactivationFlagChange;
            }
        }

        public abstract void SetEnabled(bool enabled);

        private void OnAnyDeactivationFlagChange(bool current, bool previous)
        {
            SetEnabled(!current);
        }
    }
}