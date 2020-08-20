using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Tutorial
{
    public interface ITutorialController
    {
        bool isTutorialEnabled { get; }
        void SetTutorialEnabled();
        void StartTutorialFromStep(int stepIndex);
        void SkipToNextStep();
    }

    /// <summary>
    /// Controller that handles all the flow related to the onboarding tutorial.
    /// </summary>
    public class TutorialController : MonoBehaviour, ITutorialController
    {
        public static TutorialController i { get; private set; }

        public bool isTutorialEnabled { get => isTutorialEnabledValue; }

        private const int TUTORIAL_FINISHED_MARK = -1;

        [Header("Steps Configuration")]
        [SerializeField] List<TutorialStep> steps = new List<TutorialStep>();

        [Header("Debugging")]
        public bool debugRunTutorial = false;
        public int debugStartingStepIndex;

        private bool isTutorialEnabledValue = false;
        private int currentStepIndex = TUTORIAL_FINISHED_MARK;
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

            if (nextStepIndex >= steps.Count)
                return;

            StartTutorialFromStep(nextStepIndex);
        }

        private void OnRenderingStateChanged(bool renderingEnabled, bool prevState)
        {
            if (!isTutorialEnabledValue || !renderingEnabled)
                return;

            if (debugRunTutorial)
                currentStepIndex = debugStartingStepIndex >= 0 && debugStartingStepIndex < steps.Count ? debugStartingStepIndex : TUTORIAL_FINISHED_MARK;
            else
                currentStepIndex = UserProfile.GetOwnUserProfile().tutorialStep;

            if (currentStepIndex == TUTORIAL_FINISHED_MARK || runningStep != null)
                return;

            StartTutorialFromStep(currentStepIndex);
        }

        private IEnumerator ExecuteSteps(int startingStepIndex)
        {
            if (startingStepIndex < 0 || startingStepIndex >= steps.Count)
                yield break;

            for (int i = startingStepIndex; i < steps.Count; i++)
            {
                var stepPrefab = steps[i];

                runningStep = Instantiate(stepPrefab).GetComponent<TutorialStep>();

                currentStepIndex = i;

                if (!debugRunTutorial)
                    UserProfile.GetOwnUserProfile().SetTutorialStepId(currentStepIndex);

                runningStep.OnStepStart();
                yield return runningStep.OnStepExecute();
                runningStep.OnStepFinished();

                Destroy(runningStep.gameObject);
            }

            if (!debugRunTutorial)
                UserProfile.GetOwnUserProfile().SetTutorialStepId(TUTORIAL_FINISHED_MARK);

            currentStepIndex = TUTORIAL_FINISHED_MARK;
            runningStep = null;
        }
    }
}
