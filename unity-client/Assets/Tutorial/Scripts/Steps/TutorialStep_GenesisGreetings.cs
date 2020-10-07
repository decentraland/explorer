using System.Collections;
using DCL.Controllers;
using DCL.Interface;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Tutorial
{
    /// <summary>
    /// Class that represents the onboarding tutorial step related to the greetings showed in Genesis Plaza.
    /// </summary>
    public class TutorialStep_GenesisGreetings : TutorialStep
    {
        private const int TEACHER_CANVAS_SORT_ORDER_START = 4;

        [SerializeField] Button okButton;
        [SerializeField] TMP_Text titleText;

        private bool stepIsFinished = false;
        private int defaultTeacherCanvasSortOrder;

        public override void OnStepStart()
        {
            base.OnStepStart();

            titleText.text = titleText.text.Replace("{userName}", UserProfile.GetOwnUserProfile().userName);

            okButton.onClick.AddListener(OnOkButtonClick);

            if (tutorialController)
            {
                tutorialController.SetEagleEyeCameraPosition(new Vector3(30, 30, -50), CommonScriptableObjects.playerUnityPosition.Get());
                tutorialController.SetEagleEyeCameraActive(true);

                defaultTeacherCanvasSortOrder = tutorialController.teacherCanvas.sortingOrder;
                tutorialController.SetTeacherCanvasSortingOrder(TEACHER_CANVAS_SORT_ORDER_START);

                tutorialController.hudController?.taskbarHud?.SetVisibility(false);

                if (!tutorialController.alreadyOpenedFromDeepLink && SceneController.i != null)
                {
                    WebInterface.SendSceneExternalActionEvent(SceneController.i.currentSceneId,"tutorial","begin");
                }
            }
        }

        public override IEnumerator OnStepExecute()
        {
            yield return new WaitUntil(() => stepIsFinished);
        }

        public override void OnStepFinished()
        {
            base.OnStepFinished();
            tutorialController.SetTeacherCanvasSortingOrder(defaultTeacherCanvasSortOrder);
        }

        private void OnOkButtonClick()
        {
            stepIsFinished = true;
        }
    }
}