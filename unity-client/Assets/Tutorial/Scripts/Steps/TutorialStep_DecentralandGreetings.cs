using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Tutorial
{
    /// <summary>
    /// Class that represents the onboarding tutorial step related to the greetings showed when we are not in Genesis Plaza.
    /// </summary>
    public class TutorialStep_DecentralandGreetings : TutorialStep
    {
        [SerializeField] Button okButton;
        [SerializeField] TMP_Text titleText;

        private bool stepIsFinished = false;

        public override void OnStepStart()
        {
            base.OnStepStart();

            titleText.text = titleText.text.Replace("{userName}", UserProfile.GetOwnUserProfile().userName);

            okButton.onClick.AddListener(OnOkButtonClick);
        }

        public override IEnumerator OnStepExecute()
        {
            yield return new WaitUntil(() => stepIsFinished);

            if (tutorialController != null)
                tutorialController.PlayTeacherAnimation(TutorialTeacher.TeacherAnimation.QuickGoodbye);
        }

        private void OnOkButtonClick()
        {
            stepIsFinished = true;
        }
    }
}