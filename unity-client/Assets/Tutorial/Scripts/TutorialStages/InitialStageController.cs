using System.Collections;
using UnityEngine;

public class InitialStageController : TutorialStageController
{
    [SerializeField] TutorialTooltip wellcomeTooltip = null;
    [SerializeField] TutorialTooltip controlsTooltip = null;
    [SerializeField] TutorialTooltip cameraTooltip = null;
    [SerializeField] TutorialTooltip minimapTooltip = null;

    private AvatarEditorHUDController avatarEditorHUD = null;
    private bool avatarEditorClosed = false;
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
            avatarEditorClosed = false;
        }
        else
        {
            avatarEditorClosed = true;
        }

        StartCoroutine(StageSecuence());
    }

    public override void OnStageFinished()
    {
        base.OnStageFinished();
        DCLCharacterController.OnPositionSet -= OnTeleport;
        DCLCharacterController.OnCharacterMoved -= OnCharacterMove;
        HUDController.i?.minimapHud.SetVisibility(true);
    }

    private IEnumerator StageSecuence()
    {
        yield return ShowTooltip(wellcomeTooltip);

        yield return new WaitUntil(() => avatarEditorClosed);
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
        if (avatarEditorHUD != null)
        {
            avatarEditorHUD.OnVisibilityChanged -= OnAvatarEditorVisibilityChanged;
            avatarEditorClosed = true;
        }
    }

    private void OnTeleport(DCLCharacterPosition characterPosition)
    {
        TutorialController.i?.SetRunningStageFinished();
    }

    private void OnCharacterMove(DCLCharacterPosition position)
    {
        characterMoved = true;
    }
}
