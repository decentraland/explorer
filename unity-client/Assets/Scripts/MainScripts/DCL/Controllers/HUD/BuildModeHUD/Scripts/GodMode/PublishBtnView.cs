using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PublishBtnView : MonoBehaviour
{
    public event System.Action OnPublishButtonClick;
    public event System.Action<BaseEventData, string> OnShowTooltip;
    public event System.Action OnHideTooltip;

    [SerializeField] internal Button mainButton;
    [SerializeField] internal string tooltipText = "Publish Scene";
    [SerializeField] internal EventTrigger publishButtonEventTrigger;

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
        publishButtonEventTrigger.triggers.Add(entry);
    }

    public void OnPointerClick()
    {
        OnPublishButtonClick?.Invoke();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnShowTooltip?.Invoke(eventData, tooltipText);
    }

    public void OnPointerExit()
    {
        OnHideTooltip?.Invoke();
    }

    public void SetInteractable(bool isInteractable)
    {
        mainButton.interactable = isInteractable;
    }
}
