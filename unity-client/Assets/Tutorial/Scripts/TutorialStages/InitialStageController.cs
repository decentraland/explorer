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

    public override void OnStageStart()
    {
        base.OnStageStart();

        DCLCharacterController.OnPositionSet += OnTeleport;

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
        HUDController.i?.minimapHud.SetVisibility(true);
    }

    private IEnumerator StageSecuence()
    {
        yield return ShowTooltip(wellcomeTooltip);

        yield return new WaitUntil(() => avatarEditorClosed);
        yield return WaitIdleTime();

        yield return ShowTooltip(controlsTooltip);
        yield return WaitIdleTime();

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
}
