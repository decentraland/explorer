using System;
using System.Collections;
using UnityEngine;

namespace DCL.Tutorial
{
    /// <summary>
    /// Class that represents one of the steps included in the onboarding tutorial.
    /// </summary>
    public class TutorialStep : MonoBehaviour
    {
        protected static int STEP_FINISHED_ANIMATOR_TRIGGER = Animator.StringToHash("StepFinished");

        internal event Action OnShowAnimationFinished;
        internal event Action OnJustAfterStepExecuted;

        [SerializeField] internal bool unlockCursorAtStart = false;
        [SerializeField] internal bool show3DTeacherAtStart = false;
        [SerializeField] internal protected RectTransform teacherPositionRef;
        [SerializeField] internal GameObject mainSection;
        [SerializeField] internal GameObject skipTutorialSection;
        [SerializeField] internal InputAction_Hold yesSkipInputAction;
        [SerializeField] internal InputAction_Hold noSkipInputAction;

        protected TutorialController tutorialController;
        protected Animator stepAnimator;
        protected MouseCatcher mouseCatcher;
        protected bool hideAnimationFinished = false;
        internal bool letInstantiation = true;

        /// <summary>
        /// Step initialization (occurs before OnStepExecute() execution).
        /// </summary>
        public virtual void OnStepStart()
        {
            tutorialController = TutorialController.i;
            stepAnimator = GetComponent<Animator>();

            mouseCatcher = InitialSceneReferences.i?.mouseCatcher;

            if (unlockCursorAtStart)
                mouseCatcher?.UnlockCursor();

            if (tutorialController != null)
            {
                tutorialController.ShowTeacher3DModel(show3DTeacherAtStart);

                if (show3DTeacherAtStart && teacherPositionRef != null)
                {
                    tutorialController.SetTeacherPosition(teacherPositionRef.position);

                    if (tutorialController.teacher.isHiddenByAnAnimation)
                        tutorialController.teacher.PlayAnimation(TutorialTeacher.TeacherAnimation.Reset);
                }
            }
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
            OnJustAfterStepExecuted?.Invoke();
            yield return PlayAndWaitForHideAnimation();
        }

        /// <summary>
        /// Step finalization (occurs after OnStepExecute() execution).
        /// </summary>
        public virtual void OnStepFinished()
        {
        }

        private void OnShowAnimationFinish()
        {
            OnShowAnimationFinished?.Invoke();
        }

        /// <summary>
        /// Warn about the finalization of the hide animation of the step
        /// </summary>
        private void OnHideAnimationFinish()
        {
            hideAnimationFinished = true;
        }

        private IEnumerator PlayAndWaitForHideAnimation()
        {
            if (stepAnimator == null)
                yield break;

            stepAnimator.SetTrigger(STEP_FINISHED_ANIMATOR_TRIGGER);
            yield return new WaitUntil(() => hideAnimationFinished);
        }
    }
}