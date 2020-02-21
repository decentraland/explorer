using UnityEngine;
using UnityEngine.UI;
using DCL.Interface;
using DCL.Tutorial;

public class NextSceneButtonHUD : MonoBehaviour
{
    [SerializeField] Button nextSceneButton;

    private bool hasWallet = false;
    private bool tutorialFinished = false;

    void Start()
    {
        nextSceneButton.onClick.AddListener(() => WebInterface.GoToNextTreasureHuntScene());

        UserProfile.GetOwnUserProfile().OnUpdate += OnUserProfileUpdated;
        TutorialController.i.OnTutorialFinished += OnTutorialFinished;

        gameObject.SetActive(false);
    }

    void OnUserProfileUpdated(UserProfile profile)
    {
        UserProfile.GetOwnUserProfile().OnUpdate -= OnUserProfileUpdated;
        hasWallet = profile.hasConnectedWeb3;
        tutorialFinished = TutorialController.i.isTutorialEnabled ? profile.tutorialStep == (int)TutorialStep.Id.FINISHED : true;
        CheckAndEnableButton();
    }

    void OnTutorialFinished()
    {
        TutorialController.i.OnTutorialFinished -= OnTutorialFinished;
        tutorialFinished = true;
        CheckAndEnableButton();
    }

    void CheckAndEnableButton()
    {
        gameObject.SetActive(hasWallet && tutorialFinished);
    }
}
