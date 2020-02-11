using System.Collections;
using UnityEngine;
using DCL.Interface;

namespace DCL.Tutorial
{
    public class TutorialStep_Initial : TutorialStep
    {
        [SerializeField] TutorialTooltip welcomeTooltip = null;
        [SerializeField] TutorialTooltip controlsTooltip = null;
        [SerializeField] TutorialTooltip cameraTooltip = null;
        [SerializeField] TutorialTooltip minimapTooltip = null;
        [SerializeField] GameObject claimNamePanel = null;

        const string CLAIM_NAME_URL = "http://avatars.decentraland.org";

        AvatarEditorHUDController avatarEditorHUD = null;
        bool claimNamePanelClosed = false;
        bool characterMoved = false;
        bool characterTeleported = false;

        public override void OnStepStart()
        {
            base.OnStepStart();

            HUDController.i?.minimapHud.SetVisibility(false);
            HUDController.i?.expressionsHud.SetVisibility(false);
            TutorialController.i.SetChatVisible(false);

            DCLCharacterController.OnPositionSet += OnTeleport;
            DCLCharacterController.OnCharacterMoved += OnCharacterMove;

            if (HUDController.i != null && HUDController.i.avatarEditorHud != null)
            {
                avatarEditorHUD = HUDController.i.avatarEditorHud;
                avatarEditorHUD.SetVisibility(true);
                avatarEditorHUD.OnVisibilityChanged += OnAvatarEditorVisibilityChanged;
                claimNamePanelClosed = false;
            }
            else
            {
                claimNamePanelClosed = true;
            }
        }

        public override void OnStepFinished()
        {
            base.OnStepFinished();

            welcomeTooltip.Hide();
            controlsTooltip.Hide();
            cameraTooltip.Hide();
            minimapTooltip.Hide();

            DCLCharacterController.OnPositionSet -= OnTeleport;
            DCLCharacterController.OnCharacterMoved -= OnCharacterMove;
        }

        public override IEnumerator OnStepExecute()
        {
            welcomeTooltip.Show();

            yield return new WaitUntil(() => claimNamePanelClosed);
            yield return WaitForSecondsCache.Get(3);

            controlsTooltip.Show(autoHide: false);
            characterMoved = false;
            characterTeleported = false;

#if UNITY_EDITOR
            if (DCLCharacterController.i == null)
            {
                characterMoved = true;
            }
#endif

            yield return new WaitUntil(() => characterMoved);
            yield return WaitIdleTime();
            controlsTooltip.Hide();

            yield return WaitForSecondsCache.Get(3);
            cameraTooltip.Show();
            yield return WaitIdleTime();

            HUDController.i?.minimapHud.SetVisibility(true);
            minimapTooltip.Show();

#if UNITY_EDITOR
            if (TutorialController.i.debugRunTutorialOnStart)
            {
                characterTeleported = true;
            }
#endif

            yield return new WaitUntil(() => characterTeleported == true);

        }

        private void OnAvatarEditorVisibilityChanged(bool visible)
        {
            if (visible) return;

            if (avatarEditorHUD != null)
                avatarEditorHUD.OnVisibilityChanged -= OnAvatarEditorVisibilityChanged;

            claimNamePanel.SetActive(true);
        }

        private void OnTeleport(DCLCharacterPosition characterPosition)
        {
            characterTeleported = true;
            TutorialController.i.SkipToNextStep();
        }

        private void OnCharacterMove(DCLCharacterPosition position)
        {
            characterMoved = true;
        }

        public void ClaimNameButtonAction()
        {
            WebInterface.OpenWebURL(CLAIM_NAME_URL);

            ContinueAsGuestButtonAction();
        }

        public void ContinueAsGuestButtonAction()
        {
            claimNamePanel.SetActive(false);
            claimNamePanelClosed = true;
        }
    }
}
