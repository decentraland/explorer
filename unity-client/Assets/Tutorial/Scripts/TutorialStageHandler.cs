
//public abstract class TutorialStageHandler
//{

//    public TutorialStageHandler nextHandler { private set; get; } = null;

//    private TutorialStep tutorialStageControllerPrefabRef = null;
//    private TutorialStep tutorialStageController = null;

//    public event System.Action OnStageFinish;

//    public TutorialStageHandler(TutorialStep stageControllerPrefabRef)
//    {
//        tutorialStageControllerPrefabRef = stageControllerPrefabRef;
//    }

//    public TutorialStageHandler SetNext(TutorialStageHandler handler)
//    {
//        nextHandler = handler;
//        return handler;
//    }

//    public virtual TutorialStageHandler GetHandler(int tutorialFlagMask)
//    {
//        if (!IsAlreadyFinished(tutorialFlagMask))
//        {
//            return this;
//        }
//        else if (nextHandler != null)
//        {
//            return nextHandler.GetHandler(tutorialFlagMask);
//        }

//        return null;
//    }

//    public virtual void SetUpStage() { }

//    public virtual bool IsAlreadyFinished(int tutorialFlagMask)
//    {
//        return false;
//        // return (tutorialFlagMask & (int)flag) != 0;
//    }

//    public void Start()
//    {
//        if (tutorialStageControllerPrefabRef == null)
//        {
//            return;
//        }
//        tutorialStageController = Object.Instantiate(tutorialStageControllerPrefabRef);
//        tutorialStageController?.OnStepStart();
//    }

//    public void Finish()
//    {
//        if (tutorialStageController)
//        {
//            tutorialStageController.OnStepFinished();
//            Object.Destroy(tutorialStageController.gameObject);
//        }

//        OnStageFinish?.Invoke();
//        OnStageFinish = null;
//    }
//}
