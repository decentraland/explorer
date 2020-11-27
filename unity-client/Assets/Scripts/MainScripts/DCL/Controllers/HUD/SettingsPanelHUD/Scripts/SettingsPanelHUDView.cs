using DCL.SettingsPanelHUD.Sections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.SettingsPanelHUD
{
    public class SettingsPanelHUDView : MonoBehaviour
    {
        [Header("Sections configuration")]
        [SerializeField] private SettingsPanelModel settingsPanelConfig;
        [SerializeField] private Transform menuButtonsContainer;
        [SerializeField] private Transform sectionsContainer;

        [Header("Close Settings")]
        [SerializeField] private Button closeButton;
        [SerializeField] private InputAction_Trigger closeAction;

        [Header("Animations")]
        [SerializeField] private ShowHideAnimator settingsAnimator;

        public bool isOpen { get; private set; }

        private const string PATH = "SettingsPanelHUD";

        private IHUD hudController;
        private ISettingsPanelHUDController settingsPanelController;

        public static SettingsPanelHUDView Create()
        {
            SettingsPanelHUDView view = Instantiate(Resources.Load<GameObject>(PATH)).GetComponent<SettingsPanelHUDView>();
            view.name = "_SettingsPanelHUD";
            return view;
        }

        public void Initialize(IHUD hudController, ISettingsPanelHUDController settingsPanelController)
        {
            this.hudController = hudController;
            this.settingsPanelController = settingsPanelController;

            CreateSections();
            isOpen = !settingsAnimator.hideOnEnable;

            closeButton.onClick.AddListener(() => CloseSettingsPanel());
        }

        private void CreateSections()
        {
            foreach (SettingsSectionModel sectionConfig in settingsPanelConfig.sections)
            {
                var newMenuButton = Instantiate(sectionConfig.menuButtonPrefab, menuButtonsContainer);
                var newSection = Instantiate(sectionConfig.sectionPrefab, sectionsContainer);
                newSection.gameObject.name = $"Section_{sectionConfig.text}";
                var newSectionController = Instantiate(sectionConfig.sectionController);
                settingsPanelController.AddSection(newMenuButton, newSection, newSectionController, sectionConfig);
            }

            settingsPanelController.OpenSection(0);
        }

        private void CloseSettingsPanel()
        {
            hudController.SetVisibility(false);
        }

        public void SetVisibility(bool visible)
        {
            if (visible && !isOpen)
                AudioScriptableObjects.dialogOpen.Play(true);
            else if (isOpen)
                AudioScriptableObjects.dialogClose.Play(true);

            closeAction.OnTriggered -= CloseAction_OnTriggered;
            if (visible)
            {
                closeAction.OnTriggered += CloseAction_OnTriggered;
                settingsAnimator.Show();
            }
            else
            {
                settingsAnimator.Hide();
            }

            isOpen = visible;
        }

        private void CloseAction_OnTriggered(DCLAction_Trigger action)
        {
            CloseSettingsPanel();
        }
    }
}