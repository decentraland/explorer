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
        yield return WaitIdleTime();

        // NOTE: change this on teleport
        TutorialController.i?.SetRunningStageFinished();
    }

    private void OnAvatarEditorVisibilityChanged(bool visible)
    {
        if (avatarEditorHUD != null)
        {
            avatarEditorHUD.OnVisibilityChanged -= OnAvatarEditorVisibilityChanged;
            avatarEditorClosed = true;
        }
    }
}
