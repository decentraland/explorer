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

            CommonSettingsVariables.refreshAllSettings.OnChange += RefreshAllSettings_OnChange;

            RefreshControl();
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

            if (CommonSettingsVariables.refreshAllSettings != null)
                CommonSettingsVariables.refreshAllSettings.OnChange -= RefreshAllSettings_OnChange;
        }

        public abstract void RefreshControl();

        private void OnAnyDeactivationFlagChange(bool current, bool previous)
        {
            SetEnabled(!current);
        }

        private void SetEnabled(bool enabled)
        {
            canvasGroup.alpha = enabled ? 1 : 0.5f;
            canvasGroup.interactable = enabled;
        }

        private void RefreshAllSettings_OnChange(SettingsControlController currentSender, SettingsControlController previousSender)
        {
            if (currentSender != settingsControlController)
            {
                skipPostApplySettings = true;
                RefreshControl();
            }
        }
    }
}