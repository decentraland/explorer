using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public interface IFirstPersonModeView
{
    event Action OnFirstPersonModeClick;
    event Action OnHideTooltip;
    event Action<BaseEventData, string> OnShowTooltip;

    void ConfigureEventTrigger(EventTriggerType eventType, UnityAction<BaseEventData> call);
    void OnPointerClick();
    void OnPointerEnter(PointerEventData eventData);
    void OnPointerExit();
    void RemoveEventTrigger(EventTriggerType eventType);
}

public class FirstPersonModeView : MonoBehaviour, IFirstPersonModeView
{
    public event Action OnFirstPersonModeClick;
    public event Action<BaseEventData, string> OnShowTooltip;
    public event Action OnHideTooltip;

    [SerializeField] internal Button mainButton;
    [SerializeField] internal string tooltipText = "Change Camera (I)";
    [SerializeField] internal EventTrigger changeModeEventTrigger;

    private void Awake()
    {
        mainButton.onClick.AddListener(OnPointerClick);
        ConfigureEventTrigger(EventTriggerType.PointerEnter, (eventData) => OnPointerEnter((PointerEventData)eventData));
        ConfigureEventTrigger(EventTriggerType.PointerExit, (eventData) => OnPointerExit());
    }

    private void OnDestroy()
    {
        mainButton.onClick.RemoveListener(OnPointerClick);
        RemoveEventTrigger(EventTriggerType.PointerEnter);
        RemoveEventTrigger(EventTriggerType.PointerExit);
    }

    public void ConfigureEventTrigger(EventTriggerType eventType, UnityAction<BaseEventData> call)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = eventType;
        entry.callback.AddListener(call);
        changeModeEventTrigger.triggers.Add(entry);
    }

    public void RemoveEventTrigger(EventTriggerType eventType)
    {
        changeModeEventTrigger.triggers.RemoveAll(x => x.eventID == eventType);
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
