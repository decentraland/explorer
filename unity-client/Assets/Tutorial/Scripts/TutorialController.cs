using System;
using UnityEngine;

public class TutorialController : MonoBehaviour
{
    public const float DEFAULT_STAGE_IDLE_TIME = 20f;

#if UNITY_EDITOR
    [Header("Debugging")]
    public int debugFlagStartingValue = 0;
    public bool debugRunTutorialOnStart = false;
    [Space()]
#endif

    [Header("Stage Controller References")]
    [SerializeField] TutorialStageController initialStage = null;
    [SerializeField] TutorialStageController genesisPlazaStage = null;
    [SerializeField] TutorialStageController chatAndExpressionsStage = null;

    public static TutorialController i { private set; get; }

    public bool isTutorialEnabled { private set; get; }

    [Flags]
    public enum TutorialFlags
    {
        None = 0,
        InitialScene = 1,
        LoginRewardAndUserEmail = 2,
        ChatAndAvatarExpressions = 4
    }

    private TutorialStageHandler runningStage = null;

    private TutorialStageHandler firstStage;
    private int tutorialFlagMask = 0;
    private bool initialized = false;
    private Canvas chatUIScreen = null;

    public void SetTutorialEnabled()
    {
        isTutorialEnabled = true;
        if (RenderingController.i)
        {
            RenderingController.i.OnRenderingStateChanged += OnRenderingStateChanged;
        }
        CreateStagesChainOfResponsibility();
    }

    private void Awake()
    {
        i = this;
    }

#if UNITY_EDITOR
    private void Start()
    {
        if (debugRunTutorialOnStart)
        {
            CreateStagesChainOfResponsibility();
            isTutorialEnabled = true;
            if (!RenderingController.i)
            {
                OnRenderingStateChanged(true);
            }
            else
            {
                RenderingController.i.OnRenderingStateChanged += OnRenderingStateChanged;
            }
        }
    }
#endif

    private void OnDestroy()
    {
        if (RenderingController.i)
        {
            RenderingController.i.OnRenderingStateChanged -= OnRenderingStateChanged;
        }
        i = null;
    }

    public void SetRunningStageFinished()
    {
        tutorialFlagMask |= (int)runningStage.flag;
        runningStage.Finish();

        // TODO: send update to user profile
        UserProfile.GetOwnUserProfile().SetTutorialFlag(tutorialFlagMask);

        StartTutorialStageIfNeeded(tutorialFlagMask);
    }

    private void StartTutorialStageIfNeeded(int tutorialFlagMask)
    {
        if (runningStage != null)
        {
            return;
        }
        runningStage = firstStage.GetHandler(tutorialFlagMask);
        if (runningStage != null)
        {
            runningStage.OnStageFinish += () => runningStage = null;
            runningStage.Start();
        }
    }

    private int GetTutorialFlagFromProfile()
    {
        return UserProfile.GetOwnUserProfile().tutorialFlag;
    }

    private void OnRenderingStateChanged(bool renderingEnabled)
    {
        if (!isTutorialEnabled || !renderingEnabled) return;

        tutorialFlagMask = GetTutorialFlagFromProfile(); // TODO: get flag from user profile

#if UNITY_EDITOR
        if (debugFlagStartingValue != 0)
        {
            tutorialFlagMask = debugFlagStartingValue;
        }
#endif
        if (!initialized)
        {
            Initialize(tutorialFlagMask);
        }
        StartTutorialStageIfNeeded(tutorialFlagMask);
    }

    private void Initialize(int tutorialFlagMask)
    {
        initialized = true;
        CacheChatScreen();
        TutorialStageHandler handler = firstStage;
        while (handler != null)
        {
            if (!handler.IsAlreadyFinished(tutorialFlagMask))
            {
                handler.SetUpStage();
            }
            handler = handler.nextHandler;
        }
    }

    private void CacheChatScreen()
    {
        if (chatUIScreen == null && DCL.SceneController.i)
        {
            using (var iterator = DCL.SceneController.i.loadedScenes.GetEnumerator())
            {
                while (iterator.MoveNext())
                {
                    if (iterator.Current.Value.isPersistent && iterator.Current.Value.uiScreenSpace != null)
                    {
                        chatUIScreen = iterator.Current.Value.uiScreenSpace.canvas;
                        break;
                    }
                }
            }
        }
    }

    public void SetChatVisible(bool visible)
    {
        if (chatUIScreen != null)
        {
            chatUIScreen.enabled = visible;
        }
    }

    private void CreateStagesChainOfResponsibility()
    {
        // 1st stage - initialStage
        firstStage = new GenericSceneStageHandler(TutorialFlags.InitialScene, initialStage,
            () =>
            {
                HUDController.i?.minimapHud.SetVisibility(false);
            });

        // 2nd stage - LoginRewardAndUserEmail
        var nextStage = firstStage.SetNext(new GenericSceneStageHandler(TutorialFlags.LoginRewardAndUserEmail, genesisPlazaStage, null));

        // 3rd stage - ChatAndAvatarExpressions
        nextStage = nextStage.SetNext(new GenericSceneStageHandler(TutorialFlags.ChatAndAvatarExpressions, chatAndExpressionsStage,
            () =>
            {
                HUDController.i?.avatarHud.SetVisibility(false);
                SetChatVisible(false);
                // TODO: hide avatar expressions
                //HUDController.i?.expressionsHud.SetVisibility(false);
            }));
    }

    class GenericSceneStageHandler : TutorialStageHandler
    {
        private TutorialFlags tutorialStageFlag;
        private Action setupCallback;

        public GenericSceneStageHandler(TutorialFlags stageFlag, TutorialStageController stage, Action setupStageCallback) : base(stage)
        {
            tutorialStageFlag = stageFlag;
            setupCallback = setupStageCallback;
        }

        public override TutorialFlags flag => tutorialStageFlag;

        public override void SetUpStage()
        {
            setupCallback?.Invoke();
        }
    }
}
