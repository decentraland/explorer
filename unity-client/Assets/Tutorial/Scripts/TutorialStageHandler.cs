using UnityEngine;

public abstract class TutorialStageHandler
{
    public TutorialStageHandler nextHandler { private set; get; } = null;

    private TutorialStageController tutorialStageControllerPrefabRef = null;
    private TutorialStageController tutorialStageController = null;

    public event System.Action OnStageFinish;

    public TutorialStageHandler(TutorialStageController stageControllerPrefabRef)
    {
        tutorialStageControllerPrefabRef = stageControllerPrefabRef;
    }

    public TutorialStageHandler SetNext(TutorialStageHandler handler)
    {
        nextHandler = handler;
        return handler;
    }

    public virtual TutorialStageHandler GetHandler(int tutorialFlagMask)
    {
        if (!IsAlreadyFinished(tutorialFlagMask))
        {
            return this;
        }
        else if (nextHandler != null)
        {
            return nextHandler.GetHandler(tutorialFlagMask);
        }
        return null;
    }

    public abstract TutorialController.TutorialFlags flag { get; }
    public virtual void SetUpStage() { }

    public virtual bool IsAlreadyFinished(int tutorialFlagMask)
    {
        return (tutorialFlagMask & (int)flag) != 0;
    }

    public void Start()
    {
        if (tutorialStageControllerPrefabRef == null)
        {
            return;
        }
        tutorialStageController = Object.Instantiate(tutorialStageControllerPrefabRef);
        tutorialStageController?.OnStageStart();
    }

    public void Finish()
    {
        if (tutorialStageController)
        {
            tutorialStageController.OnStageFinished();
            Object.Destroy(tutorialStageController.gameObject);
        }
        OnStageFinish?.Invoke();
        OnStageFinish = null;
    }
}
