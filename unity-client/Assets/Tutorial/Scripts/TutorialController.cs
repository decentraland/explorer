using DCL.Interface;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Tutorial
{
    public interface ITutorialController
    {
        void SetTutorialEnabled();
        void SetTutorialDisabled();
        IEnumerator StartTutorialFromStep(int stepIndex, bool abortRunningStep = false);
        void SkipAllSteps();
        void SetUserTutorialStepAsCompleted(TutorialController.TutorialFinishStep step);
    }

    /// <summary>
    /// Controller that handles all the flow related to the onboarding tutorial.
    /// </summary>
    public class TutorialController : MonoBehaviour, ITutorialController
    {
        [Flags]
        public enum TutorialFinishStep
        {
            None = 0,
            OldTutorialValue = 99, // NOTE: old tutorial set tutorialStep to 99 when finished
            EmailRequested = 128,
            NewTutorialFinished = 256
        }

        public static TutorialController i { get; private set; }

        public bool isRunning { get; private set; } = false;
        public HUDController hudController { get => HUDController.i; }

        [Header("Steps Configuration")]
        [SerializeField] List<TutorialStep> steps = new List<TutorialStep>();
        [SerializeField] float timeBetweenSteps = 0.5f;

        [Header("Debugging")]
        public bool debugRunTutorial = false;
        public int debugStartingStepIndex;

        private int currentStepIndex;
        private TutorialStep runningStep = null;
        private Coroutine executeStepsCoroutine;

        private void Awake()
        {
            i = this;
        }

        private void Start()
        {
            if (debugRunTutorial)
                SetTutorialEnabled();
        }

        private void OnDestroy()
        {
            SetTutorialDisabled();
        }

        /// <summary>
        /// Enables the tutorial controller and waits for the RenderingState is enabled to start to execute the corresponding tutorial steps.
        /// </summary>
        public void SetTutorialEnabled()
        {
            if (isRunning)
                return;

            isRunning = true;

            if (hudController != null && hudController.emailPromptHud != null)
            {
                hudController.emailPromptHud.OnSetEmailFlag += EmailPromptHud_OnSetEmailFlag;
            }

            if (!CommonScriptableObjects.rendererState.Get())
                CommonScriptableObjects.rendererState.OnChange += OnRenderingStateChanged;
            else
                OnRenderingStateChanged(true, false);
        }

        /// <summary>
        /// Disables the tutorial controller.
        /// </summary>
        public void SetTutorialDisabled()
        {
            isRunning = false;

            if (hudController != null && hudController.emailPromptHud != null)
            {
                hudController.emailPromptHud.OnSetEmailFlag -= EmailPromptHud_OnSetEmailFlag;
            }

            CommonScriptableObjects.rendererState.OnChange -= OnRenderingStateChanged;
        }

        /// <summary>
        /// Starts to execute the tutorial from a specific step.
        /// </summary>
        /// <param name="stepIndex">First step to be executed.</param>
        public IEnumerator StartTutorialFromStep(int stepIndex, bool abortRunningStep = false)
        {
            if (runningStep != null)
            {
                if (abortRunningStep)
                    yield return runningStep.AbortStep();
                else
                    yield return runningStep.OnStepPlayAnimationForHidding();

                runningStep.OnStepFinished();
                Destroy(runningStep.gameObject);

                runningStep = null;
            }

            yield return ExecuteSteps(stepIndex);
        }

        /// <summary>
        /// Skips all the steps and finalize the tutorial.
        /// </summary>
        public void SkipAllSteps()
        {
            if (executeStepsCoroutine != null)
                StopCoroutine(executeStepsCoroutine);

            executeStepsCoroutine = StartCoroutine(StartTutorialFromStep(steps.Count, true));
        }

        /// <summary>
        /// Mark the tutorial as finished in the kernel side.
        /// </summary>
        /// <param name="finishStepType">A value from TutorialFinishStep enum.</param>
        public void SetUserTutorialStepAsCompleted(TutorialFinishStep finishStepType)
        {
            WebInterface.SaveUserTutorialStep(GetTutorialStepFromProfile() | (int)finishStepType);
        }

        public void SetTimeBetweenSteps(float newTime)
        {
            timeBetweenSteps = newTime;
        }

        private int GetTutorialStepFromProfile()
        {
            return UserProfile.GetOwnUserProfile().tutorialStep;
        }

        private void OnRenderingStateChanged(bool renderingEnabled, bool prevState)
        {
            if (!renderingEnabled)
                return;

            CommonScriptableObjects.rendererState.OnChange -= OnRenderingStateChanged;

            if (debugRunTutorial)
                currentStepIndex = debugStartingStepIndex >= 0 ? debugStartingStepIndex : 0;
            else
                currentStepIndex = 0;

            executeStepsCoroutine = StartCoroutine(StartTutorialFromStep(currentStepIndex));
        }

        private IEnumerator ExecuteSteps(int startingStepIndex)
        {
            for (int i = startingStepIndex; i < steps.Count; i++)
            {
                var stepPrefab = steps[i];

                runningStep = Instantiate(stepPrefab, this.transform).GetComponent<TutorialStep>();

                currentStepIndex = i;

                runningStep.OnStepStart();
                yield return runningStep.OnStepExecute();
                yield return runningStep.OnStepPlayAnimationForHidding();
                runningStep.OnStepFinished();

                Destroy(runningStep.gameObject);

                if (i < steps.Count - 1 && timeBetweenSteps > 0)
                    yield return new WaitForSeconds(timeBetweenSteps);
            }

            if (!debugRunTutorial)
                SetUserTutorialStepAsCompleted(TutorialFinishStep.NewTutorialFinished);

            runningStep = null;

            SetTutorialDisabled();
        }

        private void EmailPromptHud_OnSetEmailFlag()
        {
            SetUserTutorialStepAsCompleted(TutorialFinishStep.EmailRequested);
        }
    }
}
