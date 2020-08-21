using DCL.Interface;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Tutorial
{
    [Flags]
    public enum TutorialFinishStep
    {
        None = 0,
        OldTutorialValue = 99, // NOTE: old tutorial set tutorialStep to 99 when finished
        EmailRequested = 128,
        NewTutorialFinished = 256
    }

    public interface ITutorialController
    {
        bool isTutorialEnabled { get; }
        void SetTutorialEnabled();
        void StartTutorialFromStep(int stepIndex);
        void SkipToNextStep();
        void SetStepCompleted(int step);
    }

    /// <summary>
    /// Controller that handles all the flow related to the onboarding tutorial.
    /// </summary>
    public class TutorialController : MonoBehaviour, ITutorialController
    {
        public static TutorialController i { get; private set; }

        public bool isTutorialEnabled { get => isTutorialEnabledValue; }

        [Header("Steps Configuration")]
        [SerializeField] List<TutorialStep> steps = new List<TutorialStep>();
        [SerializeField] float timeBetweenSteps = 0.5f;

        [Header("Debugging")]
        public bool debugRunTutorial = false;
        public int debugStartingStepIndex;

        private bool isTutorialEnabledValue = false;
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
            CommonScriptableObjects.rendererState.OnChange -= OnRenderingStateChanged;
        }

        /// <summary>
        /// Enables the tutorial controller and waits for the RenderingState is enabled to start to execute the corresponding tutorial steps.
        /// </summary>
        public void SetTutorialEnabled()
        {
            CommonScriptableObjects.rendererState.OnChange += OnRenderingStateChanged;
            isTutorialEnabledValue = true;
        }

        /// <summary>
        /// Starts to execute the tutorial from a specific step
        /// (It is necessary that the tutorial is enabled before).
        /// </summary>
        /// <param name="stepIndex">First step to be executed.</param>
        public void StartTutorialFromStep(int stepIndex)
        {
            if (!isTutorialEnabledValue)
                return;

            if (runningStep != null)
            {
                StopCoroutine(executeStepsCoroutine);

                runningStep.OnStepFinished();
                Destroy(runningStep.gameObject);

                runningStep = null;
            }

            executeStepsCoroutine = StartCoroutine(ExecuteSteps(stepIndex));
        }

        /// <summary>
        /// Skips the current running step and executes the next one.
        /// </summary>
        public void SkipToNextStep()
        {
            int nextStepIndex = currentStepIndex + 1;
            StartTutorialFromStep(nextStepIndex);
        }

        /// <summary>
        /// Skips the all the steps and finalize the tutorial.
        /// </summary>
        public void SkipAllSteps()
        {
            StartTutorialFromStep(steps.Count);
        }

        public void SetStepCompleted(int step)
        {
            WebInterface.SaveUserTutorialStep(GetTutorialStepFromProfile() | step);
        }

        private int GetTutorialStepFromProfile()
        {
            return UserProfile.GetOwnUserProfile().tutorialStep;
        }

        private void OnRenderingStateChanged(bool renderingEnabled, bool prevState)
        {
            if (!isTutorialEnabledValue || !renderingEnabled)
                return;

            if (debugRunTutorial)
                currentStepIndex = debugStartingStepIndex >= 0 ? debugStartingStepIndex : 0;
            else
                currentStepIndex = (GetTutorialStepFromProfile() & (int)TutorialFinishStep.NewTutorialFinished) == 0 ? 0 : steps.Count;

            StartTutorialFromStep(currentStepIndex);
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
                SetStepCompleted((int)TutorialFinishStep.NewTutorialFinished);

            runningStep = null;
        }
    }
}
