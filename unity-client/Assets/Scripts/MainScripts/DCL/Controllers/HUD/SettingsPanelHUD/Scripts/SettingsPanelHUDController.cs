using DCL.SettingsPanelHUD.Sections;
using System.Collections.Generic;
using System.Linq;

namespace DCL.SettingsPanelHUD
{
    public interface ISettingsPanelHUDController
    {
        List<ISettingsSectionView> sections { get; }
        void Initialize();
        void AddSection(SettingsButtonEntry newMenuButton, ISettingsSectionView newSection, ISettingsSectionController newSectionController, SettingsSectionModel sectionConfig);
        void OpenSection(ISettingsSectionView sectionToOpen);
        void OpenSection(int sectionIndex);
    }

    public class SettingsPanelHUDController : IHUD, ISettingsPanelHUDController
    {
        public SettingsPanelHUDView view { get; private set; }

        public event System.Action OnClose;

        public List<ISettingsSectionView> sections { get; } = new List<ISettingsSectionView>();

        public SettingsPanelHUDController()
        {
            view = SettingsPanelHUDView.Create();
        }

        public void Dispose()
        {
            if (view != null)
                UnityEngine.Object.Destroy(view.gameObject);
        }

        public void SetVisibility(bool visible)
        {
            if (!visible && view.isOpen)
            {
                Settings.i.SaveSettings();
                OnClose?.Invoke();
            }

            view.SetVisibility(visible);
        }

        public void Initialize()
        {
            view.Initialize(this, this);
        }

        public void AddSection(
            SettingsButtonEntry newMenuButton,
            ISettingsSectionView newSection,
            ISettingsSectionController newSectionController,
            SettingsSectionModel sectionConfig)
        {
            newMenuButton?.Initialize(sectionConfig.icon, sectionConfig.text);

            newSection.Initialize(newSectionController, sectionConfig.widgets.ToList());
            newSection.SetActive(false);
            sections.Add(newSection);
            newMenuButton?.ConfigureAction(() => OpenSection(newSection));
        }

        public void OpenSection(ISettingsSectionView sectionToOpen)
        {
            foreach (var section in sections)
            {
                section.SetActive(false);
            }

            sectionToOpen.SetActive(true);
        }

        public void OpenSection(int sectionIndex)
        {
            foreach (var section in sections)
            {
                section.SetActive(false);
            }

            sections[sectionIndex].SetActive(true);
        }
    }
}