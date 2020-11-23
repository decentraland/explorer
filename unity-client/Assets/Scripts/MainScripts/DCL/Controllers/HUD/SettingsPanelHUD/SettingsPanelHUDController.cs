using System.Collections.Generic;

namespace DCL.SettingsPanelHUD
{
    public interface ISettingsPanelHUDController
    {
        void AddMenuButton(SettingsButtonEntry newButton, SettingsButtonData buttonConfig);
        void AddSection(SettingsSection newSection, bool isActive = true);
        void OpenSection(SettingsSection sectionToOpen);
        void OpenSection(int sectionIndex);
    }

    public class SettingsPanelHUDController : IHUD, ISettingsPanelHUDController
    {
        public SettingsPanelHUDView view { get; private set; }

        public event System.Action OnClose;
        public event System.Action<SettingsSection> OnSectionOpen;

        private List<SettingsButtonEntry> settingsMenuButtons = new List<SettingsButtonEntry>();
        private List<SettingsSection> settingsSections = new List<SettingsSection>();

        public SettingsPanelHUDController()
        {
            view = SettingsPanelHUDView.Create();
            view.Initialize(this, this);
        }

        public void Dispose()
        {
            if (view != null)
                UnityEngine.Object.Destroy(view.gameObject);
        }

        public void SetVisibility(bool visible)
        {
            if (!visible && view.isOpen)
                OnClose?.Invoke();

            view.SetVisibility(visible);
        }

        public void AddMenuButton(SettingsButtonEntry newButton, SettingsButtonData buttonConfig)
        {
            newButton.Initialize(buttonConfig);
            settingsMenuButtons.Add(newButton);
        }

        public void AddSection(SettingsSection newSection, bool isActive = true)
        {
            newSection.SetActive(isActive);
            settingsSections.Add(newSection);
        }

        public void OpenSection(SettingsSection sectionToOpen)
        {
            foreach (var section in settingsSections)
            {
                section.SetActive(false);
            }

            sectionToOpen.SetActive(true);
        }

        public void OpenSection(int sectionIndex)
        {
            foreach (var section in settingsSections)
            {
                section.SetActive(false);
            }

            settingsSections[sectionIndex].SetActive(true);
        }
    }
}