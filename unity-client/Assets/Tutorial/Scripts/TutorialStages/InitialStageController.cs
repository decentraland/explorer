using System.Collections;
using UnityEngine;

public class InitialStageController : TutorialStageController
{
    [SerializeField] TutorialTooltip wellcomeTooltip = null;
    [SerializeField] TutorialTooltip controlsTooltip = null;
    [SerializeField] TutorialTooltip cameraTooltip = null;
    [SerializeField] TutorialTooltip minimapTooltip = null;
    [SerializeField] GameObject claimNamePanel = null;

    private AvatarEditorHUDController avatarEditorHUD = null;
    private bool claimNamePanelClosed = false;
    private bool characterMoved = false;

    public override void OnStageStart()
    {
        base.OnStageStart();

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

        StartCoroutine(StageSequence());
    }

    public override void OnStageFinished()
    {
        base.OnStageFinished();
        DCLCharacterController.OnPositionSet -= OnTeleport;
        DCLCharacterController.OnCharacterMoved -= OnCharacterMove;
        HUDController.i?.minimapHud.SetVisibility(true);
    }

    private IEnumerator StageSequence()
    {
        yield return ShowTooltip(wellcomeTooltip);

        yield return new WaitUntil(() => claimNamePanelClosed);
        yield return WaitSeconds(3);

        yield return ShowTooltip(controlsTooltip, autoHide: false);
        characterMoved = false;

#if UNITY_EDITOR
        if (DCLCharacterController.i == null)
        {
            characterMoved = true;
        }
#endif

        yield return new WaitUntil(() => characterMoved);
        yield return WaitIdleTime();
        HideTooltip(controlsTooltip);

        yield return WaitSeconds(3);
        yield return ShowTooltip(cameraTooltip);
        yield return WaitIdleTime();

        HUDController.i?.minimapHud.SetVisibility(true);
        yield return ShowTooltip(minimapTooltip);

#if UNITY_EDITOR
        if (TutorialController.i.debugRunTutorialOnStart)
        {
            TutorialController.i?.SetRunningStageFinished();
        }
#endif
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
        TutorialController.i?.SetRunningStageFinished();
    }

    private void OnCharacterMove(DCLCharacterPosition position)
    {
        characterMoved = true;
    }

    public void ClaimNameButtonAction()
    {
        Application.OpenURL("http://avatars.decentraland.org");

        ContinueAsGuestButtonAction();
    }

    public void ContinueAsGuestButtonAction()
    {
        claimNamePanel.SetActive(false);

        claimNamePanelClosed = true;
    }
}
