using System.Collections;
using UnityEngine;

namespace DCL.Tutorial
{
    /// <summary>
    /// Class that represents the onboarding tutorial step related to how to walk with the avatar.
    /// </summary>
    public class TutorialStep_AvatarWalking : TutorialStep_WithProgressBar
    {
        [SerializeField] InputAction_Measurable playerXAxisInpuAction;
        [SerializeField] InputAction_Measurable playerYAxisInputAction;
        [SerializeField] InputAction_Hold walkingInputAction;
        [SerializeField] float minWalkingTime = 2f;

        private float timeWalking = 0f;

        private void Update()
        {
            if ((playerXAxisInpuAction.GetValue() != 0f || playerYAxisInputAction.GetValue() != 0f) && walkingInputAction.isOn)
                timeWalking += Time.deltaTime;
        }

        public override IEnumerator OnStepExecute()
        {
            yield return new WaitUntil(() => timeWalking >= minWalkingTime);

            tutorialController.PlayTeacherAnimation(TutorialTeacher.TeacherAnimation.Goodbye);
        }
    }
}