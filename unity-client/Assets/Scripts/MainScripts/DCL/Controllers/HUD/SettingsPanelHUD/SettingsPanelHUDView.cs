using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.SettingsPanelHUD
{
    public class SettingsPanelHUDView : MonoBehaviour
    {
        [Header("Menu configuration")]
        [SerializeField] private SettingsButtonEntry menuButtonPrefab;
        [SerializeField] private List<SettingsButtonData> menuButtonList;
        [SerializeField] private Transform buttonsContainer;

        [Header("Sections configuration")]
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

            CreateAndConfigureMenus();
            isOpen = !settingsAnimator.hideOnEnable;

            closeButton.onClick.AddListener(() => CloseSettingsPanel());
        }

        private void CreateAndConfigureMenus()
        {
            foreach (SettingsButtonData menuButtonConfig in menuButtonList)
            {
                var newMenuButton = Instantiate(menuButtonPrefab, buttonsContainer);
                settingsPanelController.AddMenuButton(newMenuButton, menuButtonConfig);

                var newSection = Instantiate(menuButtonConfig.sectionToOpen, sectionsContainer);
                settingsPanelController.AddSection(newSection, false);

                newMenuButton.ConfigureAction(() => OpenSection(newSection));
            }

            settingsPanelController.OpenSection(0);
        }

        private void OpenSection(SettingsSection sectionToOpen)
        {
            settingsPanelController.OpenSection(sectionToOpen);
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