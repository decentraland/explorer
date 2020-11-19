using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DCL.Tutorial;

public class TutorialStep_PutSceneObject : TutorialStep
{
    [SerializeField] AudioEvent audioEventSuccess;
    bool sceneObjectSet = false;

    BuilderInWorldController buildModeController;
    public override void OnStepStart()
    {
        base.OnStepStart();
        buildModeController = FindObjectOfType<BuilderInWorldController>();
        buildModeController.OnSceneObjectPlaced += SceneObjectSelected;
    }

    public override void OnStepFinished()
    {
        base.OnStepFinished();

        buildModeController.OnSceneObjectPlaced -= SceneObjectSelected;
    }


    void SceneObjectSelected()
    {
        sceneObjectSet = true;
    }

    public override IEnumerator OnStepExecute()
    {
        yield return new WaitUntil(() => sceneObjectSet);
        audioEventSuccess.Play(true);
    }
}
