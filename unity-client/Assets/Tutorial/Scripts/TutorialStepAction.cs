
using UnityEngine;

namespace DCL.Tutorial
{
    public class TutorialStepAction : MonoBehaviour
    {
        protected static int KEY_LOOP_ANIMATOR_BOOL = Animator.StringToHash("KeyLoop");

        [SerializeField] internal TutorialStep step;

        private Animator actionAnimator;

        private void Start()
        {
            actionAnimator = GetComponent<Animator>();

            if (actionAnimator != null)
                actionAnimator.SetBool(KEY_LOOP_ANIMATOR_BOOL, false);

            if (step != null)
                step.OnShowAnimationFinished += Step_OnShowAnimationFinished;
        }

        private void OnDestroy()
        {
            if (step != null)
                step.OnShowAnimationFinished -= Step_OnShowAnimationFinished;
        }

        private void Step_OnShowAnimationFinished()
        {
            if (actionAnimator != null)
                actionAnimator.SetBool(KEY_LOOP_ANIMATOR_BOOL, true);
        }
    }
}