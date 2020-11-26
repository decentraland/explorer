using DCL.SettingsPanelHUD.Sections;
using System.Collections.Generic;

namespace DCL.SettingsPanelHUD
{
    public interface ISettingsPanelHUDController
    {
        List<ISettingsSectionView> sections { get; }
        void AddSection(SettingsButtonEntry newMenuButton, ISettingsSectionView newSection, ISettingsSectionController newSectionController, SettingsSectionModel sectionConfig);
        void OpenSection(ISettingsSectionView sectionToOpen);
        void OpenSection(int sectionIndex);
    }

    public class SettingsPanelHUDController : IHUD, ISettingsPanelHUDController
    {
        public SettingsPanelHUDView view { get; private set; }

        public event System.Action OnClose;

        public List<ISettingsSectionView> sections { get => sectionList; }
        private List<ISettingsSectionView> sectionList = new List<ISettingsSectionView>();

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
            ISettingsSectionView newSection,
            ISettingsSectionController newSectionController,
            SettingsSectionModel sectionConfig)
        {
            newMenuButton.Initialize(sectionConfig.icon, sectionConfig.text);
            newSection.Initialize(newSectionController, sectionConfig.widgets);
            newSection.SetActive(false);
            sectionList.Add(newSection);
            newMenuButton.ConfigureAction(() => OpenSection(newSection));
        }

        public void OpenSection(ISettingsSectionView sectionToOpen)
        {
            foreach (var section in sectionList)
            {
                section.SetActive(false);
            }

            sectionToOpen.SetActive(true);
        }

        public void OpenSection(int sectionIndex)
        {
            foreach (var section in sectionList)
            {
                section.SetActive(false);
            }

            sectionList[sectionIndex].SetActive(true);
        }
    }
}