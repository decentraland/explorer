using DCL.Controllers;
using DCL.Interface;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;

namespace DCL.Tutorial
{
    /// <summary>
    /// Controller that handles all the flow related to the onboarding tutorial.
    /// </summary>
    public class TutorialController : MonoBehaviour
    {
        [Flags]
        public enum TutorialFinishStep
        {
            None = 0,
            OldTutorialValue = 99, // NOTE: old tutorial set tutorialStep to 99 when finished
            EmailRequested = 128, // NOTE: old email prompt set tutorialStep to 128 when finished
            NewTutorialFinished = 256
        }

        internal enum TutorialPath
        {
            FromGenesisPlaza,
            FromDeepLink,
            FromGenesisPlazaAfterDeepLink
        }

        public static TutorialController i { get; private set; }

        public HUDController hudController { get => HUDController.i; }

        [Header("General Configuration")]
        [SerializeField] internal int tutorialVersion = 1;
        [SerializeField] internal float timeBetweenSteps = 0.5f;
        [SerializeField] internal bool sendStats = true;

        [Header("Tutorial Steps on Genesis Plaza")]
        [SerializeField] internal List<TutorialStep> stepsOnGenesisPlaza = new List<TutorialStep>();

        [Header("Tutorial Steps from Deep Link")]
        [SerializeField] internal List<TutorialStep> stepsFromDeepLink = new List<TutorialStep>();

        [Header("Tutorial Steps on Genesis Plaza (after Deep Link)")]
        [SerializeField] internal List<TutorialStep> stepsOnGenesisPlazaAfterDeepLink = new List<TutorialStep>();

        [Header("3D Model Teacher")]
        [SerializeField] internal Camera teacherCamera;
        [SerializeField] internal RawImage teacherRawImage;
        [SerializeField] internal TutorialTeacher teacher;
        [SerializeField] internal float teacherMovementSpeed = 4f;
        [SerializeField] internal AnimationCurve teacherMovementCurve;
        [SerializeField] internal Canvas teacherCanvas;

        [Header("Eagle Eye Camera")]
        [SerializeField] internal CinemachineVirtualCamera eagleEyeCamera;
        [SerializeField] internal Vector3 eagleCamInitPosition = new Vector3(30, 30, -50);
        [SerializeField] internal Vector3 eagleCamInitLookAtPoint = new Vector3(0, 0, 0);
        [SerializeField] internal bool eagleCamRotationActived = true;
        [SerializeField] internal float eagleCamRotationSpeed = 1f;

        [Header("Debugging")]
        [SerializeField] internal bool debugRunTutorial = false;
        [SerializeField] internal int debugStartingStepIndex;
        [SerializeField] internal bool debugOpenedFromDeepLink = false;

        internal bool isRunning = false;
        internal bool openedFromDeepLink = false;
        internal bool alreadyOpenedFromDeepLink = false;
        internal bool playerIsInGenesisPlaza = false;
        internal bool markTutorialAsCompleted = false;
        internal TutorialStep runningStep = null;

        private int currentStepIndex;
        private Coroutine executeStepsCoroutine;
        private Coroutine teacherMovementCoroutine;
        private Coroutine eagleEyeRotationCoroutine;

        private void Awake()
        {
            i = this;
            ShowTeacher3DModel(false);
        }

        private void Start()
        {
            if (debugRunTutorial)
                SetTutorialEnabled(debugOpenedFromDeepLink.ToString());
        }

        private void OnDestroy()
        {
            SetTutorialDisabled();

            if (hudController != null)
            {
                if (hudController.emailPromptHud != null)
                    hudController.emailPromptHud.waitForEndOfTutorial = false;

                if (hudController.goToGenesisPlazaHud != null)
                {
                    hudController.goToGenesisPlazaHud.OnBeforeGoToGenesisPlaza -= GoToGenesisPlazaHud_OnBeforeGoToGenesisPlaza;
                    hudController.goToGenesisPlazaHud.OnAfterGoToGenesisPlaza -= GoToGenesisPlazaHud_OnAfterGoToGenesisPlaza;
                }
            }

            NotificationsController.disableWelcomeNotification = false;
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

            if (hudController != null)
            {
                if (hudController.emailPromptHud != null)
                    hudController.emailPromptHud.waitForEndOfTutorial = true;

                if (hudController.goToGenesisPlazaHud != null)
                {
                    hudController.goToGenesisPlazaHud.OnBeforeGoToGenesisPlaza -= GoToGenesisPlazaHud_OnBeforeGoToGenesisPlaza;
                    hudController.goToGenesisPlazaHud.OnBeforeGoToGenesisPlaza += GoToGenesisPlazaHud_OnBeforeGoToGenesisPlaza;
                    hudController.goToGenesisPlazaHud.OnAfterGoToGenesisPlaza -= GoToGenesisPlazaHud_OnAfterGoToGenesisPlaza;
                    hudController.goToGenesisPlazaHud.OnAfterGoToGenesisPlaza += GoToGenesisPlazaHud_OnAfterGoToGenesisPlaza;
                }
            }

            NotificationsController.disableWelcomeNotification = true;

            WebInterface.SetDelightedSurveyEnabled(false);

            if (!CommonScriptableObjects.rendererState.Get())
                CommonScriptableObjects.rendererState.OnChange += OnRenderingStateChanged;
            else
                OnRenderingStateChanged(true, false);
        }

        /// <summary>
        /// Stop and disables the tutorial controller.
        /// </summary>
        public void SetTutorialDisabled()
        {
            if (executeStepsCoroutine != null)
            {
                StopCoroutine(executeStepsCoroutine);
                executeStepsCoroutine = null;
            }

            if (runningStep != null)
            {
                Destroy(runningStep.gameObject);
                runningStep = null;
            }

            isRunning = false;
            ShowTeacher3DModel(false);
            WebInterface.SetDelightedSurveyEnabled(true);

            if (!alreadyOpenedFromDeepLink && SceneController.i != null)
            {
                WebInterface.SendSceneExternalActionEvent(SceneController.i.currentSceneId,"tutorial","end");
            }

            NotificationsController.disableWelcomeNotification = false;

            if (hudController != null && hudController.emailPromptHud != null)
                hudController.emailPromptHud.waitForEndOfTutorial = false;

            CommonScriptableObjects.rendererState.OnChange -= OnRenderingStateChanged;
        }

        /// <summary>
        /// Starts to execute the tutorial from a specific step (It is needed to call SetTutorialEnabled() before).
        /// </summary>
        /// <param name="stepIndex">First step to be executed.</param>
        public IEnumerator StartTutorialFromStep(int stepIndex)
        {
            if (!isRunning)
                yield break;

            if (runningStep != null)
            {
                runningStep.OnStepFinished();
                Destroy(runningStep.gameObject);
                runningStep = null;
            }

            if (playerIsInGenesisPlaza)
            {
                markTutorialAsCompleted = true;

                if (alreadyOpenedFromDeepLink)
                    yield return ExecuteSteps(TutorialPath.FromGenesisPlazaAfterDeepLink, stepIndex);
                else
                    yield return ExecuteSteps(TutorialPath.FromGenesisPlaza, stepIndex);

            }
            else if (openedFromDeepLink)
            {
                markTutorialAsCompleted = false;
                alreadyOpenedFromDeepLink = true;
                yield return ExecuteSteps(TutorialPath.FromDeepLink, stepIndex);
            }
            else
            {
                SetTutorialDisabled();
                yield break;
            }
        }

        /// <summary>
        /// Shows the teacher that will be guiding along the tutorial.
        /// </summary>
        /// <param name="active">True for show the teacher.</param>
        public void ShowTeacher3DModel(bool active)
        {
            teacherCamera.enabled = active;
            teacherRawImage.gameObject.SetActive(active);
        }

        /// <summary>
        /// Move the tutorial teacher to a specific position.
        /// </summary>
        /// <param name="position">Target position.</param>
        /// <param name="animated">True for apply a smooth movement.</param>
        public void SetTeacherPosition(Vector2 position, bool animated = true)
        {
            if (teacherMovementCoroutine != null)
                StopCoroutine(teacherMovementCoroutine);

            if (animated)
                teacherMovementCoroutine = StartCoroutine(MoveTeacher(teacherRawImage.rectTransform.position, position));
            else
                teacherRawImage.rectTransform.position = position;
        }

        /// <summary>
        /// Plays a specific animation on the tutorial teacher.
        /// </summary>
        /// <param name="animation">Animation to apply.</param>
        public void PlayTeacherAnimation(TutorialTeacher.TeacherAnimation animation)
        {
            teacher.PlayAnimation(animation);
        }

        /// <summary>
        /// Set sort order for canvas containing teacher RawImage
        /// </summary>
        /// <param name="sortOrder"></param>
        public void SetTeacherCanvasSortingOrder(int sortOrder)
        {
            teacherCanvas.sortingOrder = sortOrder;
        }

        /// <summary>
        /// Finishes the current running step, skips all the next ones and completes the tutorial.
        /// </summary>
        public void SkipTutorial()
        {
            if (!debugRunTutorial && sendStats)
            {
                SendSkipTutorialSegmentStats(
                    tutorialVersion,
                    runningStep.name.Replace("(Clone)", "").Replace("TutorialStep_", ""));
            }

            int skipIndex = stepsOnGenesisPlaza.Count +
                stepsFromDeepLink.Count +
                stepsOnGenesisPlazaAfterDeepLink.Count;

            StartCoroutine(StartTutorialFromStep(skipIndex));

            hudController?.taskbarHud?.SetVisibility(true);
        }

        /// <summary>
        /// Activate/deactivate the eagle eye camera.
        /// </summary>
        /// <param name="isActive">True for activate the eagle eye camera.</param>
        public void SetEagleEyeCameraActive(bool isActive)
        {
            eagleEyeCamera.gameObject.SetActive(isActive);
            hudController?.minimapHud?.SetVisibility(!isActive);
            hudController?.manaHud?.SetVisibility(!isActive);
            hudController?.profileHud?.SetVisibility(!isActive);

            if (isActive)
            {
                eagleEyeCamera.transform.position = eagleCamInitPosition;
                eagleEyeCamera.transform.LookAt(eagleCamInitLookAtPoint);

                if (eagleCamRotationActived)
                    eagleEyeRotationCoroutine = StartCoroutine(EagleEyeCameraRotation(eagleCamRotationSpeed));
            }
            else if (eagleEyeRotationCoroutine != null)
            {
                StopCoroutine(eagleEyeRotationCoroutine);
            }
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

            PlayTeacherAnimation(TutorialTeacher.TeacherAnimation.Reset);
            executeStepsCoroutine = StartCoroutine(StartTutorialFromStep(currentStepIndex));
        }

        private IEnumerator ExecuteSteps(TutorialPath tutorialPath, int startingStepIndex)
        {
            List<TutorialStep> steps = new List<TutorialStep>();

            switch (tutorialPath)
            {
                case TutorialPath.FromGenesisPlaza:
                    steps = stepsOnGenesisPlaza;
                    break;
                case TutorialPath.FromDeepLink:
                    steps = stepsFromDeepLink;
                    break;
                case TutorialPath.FromGenesisPlazaAfterDeepLink:
                    steps = stepsOnGenesisPlazaAfterDeepLink;
                    break;
            }

            float elapsedTime = 0f;
            for (int i = startingStepIndex; i < steps.Count; i++)
            {
                var stepPrefab = steps[i];

                if (stepPrefab.letInstantiation)
                    runningStep = Instantiate(stepPrefab, this.transform).GetComponent<TutorialStep>();
                else
                    runningStep = steps[i];

                currentStepIndex = i;

                elapsedTime = Time.realtimeSinceStartup;
                runningStep.OnStepStart();
                yield return runningStep.OnStepExecute();
                if (i < steps.Count - 1)
                    PlayTeacherAnimation(TutorialTeacher.TeacherAnimation.StepCompleted);
                else
                    PlayTeacherAnimation(TutorialTeacher.TeacherAnimation.QuickGoodbye);

                yield return runningStep.OnStepPlayAnimationForHidding();
                runningStep.OnStepFinished();
                elapsedTime = Time.realtimeSinceStartup - elapsedTime;
                if (!debugRunTutorial && sendStats)
                {
                    SendStepCompletedSegmentStats(
                        tutorialVersion,
                        tutorialPath,
                        i + 1,
                        runningStep.name.Replace("(Clone)", "").Replace("TutorialStep_", ""),
                        elapsedTime);
                }
                Destroy(runningStep.gameObject);

                if (i < steps.Count - 1 && timeBetweenSteps > 0)
                    yield return new WaitForSeconds(timeBetweenSteps);
            }

            if (!debugRunTutorial && markTutorialAsCompleted)
                SetUserTutorialStepAsCompleted(TutorialFinishStep.NewTutorialFinished);

            runningStep = null;

            SetTutorialDisabled();
        }

        private void SetUserTutorialStepAsCompleted(TutorialFinishStep finishStepType)
        {
            WebInterface.SaveUserTutorialStep(UserProfile.GetOwnUserProfile().tutorialStep | (int)finishStepType);
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

        private void GoToGenesisPlazaHud_OnBeforeGoToGenesisPlaza()
        {
            SetTutorialDisabled();
        }

        private void GoToGenesisPlazaHud_OnAfterGoToGenesisPlaza()
        {
            SetTutorialEnabled(false.ToString());

            if (hudController != null)
                hudController.taskbarHud?.HideGoToGenesisPlazaButton();
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

        private void SendStepCompletedSegmentStats(int version, TutorialPath tutorialPath, int stepNumber, string stepName, float elapsedTime)
        {
            WebInterface.AnalyticsPayload.Property[] properties = new WebInterface.AnalyticsPayload.Property[]
            {
                new WebInterface.AnalyticsPayload.Property("version", version.ToString()),
                new WebInterface.AnalyticsPayload.Property("path", tutorialPath.ToString()),
                new WebInterface.AnalyticsPayload.Property("step number", stepNumber.ToString()),
                new WebInterface.AnalyticsPayload.Property("step name", stepName),
                new WebInterface.AnalyticsPayload.Property("elapsed time", elapsedTime.ToString("0.00"))
            };
            WebInterface.ReportAnalyticsEvent("tutorial step completed", properties);
        }

        private void SendSkipTutorialSegmentStats(int version, string stepName)
        {
            WebInterface.AnalyticsPayload.Property[] properties = new WebInterface.AnalyticsPayload.Property[]
            {
                new WebInterface.AnalyticsPayload.Property("version", version.ToString()),
                new WebInterface.AnalyticsPayload.Property("step name", stepName),
                new WebInterface.AnalyticsPayload.Property("elapsed time", Time.realtimeSinceStartup.ToString("0.00"))
            };
            WebInterface.ReportAnalyticsEvent("tutorial skipped", properties);
        }

        private IEnumerator EagleEyeCameraRotation(float rotationSpeed)
        {
            while (true)
            {
                eagleEyeCamera.transform.Rotate(Vector3.up * Time.deltaTime * rotationSpeed, Space.World);
                yield return null;
            }
        }
    }
}
