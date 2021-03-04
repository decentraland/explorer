using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public interface ITopActionsButtonsView
{
    event Action OnChangeModeClicked,
                 OnExtraClicked,
                 OnTranslateClicked,
                 OnRotateClicked,
                 OnScaleClicked,
                 OnResetClicked,
                 OnDuplicateClicked,
                 OnDeleteClicked,
                 OnLogOutClicked,
                 OnPointerExit;

    event Action<BaseEventData, string> OnChangeCameraModePointerEnter,
                                        OnTranslatePointerEnter,
                                        OnRotatePointerEnter,
                                        OnScalePointerEnter,
                                        OnResetPointerEnter,
                                        OnDuplicatePointerEnter,
                                        OnDeletePointerEnter,
                                        OnMoreActionsPointerEnter,
                                        OnLogoutPointerEnter;

    void ConfigureEventTrigger(EventTrigger eventTrigger, EventTriggerType eventType, UnityAction<BaseEventData> call);
    void ConfigureExtraActions(IExtraActionsController extraActionsController);
    void OnChangeModeClick();
    void OnDeleteClick();
    void OnDuplicateClick();
    void OnExtraClick();
    void OnLogOutClick();
    void OnResetClick();
    void OnRotateClick();
    void OnScaleClick();
    void OnTranslateClick();
    void RemoveEventTrigger(EventTrigger eventTrigger, EventTriggerType eventType);
}

public class TopActionsButtonsView : MonoBehaviour, ITopActionsButtonsView
{
    public event Action OnChangeModeClicked,
                        OnExtraClicked,
                        OnTranslateClicked,
                        OnRotateClicked,
                        OnScaleClicked,
                        OnResetClicked,
                        OnDuplicateClicked,
                        OnDeleteClicked,
                        OnLogOutClicked,
                        OnPointerExit;

    public event Action<BaseEventData, string> OnChangeCameraModePointerEnter,
                                               OnTranslatePointerEnter,
                                               OnRotatePointerEnter,
                                               OnScalePointerEnter,
                                               OnResetPointerEnter,
                                               OnDuplicatePointerEnter,
                                               OnDeletePointerEnter,
                                               OnMoreActionsPointerEnter,
                                               OnLogoutPointerEnter;

    [Header("Buttons")]
    [SerializeField] internal Button changeModeBtn;
    [SerializeField] internal Button extraBtn;
    [SerializeField] internal Button translateBtn;
    [SerializeField] internal Button rotateBtn;
    [SerializeField] internal Button scaleBtn;
    [SerializeField] internal Button resetBtn;
    [SerializeField] internal Button duplicateBtn;
    [SerializeField] internal Button deleteBtn;
    [SerializeField] internal Button logOutBtn;

    [Header("Input Actions")]
    [SerializeField] internal InputAction_Trigger toggleChangeCameraInputAction;
    [SerializeField] internal InputAction_Trigger toggleTranslateInputAction;
    [SerializeField] internal InputAction_Trigger toggleRotateInputAction;
    [SerializeField] internal InputAction_Trigger toggleScaleInputAction;
    [SerializeField] internal InputAction_Trigger toggleResetInputAction;
    [SerializeField] internal InputAction_Trigger toggleDuplicateInputAction;
    [SerializeField] internal InputAction_Trigger toggleDeleteInputAction;

    [Header("Event Triggers")]
    [SerializeField] internal EventTrigger changeCameraModeEventTrigger;
    [SerializeField] internal EventTrigger translateEventTrigger;
    [SerializeField] internal EventTrigger rotateEventTrigger;
    [SerializeField] internal EventTrigger scaleEventTrigger;
    [SerializeField] internal EventTrigger resetEventTrigger;
    [SerializeField] internal EventTrigger duplicateEventTrigger;
    [SerializeField] internal EventTrigger deleteEventTrigger;
    [SerializeField] internal EventTrigger moreActionsEventTrigger;
    [SerializeField] internal EventTrigger logoutEventTrigger;

    [Header("Tooltip Texts")]
    [SerializeField] internal string changeCameraModeTooltipText = "Change Camera (V)";
    [SerializeField] internal string translateTooltipText = "Translate (M)";
    [SerializeField] internal string rotateTooltipText = "Rotate (R)";
    [SerializeField] internal string scaleTooltipText = "Scale (G)";
    [SerializeField] internal string resetTooltipText = "Reset (Control+R)";
    [SerializeField] internal string duplicateTooltipText = "Duplicate (Control+D)";
    [SerializeField] internal string deleteTooltipText = "Delete (Del) or (Backspace)";
    [SerializeField] internal string moreActionsTooltipText = "Extra Actions";
    [SerializeField] internal string logoutTooltipText = "Exit from edition";

    [Header("Sub-Views")]
    [SerializeField] internal ExtraActionsView extraActionsView;

    internal IExtraActionsController extraActionsController;

    private const string VIEW_PATH = "GodMode/TopActionsButtons/TopActionsButtonsView";

    internal static TopActionsButtonsView Create()
    {
        var view = Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<TopActionsButtonsView>();
        view.gameObject.name = "_TopActionsButtonsView";

        return view;
    }

    private void Awake()
    {
        changeModeBtn.onClick.AddListener(OnChangeModeClick);
        translateBtn.onClick.AddListener(OnTranslateClick);
        rotateBtn.onClick.AddListener(OnRotateClick);
        scaleBtn.onClick.AddListener(OnScaleClick);
        resetBtn.onClick.AddListener(OnResetClick);
        duplicateBtn.onClick.AddListener(OnDuplicateClick);
        deleteBtn.onClick.AddListener(OnDeleteClick);
        logOutBtn.onClick.AddListener(OnLogOutClick);
        extraBtn.onClick.AddListener(OnExtraClick);

        ConfigureEventTrigger(
            changeCameraModeEventTrigger,
            EventTriggerType.PointerEnter,
            (eventData) => OnChangeCameraModePointerEnter?.Invoke(eventData, changeCameraModeTooltipText));

        ConfigureEventTrigger(
            changeCameraModeEventTrigger,
            EventTriggerType.PointerExit,
            (eventData) => OnPointerExit?.Invoke());

        ConfigureEventTrigger(
            translateEventTrigger,
            EventTriggerType.PointerEnter,
            (eventData) => OnTranslatePointerEnter?.Invoke(eventData, translateTooltipText));

        ConfigureEventTrigger(
            translateEventTrigger,
            EventTriggerType.PointerExit,
            (eventData) => OnPointerExit?.Invoke());

        ConfigureEventTrigger(
            rotateEventTrigger,
            EventTriggerType.PointerEnter,
            (eventData) => OnRotatePointerEnter?.Invoke(eventData, rotateTooltipText));

        ConfigureEventTrigger(
            rotateEventTrigger,
            EventTriggerType.PointerExit,
            (eventData) => OnPointerExit?.Invoke());

        ConfigureEventTrigger(
            scaleEventTrigger,
            EventTriggerType.PointerEnter,
            (eventData) => OnScalePointerEnter?.Invoke(eventData, scaleTooltipText));

        ConfigureEventTrigger(
            scaleEventTrigger,
            EventTriggerType.PointerExit,
            (eventData) => OnPointerExit?.Invoke());

        ConfigureEventTrigger(
            resetEventTrigger,
            EventTriggerType.PointerEnter,
            (eventData) => OnResetPointerEnter?.Invoke(eventData, resetTooltipText));

        ConfigureEventTrigger(
            resetEventTrigger,
            EventTriggerType.PointerExit,
            (eventData) => OnPointerExit?.Invoke());

        ConfigureEventTrigger(
            duplicateEventTrigger,
            EventTriggerType.PointerEnter,
            (eventData) => OnDuplicatePointerEnter?.Invoke(eventData, duplicateTooltipText));

        ConfigureEventTrigger(
            duplicateEventTrigger,
            EventTriggerType.PointerExit,
            (eventData) => OnPointerExit?.Invoke());

        ConfigureEventTrigger(
            deleteEventTrigger,
            EventTriggerType.PointerEnter,
            (eventData) => OnDeletePointerEnter?.Invoke(eventData, deleteTooltipText));

        ConfigureEventTrigger(
            deleteEventTrigger,
            EventTriggerType.PointerExit,
            (eventData) => OnPointerExit?.Invoke());

        ConfigureEventTrigger(
            moreActionsEventTrigger,
            EventTriggerType.PointerEnter,
            (eventData) => OnMoreActionsPointerEnter?.Invoke(eventData, moreActionsTooltipText));

        ConfigureEventTrigger(
            moreActionsEventTrigger,
            EventTriggerType.PointerExit,
            (eventData) => OnPointerExit?.Invoke());

        ConfigureEventTrigger(
            logoutEventTrigger,
            EventTriggerType.PointerEnter,
            (eventData) => OnLogoutPointerEnter?.Invoke(eventData, logoutTooltipText));

        ConfigureEventTrigger(
            logoutEventTrigger,
            EventTriggerType.PointerExit,
            (eventData) => OnPointerExit?.Invoke());

        toggleChangeCameraInputAction.OnTriggered += (action) => OnChangeModeClick();
        toggleTranslateInputAction.OnTriggered += (action) => OnTranslateClick();
        toggleRotateInputAction.OnTriggered += (action) => OnRotateClick();
        toggleScaleInputAction.OnTriggered += (action) => OnScaleClick();
        toggleResetInputAction.OnTriggered += (action) => OnResetClick();
        toggleDuplicateInputAction.OnTriggered += (action) => OnDuplicateClick();
        toggleDeleteInputAction.OnTriggered += (action) => OnDeleteClick();
    }

    private void OnDestroy()
    {
        changeModeBtn.onClick.RemoveListener(OnChangeModeClick);
        translateBtn.onClick.RemoveListener(OnTranslateClick);
        rotateBtn.onClick.RemoveListener(OnRotateClick);
        scaleBtn.onClick.RemoveListener(OnScaleClick);
        resetBtn.onClick.RemoveListener(OnResetClick);
        duplicateBtn.onClick.RemoveListener(OnDuplicateClick);
        deleteBtn.onClick.RemoveListener(OnDeleteClick);
        logOutBtn.onClick.RemoveListener(OnLogOutClick);
        extraBtn.onClick.RemoveListener(OnExtraClick);

        RemoveEventTrigger(changeCameraModeEventTrigger, EventTriggerType.PointerEnter);
        RemoveEventTrigger(changeCameraModeEventTrigger, EventTriggerType.PointerExit);
        RemoveEventTrigger(translateEventTrigger, EventTriggerType.PointerEnter);
        RemoveEventTrigger(translateEventTrigger, EventTriggerType.PointerExit);
        RemoveEventTrigger(rotateEventTrigger, EventTriggerType.PointerEnter);
        RemoveEventTrigger(rotateEventTrigger, EventTriggerType.PointerExit);
        RemoveEventTrigger(scaleEventTrigger, EventTriggerType.PointerEnter);
        RemoveEventTrigger(scaleEventTrigger, EventTriggerType.PointerExit);
        RemoveEventTrigger(resetEventTrigger, EventTriggerType.PointerEnter);
        RemoveEventTrigger(resetEventTrigger, EventTriggerType.PointerExit);
        RemoveEventTrigger(duplicateEventTrigger, EventTriggerType.PointerEnter);
        RemoveEventTrigger(duplicateEventTrigger, EventTriggerType.PointerExit);
        RemoveEventTrigger(deleteEventTrigger, EventTriggerType.PointerEnter);
        RemoveEventTrigger(deleteEventTrigger, EventTriggerType.PointerExit);
        RemoveEventTrigger(moreActionsEventTrigger, EventTriggerType.PointerEnter);
        RemoveEventTrigger(moreActionsEventTrigger, EventTriggerType.PointerExit);
        RemoveEventTrigger(logoutEventTrigger, EventTriggerType.PointerEnter);
        RemoveEventTrigger(logoutEventTrigger, EventTriggerType.PointerExit);

        toggleChangeCameraInputAction.OnTriggered -= (action) => OnChangeModeClick();
        toggleTranslateInputAction.OnTriggered -= (action) => OnTranslateClick();
        toggleRotateInputAction.OnTriggered -= (action) => OnRotateClick();
        toggleScaleInputAction.OnTriggered -= (action) => OnScaleClick();
        toggleResetInputAction.OnTriggered -= (action) => OnResetClick();
        toggleDuplicateInputAction.OnTriggered -= (action) => OnDuplicateClick();
        toggleDeleteInputAction.OnTriggered -= (action) => OnDeleteClick();

        if (extraActionsController != null)
            extraActionsController.Dispose();
    }

    public void ConfigureEventTrigger(EventTrigger eventTrigger, EventTriggerType eventType, UnityAction<BaseEventData> call)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = eventType;
        entry.callback.AddListener(call);
        eventTrigger.triggers.Add(entry);
    }

    public void RemoveEventTrigger(EventTrigger eventTrigger, EventTriggerType eventType)
    {
        eventTrigger.triggers.RemoveAll(x => x.eventID == eventType);
    }

    public void ConfigureExtraActions(IExtraActionsController extraActionsController)
    {
        this.extraActionsController = extraActionsController;
        this.extraActionsController.Initialize(extraActionsView);
    }

    public void OnChangeModeClick()
    {
        OnChangeModeClicked?.Invoke();
    }

    public void OnExtraClick()
    {
        OnExtraClicked?.Invoke();
    }

    public void OnTranslateClick()
    {
        OnTranslateClicked?.Invoke();
    }

    public void OnRotateClick()
    {
        OnRotateClicked?.Invoke();
    }

    public void OnScaleClick()
    {
        OnScaleClicked?.Invoke();
    }

    public void OnResetClick()
    {
        OnResetClicked?.Invoke();
    }

    public void OnDuplicateClick()
    {
        OnDuplicateClicked?.Invoke();
    }

    public void OnDeleteClick()
    {
        OnDeleteClicked?.Invoke();
    }

    public void OnLogOutClick()
    {
        OnLogOutClicked?.Invoke();
    }
}
