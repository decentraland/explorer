using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class DragAndDropSceneObjectView : MonoBehaviour
{
    public event System.Action OnDrop;

    [SerializeField] internal EventTrigger changeModeEventTrigger;

    private void Awake()
    {
        ConfigureEventTrigger(EventTriggerType.Drop, (eventData) => Drop());
    }

    private void ConfigureEventTrigger(EventTriggerType eventType, UnityAction<BaseEventData> call)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = eventType;
        entry.callback.AddListener(call);
        changeModeEventTrigger.triggers.Add(entry);
    }

    public void Drop()
    {
        OnDrop?.Invoke();
    }
}
