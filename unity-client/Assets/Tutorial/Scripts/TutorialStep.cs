using System.Collections;
using UnityEngine;

namespace DCL.Tutorial
{
    public interface ITutorialStep
    {
        void OnStepStart();
        IEnumerator OnStepExecute();
        IEnumerator OnStepPlayAnimationForHidding();
        void OnStepFinished();
    }

    /// <summary>
    /// Class that represents one of the steps included in the onboarding tutorial.
    /// </summary>
    public class TutorialStep : MonoBehaviour, ITutorialStep
    {
        protected const string STEP_FINISHED_ANIMATOR_TRIGGER = "StepFinished";
        protected Animator stepAnimator;

        /// <summary>
        /// Step initialization (occurs before OnStepExecute() execution).
        /// </summary>
        public virtual void OnStepStart()
        {
            stepAnimator = GetComponent<Animator>();
        }

        /// <summary>
        /// Executes the main flow of the step and waits for its finalization.
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerator OnStepExecute()
        {
            yield break;
        }

        /// <summary>
        /// Executes the final animation and waits for its finalization.
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerator OnStepPlayAnimationForHidding()
        {
            if (stepAnimator == null)
                yield break;

            stepAnimator.SetTrigger(STEP_FINISHED_ANIMATOR_TRIGGER);
            yield return null; // NOTE(Santi): It is needed to wait a frame for get the reference to the next animation clip correctly.
            yield return new WaitForSeconds(stepAnimator.GetCurrentAnimatorClipInfo(0)[0].clip.length);
        }

        /// <summary>
        /// Step finalization (occurs after OnStepExecute() execution).
        /// </summary>
        public virtual void OnStepFinished()
        {
        }
    }
}