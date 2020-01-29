using System;
using UnityEngine;

public class TutorialController : MonoBehaviour
{
    public const float TOOLTIP_AUTO_HIDE_SECONDS = 2f;
    public const float DEFAULT_STAGE_IDLE_TIME = 5f;

#if UNITY_EDITOR
    [Header("Debugging")]
    [SerializeField] int debugFlagStartingValue = 0;
    [SerializeField] bool debugRunTutorialOnStart = false;
    [Space()]
#endif

    [Header("Stage Controller References")]
    [SerializeField] TutorialStageController initalStage = null;
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

    private TutorialStageHandler initialStage;
    private int tutorialFlagMask = 0;
    private bool initialized = false;

    public void SetTutorialEnabled()
    {
        isTutorialEnabled = true;
        if (RenderingController.i)
        {
            RenderingController.i.OnRenderingStateChanged += OnRenderingStateChanged;
        }
        tutorialFlagMask = 0; // TODO: get flag from user
        CreateStagesChainOfResponsibility();
    }

    private void Awake()
    {
        i = this;
    }

#if UNITY_EDITOR
    private void Start()
    {
        if (debugFlagStartingValue != 0)
        {
            tutorialFlagMask = debugFlagStartingValue;
        }

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
        runningStage.OnStageFinished();
        // TODO: update user profile
        StartTutorialStageIfNeeded(tutorialFlagMask);
    }

    private void StartTutorialStageIfNeeded(int tutorialFlagMask)
    {
        if (runningStage != null)
        {
            return;
        }
        runningStage = initialStage.GetHandler(tutorialFlagMask);
        if (runningStage != null)
        {
            runningStage.OnStageFinish += () => runningStage = null;
            runningStage.OnStageStart();
        }
    }

    private void OnRenderingStateChanged(bool renderingEnabled)
    {
        if (renderingEnabled && isTutorialEnabled)
        {
            if (!initialized)
            {
                Initialize(tutorialFlagMask);
            }
            StartTutorialStageIfNeeded(tutorialFlagMask);
        }
    }

    private void Initialize(int tutorialFlagMask)
    {
        initialized = true;
        TutorialStageHandler handler = initialStage;
        while (handler != null)
        {
            if (!handler.IsAlreadyFinished(tutorialFlagMask))
            {
                handler.SetUpStage();
            }
            handler = handler.nextHandler;
        }
    }

    private void CreateStagesChainOfResponsibility()
    {
        initialStage = new GenericSceneStageHandler(TutorialFlags.InitialScene, initalStage,
            () =>
            {
                HUDController.i?.minimapHud.SetVisibility(false);
            });

        var nextStage = initialStage.SetNext(new GenericSceneStageHandler(TutorialFlags.LoginRewardAndUserEmail, genesisPlazaStage, null));
        nextStage = nextStage.SetNext(new GenericSceneStageHandler(TutorialFlags.ChatAndAvatarExpressions, chatAndExpressionsStage,
            () =>
            {
                // TODO: hide chat and avatar expressions
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
