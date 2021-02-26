using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InspectorBtnView : MonoBehaviour
{
    public event System.Action OnInspectorButtonClick;
    public event System.Action<BaseEventData, string> OnShowTooltip;
    public event System.Action OnHideTooltip;

    [SerializeField] internal Button mainButton;
    [SerializeField] internal string tooltipText = "Open Entity List (Q)";
    [SerializeField] internal EventTrigger inspectorButtonEventTrigger;
    [SerializeField] internal InputAction_Trigger toggleOpenEntityListInputAction;

    private void Awake()
    {
        mainButton.onClick.AddListener(OnPointerClick);
        toggleOpenEntityListInputAction.OnTriggered += (action) => OnPointerClick();

        ConfigureEventTrigger(EventTriggerType.PointerEnter, (eventData) => OnPointerEnter((PointerEventData)eventData));
        ConfigureEventTrigger(EventTriggerType.PointerExit, (eventData) => OnPointerExit());
    }

    private void ConfigureEventTrigger(EventTriggerType eventType, UnityAction<BaseEventData> call)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = eventType;
        entry.callback.AddListener(call);
        inspectorButtonEventTrigger.triggers.Add(entry);
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
