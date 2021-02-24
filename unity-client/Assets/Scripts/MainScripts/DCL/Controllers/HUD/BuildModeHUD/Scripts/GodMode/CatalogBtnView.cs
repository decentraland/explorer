using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CatalogBtnView : MonoBehaviour
{
    public event System.Action OnCatalogButtonClick;
    public event System.Action<BaseEventData, string> OnShowTooltip;
    public event System.Action OnHideTooltip;

    [SerializeField] internal Button mainButton;
    [SerializeField] internal string tooltipText = "Open Catalog (J)";
    [SerializeField] internal EventTrigger catalogButtonEventTrigger;

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
        catalogButtonEventTrigger.triggers.Add(entry);
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