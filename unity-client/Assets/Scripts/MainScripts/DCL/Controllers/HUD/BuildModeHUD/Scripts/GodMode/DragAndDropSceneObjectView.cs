using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public interface IDragAndDropSceneObjectView
{
    event Action OnDrop;

    void ConfigureEventTrigger(EventTriggerType eventType, UnityAction<BaseEventData> call);
    void Drop();
    void RemoveEventTrigger(EventTriggerType eventType);
}

public class DragAndDropSceneObjectView : MonoBehaviour, IDragAndDropSceneObjectView
{
    public event Action OnDrop;

    [SerializeField] internal EventTrigger dragAndDropEventTrigger;

    private const string VIEW_PATH = "GodMode/DragAndDropSceneObjectView";

    internal static DragAndDropSceneObjectView Create()
    {
        var view = Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<DragAndDropSceneObjectView>();
        view.gameObject.name = "_DragAndDropSceneObjectView";

        return view;
    }

    private void Awake()
    {
        ConfigureEventTrigger(EventTriggerType.Drop, (eventData) => Drop());
    }
    private void OnDestroy()
    {
        RemoveEventTrigger(EventTriggerType.Drop);
    }

    public void ConfigureEventTrigger(EventTriggerType eventType, UnityAction<BaseEventData> call)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = eventType;
        entry.callback.AddListener(call);
        dragAndDropEventTrigger.triggers.Add(entry);
    }

    public void RemoveEventTrigger(EventTriggerType eventType)
    {
        dragAndDropEventTrigger.triggers.RemoveAll(x => x.eventID == eventType);
    }

    public void Drop()
    {
        OnDrop?.Invoke();
    }
}
