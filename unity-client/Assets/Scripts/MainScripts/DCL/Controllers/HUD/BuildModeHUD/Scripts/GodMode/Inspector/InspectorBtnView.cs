using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public interface IInspectorBtnView
{
    event Action OnHideTooltip;
    event Action OnInspectorButtonClick;
    event Action<BaseEventData, string> OnShowTooltip;

    void ConfigureEventTrigger(EventTriggerType eventType, UnityAction<BaseEventData> call);
    void OnPointerClick();
    void OnPointerEnter(PointerEventData eventData);
    void OnPointerExit();
    void RemoveEventTrigger(EventTriggerType eventType);
}

public class InspectorBtnView : MonoBehaviour, IInspectorBtnView
{
    public event Action OnInspectorButtonClick;
    public event Action<BaseEventData, string> OnShowTooltip;
    public event Action OnHideTooltip;

    [SerializeField] internal Button mainButton;
    [SerializeField] internal string tooltipText = "Open Entity List (Q)";
    [SerializeField] internal EventTrigger inspectorButtonEventTrigger;
    [SerializeField] internal InputAction_Trigger toggleOpenEntityListInputAction;

    private const string VIEW_PATH = "GodMode/Inspector/InspectorBtnView";

    internal static InspectorBtnView Create()
    {
        var view = Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<InspectorBtnView>();
        view.gameObject.name = "_InspectorBtnView";

        return view;
    }

    private void Awake()
    {
        mainButton.onClick.AddListener(OnPointerClick);
        toggleOpenEntityListInputAction.OnTriggered += (action) => OnPointerClick();
        ConfigureEventTrigger(EventTriggerType.PointerEnter, (eventData) => OnPointerEnter((PointerEventData)eventData));
        ConfigureEventTrigger(EventTriggerType.PointerExit, (eventData) => OnPointerExit());
    }

    private void OnDestroy()
    {
        mainButton.onClick.RemoveListener(OnPointerClick);
        toggleOpenEntityListInputAction.OnTriggered -= (action) => OnPointerClick();
        RemoveEventTrigger(EventTriggerType.PointerEnter);
        RemoveEventTrigger(EventTriggerType.PointerExit);
    }

    public void ConfigureEventTrigger(EventTriggerType eventType, UnityAction<BaseEventData> call)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = eventType;
        entry.callback.AddListener(call);
        inspectorButtonEventTrigger.triggers.Add(entry);
    }

    public void RemoveEventTrigger(EventTriggerType eventType)
    {
        inspectorButtonEventTrigger.triggers.RemoveAll(x => x.eventID == eventType);
    }

    public void OnPointerClick()
    {
        OnInspectorButtonClick?.Invoke();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnShowTooltip?.Invoke(eventData, tooltipText);
    }

    public void OnPointerExit()
    {
        OnHideTooltip?.Invoke();
    }
}
