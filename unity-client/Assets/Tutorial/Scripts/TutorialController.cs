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
        void SetTutorialEnabled();
        IEnumerator StartTutorialFromStep(int stepIndex);
        void SkipAllSteps();
        void SetUserTutorialStepAsCompleted(TutorialFinishStep step);
    }

    /// <summary>
    /// Controller that handles all the flow related to the onboarding tutorial.
    /// </summary>
    public class TutorialController : MonoBehaviour, ITutorialController
    {
        public static TutorialController i { get; private set; }

        [Header("Steps Configuration")]
        [SerializeField] List<TutorialStep> steps = new List<TutorialStep>();
        [SerializeField] float timeBetweenSteps = 0.5f;

        [Header("Debugging")]
        public bool debugRunTutorial = false;
        public int debugStartingStepIndex;

        private int currentStepIndex;
        private TutorialStep runningStep = null;
        private Coroutine executeStepsCoroutine;
        private float originalTimeBetweenSteps;

        private void Awake()
        {
            i = this;

            originalTimeBetweenSteps = timeBetweenSteps;
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
            if (!CommonScriptableObjects.rendererState.Get())
                CommonScriptableObjects.rendererState.OnChange += OnRenderingStateChanged;
            else
                OnRenderingStateChanged(true, false);
        }

        /// <summary>
        /// Starts to execute the tutorial from a specific step.
        /// </summary>
        /// <param name="stepIndex">First step to be executed.</param>
        public IEnumerator StartTutorialFromStep(int stepIndex)
        {
            if (runningStep != null)
            {
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

            executeStepsCoroutine = StartCoroutine(StartTutorialFromStep(steps.Count));
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
                currentStepIndex = (GetTutorialStepFromProfile() & (int)TutorialFinishStep.NewTutorialFinished) == 0 ? 0 : steps.Count;

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
        }
    }
}
