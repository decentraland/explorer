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
        void SetTutorialEnabled();
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

        [Header("Steps Configuration")]
        [SerializeField] float timeBetweenSteps = 0.5f;
        [SerializeField] List<TutorialStep> steps = new List<TutorialStep>();

        [Header("3D Model Teacher")]
        [SerializeField] RawImage teacherRawImage;
        [SerializeField] TutorialTeacher teacher;
        [SerializeField] float teacherMovementSpeed = 4f;
        [SerializeField] AnimationCurve teacherMovementCurve;

        [Header("Debugging")]
        public bool debugRunTutorial = false;
        public int debugStartingStepIndex;

        private bool isRunning = false;
        private int currentStepIndex;
        private TutorialStep runningStep = null;
        private Coroutine executeStepsCoroutine;
        private Coroutine teacherMovementCoroutine;

        private void Awake()
        {
            i = this;
        }

        private void Start()
        {
            ShowTeacher3DModel(false);

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

            yield return ExecuteSteps(stepIndex);
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
                PlayTeacherAnimation(TutorialTeacher.TeacherAnimation.StepCompleted);
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
    }
}
