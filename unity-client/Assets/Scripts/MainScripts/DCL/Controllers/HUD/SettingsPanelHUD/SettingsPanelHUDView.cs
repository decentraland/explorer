using System.Collections.Generic;
using UnityEngine;

namespace DCL.SettingsPanelHUD
{
    public class SettingsPanelHUDView : MonoBehaviour
    {
        [Header("Menu configuration")]
        [SerializeField]
        private SettingsButtonEntry menuButtonPrefab;

        [SerializeField]
        private List<SettingsButtonData> menuButtonList;

        [SerializeField]
        private Transform buttonsContainer;

        [Header("Sections configuration")]
        [SerializeField]
        private Transform sectionsContainer;

        private List<SettingsSection> loadedSections = new List<SettingsSection>();

        private void Awake()
        {
            CreateAndConfigureMenus();
        }

        private void CreateAndConfigureMenus()
        {
            foreach (SettingsButtonData menuButtonConfig in menuButtonList)
            {
                var newButton = Instantiate(menuButtonPrefab, buttonsContainer);
                newButton.Initialize(menuButtonConfig);

                var newSection = Instantiate(menuButtonConfig.sectionToOpen, sectionsContainer);
                newSection.SetActive(false);
                loadedSections.Add(newSection);

                newButton.ConfigureAction(() => OpenSection(newSection));
            }
        }

        private void OpenSection(SettingsSection newSection)
        {
            foreach (var section in loadedSections)
            {
                section.SetActive(false);
            }

            newSection.SetActive(true);
        }
    }
}