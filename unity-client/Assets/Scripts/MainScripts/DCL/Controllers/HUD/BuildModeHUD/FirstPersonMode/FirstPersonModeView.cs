using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FirstPersonModeView : MonoBehaviour
{
    public event System.Action OnFirstPersonModeClick;
    public event System.Action<BaseEventData, string> OnShowTooltip;
    public event System.Action OnHideTooltip;

    [SerializeField] internal Button mainButton;
    [SerializeField] internal string tooltipText = "Change Camera (I)";
    [SerializeField] internal EventTrigger changeModeEventTrigger;

    private void Awake()
    {
        mainButton.onClick.AddListener(OnPointerClick);
        ConfigureEventTrigger(EventTriggerType.PointerEnter, (eventData) => OnPointerEnter((PointerEventData)eventData));
        ConfigureEventTrigger(EventTriggerType.PointerExit, (eventData) => OnPointerExit());
    }

    private void ConfigureEventTrigger(EventTriggerType eventType, UnityAction<BaseEventData> call)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = eventType;
        entry.callback.AddListener(call);
        changeModeEventTrigger.triggers.Add(entry);
    }

    public void OnPointerClick()
    {
        OnFirstPersonModeClick?.Invoke();
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
