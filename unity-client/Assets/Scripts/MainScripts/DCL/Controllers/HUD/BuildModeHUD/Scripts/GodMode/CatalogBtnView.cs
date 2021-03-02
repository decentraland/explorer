using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public interface ICatalogBtnView
{
    event Action OnCatalogButtonClick;
    event Action OnHideTooltip;
    event Action<BaseEventData, string> OnShowTooltip;

    void ConfigureEventTrigger(EventTriggerType eventType, UnityAction<BaseEventData> call);
    void OnPointerClick();
    void OnPointerEnter(PointerEventData eventData);
    void OnPointerExit();
    void RemoveEventTrigger(EventTriggerType eventType);
}

public class CatalogBtnView : MonoBehaviour, ICatalogBtnView
{
    public event Action OnCatalogButtonClick;
    public event Action<BaseEventData, string> OnShowTooltip;
    public event Action OnHideTooltip;

    [SerializeField] internal Button mainButton;
    [SerializeField] internal string tooltipText = "Open Catalog (C)";
    [SerializeField] internal EventTrigger catalogButtonEventTrigger;
    [SerializeField] internal InputAction_Trigger toggleCatalogInputAction;

    private void Awake()
    {
        mainButton.onClick.AddListener(OnPointerClick);
        toggleCatalogInputAction.OnTriggered += (action) => OnPointerClick();
        ConfigureEventTrigger(EventTriggerType.PointerEnter, (eventData) => OnPointerEnter((PointerEventData)eventData));
        ConfigureEventTrigger(EventTriggerType.PointerExit, (eventData) => OnPointerExit());
    }

    private void OnDestroy()
    {
        mainButton.onClick.RemoveListener(OnPointerClick);
        toggleCatalogInputAction.OnTriggered -= (action) => OnPointerClick();
        RemoveEventTrigger(EventTriggerType.PointerEnter);
        RemoveEventTrigger(EventTriggerType.PointerExit);
    }

    public void ConfigureEventTrigger(EventTriggerType eventType, UnityAction<BaseEventData> call)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = eventType;
        entry.callback.AddListener(call);
        catalogButtonEventTrigger.triggers.Add(entry);
    }

    public void RemoveEventTrigger(EventTriggerType eventType)
    {
        catalogButtonEventTrigger.triggers.RemoveAll(x => x.eventID == eventType);
    }

    public void OnPointerClick()
    {
        OnCatalogButtonClick?.Invoke();
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