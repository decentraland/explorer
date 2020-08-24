using System.Collections;
using UnityEngine;

namespace DCL.Tutorial
{
    /// <summary>
    /// Class that represents the onboarding tutorial step related to how to run with the avatar.
    /// </summary>
    public class TutorialStep_AvatarMovement : TutorialStep_WithProgressBar
    {
        [SerializeField] InputAction_Measurable playerXAxisInpuAction;
        [SerializeField] InputAction_Measurable playerYAxisInputAction;
        [SerializeField] float minRunningTime = 2f;

        private float timeRunning = 0f;

        private void Update()
        {
            if (playerXAxisInpuAction.GetValue() != 0f || playerYAxisInputAction.GetValue() != 0f)
                timeRunning += Time.deltaTime;
        }

        public override void OnStepStart()
        {
            base.OnStepStart();

            TutorialController.i?.SetTimeBetweenSteps(0);
        }

        public override IEnumerator OnStepExecute()
        {
            yield return new WaitUntil(() => timeRunning >= minRunningTime);
        }
    }
}