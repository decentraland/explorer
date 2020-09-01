using DCL.Controllers;
using DCL.Interface;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Tutorial
{
    public interface ITutorialController
    {
        void SetTutorialEnabled(string fromDeepLink);
        void SetTutorialDisabled();
        IEnumerator StartTutorialFromStep(int stepIndex);
        void SetUserTutorialStepAsCompleted(TutorialController.TutorialFinishStep step);
        void SetTimeBetweenSteps(float newTime);
        void ShowTeacher3DModel(bool active);
        void SetTeacherPosition(Vector2 position, bool animated = true);
        void PlayTeacherAnimation(TutorialTeacher.TeacherAnimation animation);
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

        internal HUDController hudController { get => HUDController.i; }

        [Header("General Configuration")]
        [SerializeField] float timeBetweenSteps = 0.5f;

        [Header("Tutorial Steps on Genesis Plaza")]
        [SerializeField] List<TutorialStep> stepsOnGenesisPlaza = new List<TutorialStep>();

        [Header("Tutorial Steps from Deep Link")]
        [SerializeField] List<TutorialStep> stepsFromDeepLink = new List<TutorialStep>();

        [Header("Tutorial Steps on Genesis Plaza (after Deep Link)")]
        [SerializeField] List<TutorialStep> stepsOnGenesisPlazaAfterDeepLink = new List<TutorialStep>();

        [Header("3D Model Teacher")]
        [SerializeField] RawImage teacherRawImage;
        [SerializeField] TutorialTeacher teacher;
        [SerializeField] float teacherMovementSpeed = 4f;
        [SerializeField] AnimationCurve teacherMovementCurve;

        [Header("Debugging")]
        public bool debugRunTutorial = false;
        public int debugStartingStepIndex;
        public bool debugOpenedFromDeepLink = false;

        private bool isRunning = false;
        private int currentStepIndex;
        private TutorialStep runningStep = null;
        private Coroutine executeStepsCoroutine;
        private Coroutine teacherMovementCoroutine;
        private bool openedFromDeepLink = false;
        private bool tutorialFromDeepLinkDone = false;
        private bool playerIsInGenesisPlaza = false;

        private void Awake()
        {
            i = this;
        }

        private void Start()
        {
            ShowTeacher3DModel(false);

            if (debugRunTutorial)
                SetTutorialEnabled(debugOpenedFromDeepLink.ToString());
        }

        private void OnDestroy()
        {
            if (executeStepsCoroutine != null)
                StopCoroutine(executeStepsCoroutine);

            SetTutorialDisabled();
        }

        /// <summary>
        /// Enables the tutorial controller and waits for the RenderingState is enabled to start to execute the corresponding tutorial steps.
        /// </summary>
        public void SetTutorialEnabled(string fromDeepLink)
        {
            if (isRunning)
                return;

            isRunning = true;
            openedFromDeepLink = Convert.ToBoolean(fromDeepLink);

            if (hudController != null && hudController.emailPromptHud != null)
            {
                hudController.emailPromptHud.OnSetEmailFlag += EmailPromptHud_OnSetEmailFlag;
                hudController.emailPromptHud.waitForEndOfTutorial = true;
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
            ShowTeacher3DModel(false);

            if (hudController != null && hudController.emailPromptHud != null)
            {
                hudController.emailPromptHud.OnSetEmailFlag -= EmailPromptHud_OnSetEmailFlag;
                hudController.emailPromptHud.waitForEndOfTutorial = false;
            }

            CommonScriptableObjects.rendererState.OnChange -= OnRenderingStateChanged;
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

            if (playerIsInGenesisPlaza)
            {
                if (tutorialFromDeepLinkDone)
                    yield return ExecuteSteps(stepsOnGenesisPlazaAfterDeepLink, stepIndex);
                else
                    yield return ExecuteSteps(stepsOnGenesisPlaza, stepIndex);
                
            }
            else if (openedFromDeepLink)
            {
                yield return ExecuteSteps(stepsFromDeepLink, stepIndex);
                tutorialFromDeepLinkDone = true;
            }
            else
            {
                yield break;
            }
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

        public void ShowTeacher3DModel(bool active)
        {
            teacherRawImage.gameObject.SetActive(active);
        }

        public void SetTeacherPosition(Vector2 position, bool animated = true)
        {
            if (teacherMovementCoroutine != null)
                StopCoroutine(teacherMovementCoroutine);

            if (animated)
                teacherMovementCoroutine = StartCoroutine(MoveTeacher(teacherRawImage.rectTransform.position, position));
            else
                teacherRawImage.rectTransform.position = position;
        }

        public void PlayTeacherAnimation(TutorialTeacher.TeacherAnimation animation)
        {
            teacher.PlayAnimation(animation);
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

            playerIsInGenesisPlaza = IsPlayerInsideGenesisPlaza();

            if (debugRunTutorial)
                currentStepIndex = debugStartingStepIndex >= 0 ? debugStartingStepIndex : 0;
            else
                currentStepIndex = 0;

            executeStepsCoroutine = StartCoroutine(StartTutorialFromStep(currentStepIndex));
        }

        private IEnumerator ExecuteSteps(List<TutorialStep> steps, int startingStepIndex)
        {
            for (int i = startingStepIndex; i < steps.Count; i++)
            {
                var stepPrefab = steps[i];

                runningStep = Instantiate(stepPrefab, this.transform).GetComponent<TutorialStep>();

                currentStepIndex = i;

                runningStep.OnStepStart();
                yield return runningStep.OnStepExecute();
                PlayTeacherAnimation(TutorialTeacher.TeacherAnimation.StepCompleted);
                yield return runningStep.OnStepPlayAnimationForHidding();
                runningStep.OnStepFinished();

                Destroy(runningStep.gameObject);

                if (i < steps.Count - 1 && timeBetweenSteps > 0)
                    yield return new WaitForSeconds(timeBetweenSteps);
            }

            if (!debugRunTutorial && !openedFromDeepLink)
                SetUserTutorialStepAsCompleted(TutorialFinishStep.NewTutorialFinished);

            runningStep = null;

            SetTutorialDisabled();
        }

        private IEnumerator MoveTeacher(Vector2 fromPosition, Vector2 toPosition)
        {
            float t = 0f;

            while (Vector2.Distance(teacherRawImage.rectTransform.position, toPosition) > 0)
            {
                t += teacherMovementSpeed * Time.deltaTime;
                if (t <= 1.0f)
                    teacherRawImage.rectTransform.position = Vector2.Lerp(fromPosition, toPosition, teacherMovementCurve.Evaluate(t));
                else
                    teacherRawImage.rectTransform.position = toPosition;

                yield return null;
            }
        }

        private void EmailPromptHud_OnSetEmailFlag()
        {
            SetUserTutorialStepAsCompleted(TutorialFinishStep.EmailRequested);
        }

        private bool IsPlayerInsideGenesisPlaza()
        {
            if (SceneController.i == null || SceneController.i.currentSceneId == null)
                return false;

            Vector2Int genesisPlazaBaseCoords = new Vector2Int(-9, -9);
            ParcelScene currentScene = SceneController.i.loadedScenes[SceneController.i.currentSceneId];

            if (currentScene != null && currentScene.IsInsideSceneBoundaries(genesisPlazaBaseCoords))
                return true;

            return false;
        }
    }
}
