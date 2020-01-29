using System.Collections;
using UnityEngine;

public class ChatAndAvatarExpressionsStageController : TutorialStageController
{
    [SerializeField] TutorialTooltip chatTooltip = null;
    [SerializeField] TutorialTooltip avatarExpressionTooltip = null;
    [SerializeField] TutorialTooltip gotoCommandTooltip = null;
    [SerializeField] TutorialTooltip avatarHUDTooltip = null;


    public override void OnStageStart()
    {
        StartCoroutine(StageSecuence());
    }

    private IEnumerator StageSecuence()
    {
        yield return WaitIdleTime();

        TutorialController.i?.SetChatVisible(true);

        yield return ShowTooltip(chatTooltip);
        yield return WaitIdleTime();

        // TODO: show avatar expressions
        //HUDController.i?.expressionsHud.SetVisibility(true);
        yield return ShowTooltip(avatarExpressionTooltip);
        yield return WaitIdleTime();

        yield return ShowTooltip(gotoCommandTooltip);
        yield return WaitIdleTime();

        HUDController.i?.avatarHud.SetVisibility(true);
        yield return ShowTooltip(avatarHUDTooltip);
        yield return WaitIdleTime();

        TutorialController.i?.SetRunningStageFinished();
    }
}
