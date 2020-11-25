using DCL.SettingsPanelHUD.Sections;
using System.Collections.Generic;

namespace DCL.SettingsPanelHUD
{
    public interface ISettingsPanelHUDController
    {
        void AddSection(SettingsButtonEntry newMenuButton, SettingsSectionView newSection, SettingsSectionController newSectionController, SettingsSectionModel sectionConfig);
        void OpenSection(SettingsSectionView sectionToOpen);
        void OpenSection(int sectionIndex);
    }

    public class SettingsPanelHUDController : IHUD, ISettingsPanelHUDController
    {
        public SettingsPanelHUDView view { get; private set; }

        public event System.Action OnClose;

        private List<SettingsSectionView> settingsSections = new List<SettingsSectionView>();

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

        public void AddSection(
            SettingsButtonEntry newMenuButton,
            SettingsSectionView newSection,
            SettingsSectionController newSectionController,
            SettingsSectionModel sectionConfig)
        {
            newMenuButton.Initialize(sectionConfig.icon, sectionConfig.text);
            newSection.Initialize(newSectionController, sectionConfig.widgets);
            newSection.SetActive(false);
            settingsSections.Add(newSection);
            newMenuButton.ConfigureAction(() => OpenSection(newSection));
        }

        public void OpenSection(SettingsSectionView sectionToOpen)
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