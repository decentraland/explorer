using System.Collections;
using UnityEngine;

namespace DCL.Tutorial
{
    /// <summary>
    /// Class that represents the onboarding tutorial step related to how to open the player menu options.
    /// </summary>
    public class TutorialStep_OpenPlayerMenuOptions : TutorialStep_WithProgressBar
    {
        [SerializeField] InputAction_Hold confirmInputAction;

        public override IEnumerator OnStepExecute()
        {
            yield return new WaitUntil(() => confirmInputAction.isOn);

            tutorialController.PlayTeacherAnimation(TutorialTeacher.TeacherAnimation.Goodbye);
        }
    }
}