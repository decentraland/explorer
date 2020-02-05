using UnityEngine;

public class ClaimNameStageController : TutorialStageController
{
    public override void OnStageStart()
    {
        base.OnStageStart();
    }

    public void ContinueAsGuestButtonAction()
    {
        gameObject.SetActive(false);

        TutorialController.i?.SetRunningStageFinished();
    }

    public void ClaimNameButtonAction()
    {
        Application.OpenURL("http://avatars.decentraland.org");

        ContinueAsGuestButtonAction();
    }
}
