using DCL.SettingsPanelHUD.Common;
using TMPro;
using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    public interface ISettingsControlView
    {
        void Initialize(SettingsControlModel controlConfig, SettingsControlController settingsControlController);
        void SetEnabled(bool enabled);
        void RefreshControl();
    }

    public abstract class SettingsControlView : MonoBehaviour, ISettingsControlView
    {
        [SerializeField] private TextMeshProUGUI title;

        protected SettingsControlController settingsControlController;

        private SettingsControlModel controlConfig;

        public virtual void Initialize(SettingsControlModel controlConfig, SettingsControlController settingsControlController)
        {
            this.controlConfig = controlConfig;
            this.settingsControlController = settingsControlController;
            this.settingsControlController.Initialize(this);
            this.title.text = controlConfig.title;

            foreach (BooleanVariable flag in controlConfig.flagsThatDeactivateMe)
            {
                flag.OnChange += OnAnyDeactivationFlagChange;
            }

            CommonSettingsVariables.refreshAllSettings.OnChange += RefreshAllSettings_OnChange;
        }

        private void OnDestroy()
        {
            foreach (BooleanVariable flag in controlConfig.flagsThatDeactivateMe)
            {
                flag.OnChange -= OnAnyDeactivationFlagChange;
            }

            if (CommonSettingsVariables.refreshAllSettings != null)
                CommonSettingsVariables.refreshAllSettings.OnChange -= RefreshAllSettings_OnChange;
        }

        public abstract void SetEnabled(bool enabled);

        public abstract void RefreshControl();

        private void OnAnyDeactivationFlagChange(bool current, bool previous)
        {
            SetEnabled(!current);
        }

        private void RefreshAllSettings_OnChange(bool current, bool previous)
        {
            if (current)
                RefreshControl();
        }
    }
}