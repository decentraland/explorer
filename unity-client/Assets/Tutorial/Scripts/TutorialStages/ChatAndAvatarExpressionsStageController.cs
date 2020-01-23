﻿using System.Collections;
using UnityEngine;

public class ChatAndAvatarExpressionsStageController : TutorialStageController
{
    [SerializeField] TutorialTooltip chatTooltip = null;
    [SerializeField] TutorialTooltip avatarExpressionTooltip = null;
    [SerializeField] TutorialTooltip gotoCommandTooltip = null;


    public override void OnStageStart()
    {
        StartCoroutine(StageSecuence());
    }

    private IEnumerator StageSecuence()
    {
        yield return WaitIdleTime();

        yield return ShowTooltip(chatTooltip);
        yield return WaitIdleTime();

        yield return ShowTooltip(avatarExpressionTooltip);
        yield return WaitIdleTime();

        yield return ShowTooltip(gotoCommandTooltip);
        yield return WaitIdleTime();

        TutorialController.i?.SetRunningStageFinished();
    }
}
