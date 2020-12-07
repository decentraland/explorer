using DCL.SettingsPanelHUD.Common;
using TMPro;
using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    public interface ISettingsControlView
    {
        void Initialize(SettingsControlModel controlConfig, SettingsControlController settingsControlController);
        void RefreshControl();
    }

    public abstract class SettingsControlView : MonoBehaviour, ISettingsControlView
    {
        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private GameObject betaIndicator;
        [SerializeField] private CanvasGroup canvasGroup;

        protected SettingsControlController settingsControlController;
        protected bool skipPostApplySettings = false;

        private SettingsControlModel controlConfig;

        private void OnEnable()
        {
            if (settingsControlController == null)
                return;

            skipPostApplySettings = true;
            RefreshControl();
        }

        public virtual void Initialize(SettingsControlModel controlConfig, SettingsControlController settingsControlController)
        {
            this.controlConfig = controlConfig;
            this.settingsControlController = settingsControlController;
            this.settingsControlController.Initialize(this);
            title.text = controlConfig.title;
            betaIndicator.SetActive(controlConfig.isBeta);

            foreach (BooleanVariable flag in controlConfig.flagsThatDeactivateMe)
            {
                flag.OnChange += OnAnyDeactivationFlagChange;
                OnAnyDeactivationFlagChange(flag.Get(), false);
            }

            CommonSettingsEvents.OnRefreshAllSettings += OnRefreshAllSettings;

            skipPostApplySettings = true;
            RefreshControl();
            settingsControlController.OnControlChanged(settingsControlController.GetStoredValue());
        }

        private void OnDestroy()
        {
            if (controlConfig != null)
            {
                foreach (BooleanVariable flag in controlConfig.flagsThatDeactivateMe)
                {
                    flag.OnChange -= OnAnyDeactivationFlagChange;
                }
            }

            CommonSettingsEvents.OnRefreshAllSettings -= OnRefreshAllSettings;
        }

        public abstract void RefreshControl();

        protected void ApplySetting(object newValue)
        {
            settingsControlController.OnControlChanged(newValue);
            settingsControlController.ApplySettings();

            if (!skipPostApplySettings)
                settingsControlController.PostApplySettings();
            skipPostApplySettings = false;
        }

        private void OnAnyDeactivationFlagChange(bool current, bool previous)
        {
            SetEnabled(!current);
        }

        private void SetEnabled(bool enabled)
        {
            canvasGroup.alpha = enabled ? 1 : 0.5f;
            canvasGroup.interactable = enabled;
        }

        private void OnRefreshAllSettings(SettingsControlController sender)
        {
            if (sender != settingsControlController)
            {
                skipPostApplySettings = true;
                RefreshControl();
            }
        }
    }
}