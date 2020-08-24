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
        private bool walkingActivated = false;

        private void Update()
        {
            if ((playerXAxisInpuAction.GetValue() != 0f || playerYAxisInputAction.GetValue() != 0f) && walkingActivated)
                timeWalking += Time.deltaTime;
        }

        public override void OnStepStart()
        {
            base.OnStepStart();

            walkingInputAction.OnStarted += WalkingInputAction_OnStarted;
            walkingInputAction.OnFinished += WalkingInputAction_OnFinished;
        }

        public override IEnumerator OnStepExecute()
        {
            yield return new WaitUntil(() => timeWalking >= minWalkingTime);

            yield return base.OnStepExecute();
        }

        public override void OnStepFinished()
        {
            base.OnStepFinished();

            walkingInputAction.OnStarted -= WalkingInputAction_OnStarted;
            walkingInputAction.OnFinished -= WalkingInputAction_OnFinished;
        }

        private void WalkingInputAction_OnStarted(DCLAction_Hold action)
        {
            walkingActivated = true;
        }

        private void WalkingInputAction_OnFinished(DCLAction_Hold action)
        {
            walkingActivated = false;
        }
    }
}