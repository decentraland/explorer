using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public interface IQuickBarView
{
    event Action<int> OnQuickBarInputTriggered;
    event Action<int> OnQuickBarObjectSelected;
    event Action<BaseEventData> OnSceneObjectDropped;
    event Action<int> OnSetIndexToDrop;

    void ConfigureEventTrigger(int index, EventTriggerType eventType, UnityAction<BaseEventData> call);
    void OnQuickBarInputTriggedered(int index);
    void QuickBarObjectSelected(int index);
    void RemoveEventTrigger(int index, EventTriggerType eventType);
    void SceneObjectDropped(BaseEventData data);
    void SetIndexToDrop(int index);
    void SetTextureToShortcut(int shortcutIndex, Texture texture);
}

public class QuickBarView : MonoBehaviour, IQuickBarView
{
    public event Action<int> OnQuickBarObjectSelected;
    public event Action<int> OnSetIndexToDrop;
    public event Action<BaseEventData> OnSceneObjectDropped;
    public event Action<int> OnQuickBarInputTriggered;

    [SerializeField] internal QuickBarSlot[] shortcutsImgs;
    [SerializeField] internal Button[] shortcutsButtons;
    [SerializeField] internal EventTrigger[] shortcutsEventTriggers;
    [SerializeField] internal InputAction_Trigger quickBar1InputAction;
    [SerializeField] internal InputAction_Trigger quickBar2InputAction;
    [SerializeField] internal InputAction_Trigger quickBar3InputAction;
    [SerializeField] internal InputAction_Trigger quickBar4InputAction;
    [SerializeField] internal InputAction_Trigger quickBar5InputAction;
    [SerializeField] internal InputAction_Trigger quickBar6InputAction;
    [SerializeField] internal InputAction_Trigger quickBar7InputAction;
    [SerializeField] internal InputAction_Trigger quickBar8InputAction;
    [SerializeField] internal InputAction_Trigger quickBar9InputAction;

    private const string VIEW_PATH = "Common/QuickBarView";

    internal static QuickBarView Create()
    {
        var view = Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<QuickBarView>();
        view.gameObject.name = "_QuickBarView";

        return view;
    }

    private void Awake()
    {
        for (int i = 0; i < shortcutsButtons.Length; i++)
        {
            int buttonIndex = i;
            shortcutsButtons[buttonIndex].onClick.AddListener(() => QuickBarObjectSelected(buttonIndex));
        }

        for (int i = 0; i < shortcutsEventTriggers.Length; i++)
        {
            int triggerIndex = i;
            ConfigureEventTrigger(triggerIndex, EventTriggerType.Drop, (eventData) =>
            {
                SetIndexToDrop(triggerIndex);
                SceneObjectDropped(eventData);
            });
        }

        quickBar1InputAction.OnTriggered += (action) => OnQuickBarInputTriggedered(0);
        quickBar2InputAction.OnTriggered += (action) => OnQuickBarInputTriggedered(1);
        quickBar3InputAction.OnTriggered += (action) => OnQuickBarInputTriggedered(2);
        quickBar4InputAction.OnTriggered += (action) => OnQuickBarInputTriggedered(3);
        quickBar5InputAction.OnTriggered += (action) => OnQuickBarInputTriggedered(4);
        quickBar6InputAction.OnTriggered += (action) => OnQuickBarInputTriggedered(5);
        quickBar7InputAction.OnTriggered += (action) => OnQuickBarInputTriggedered(6);
        quickBar8InputAction.OnTriggered += (action) => OnQuickBarInputTriggedered(7);
        quickBar9InputAction.OnTriggered += (action) => OnQuickBarInputTriggedered(8);
    }

    private void OnDestroy()
    {
        for (int i = 0; i < shortcutsButtons.Length; i++)
        {
            int buttonIndex = i;
            shortcutsButtons[buttonIndex].onClick.RemoveListener(() => QuickBarObjectSelected(buttonIndex));
        }

        for (int i = 0; i < shortcutsEventTriggers.Length; i++)
        {
            int triggerIndex = i;
            RemoveEventTrigger(triggerIndex, EventTriggerType.Drop);
        }

        quickBar1InputAction.OnTriggered -= (action) => OnQuickBarInputTriggedered(0);
        quickBar2InputAction.OnTriggered -= (action) => OnQuickBarInputTriggedered(1);
        quickBar3InputAction.OnTriggered -= (action) => OnQuickBarInputTriggedered(2);
        quickBar4InputAction.OnTriggered -= (action) => OnQuickBarInputTriggedered(3);
        quickBar5InputAction.OnTriggered -= (action) => OnQuickBarInputTriggedered(4);
        quickBar6InputAction.OnTriggered -= (action) => OnQuickBarInputTriggedered(5);
        quickBar7InputAction.OnTriggered -= (action) => OnQuickBarInputTriggedered(6);
        quickBar8InputAction.OnTriggered -= (action) => OnQuickBarInputTriggedered(7);
        quickBar9InputAction.OnTriggered -= (action) => OnQuickBarInputTriggedered(8);
    }

    public void ConfigureEventTrigger(int index, EventTriggerType eventType, UnityAction<BaseEventData> call)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = eventType;
        entry.callback.AddListener(call);
        shortcutsEventTriggers[index].triggers.Add(entry);
    }

    public void RemoveEventTrigger(int index, EventTriggerType eventType)
    {
        shortcutsEventTriggers[index].triggers.RemoveAll(x => x.eventID == eventType);
    }

    public void QuickBarObjectSelected(int index)
    {
        OnQuickBarObjectSelected?.Invoke(index);
    }

    public void SetIndexToDrop(int index)
    {
        OnSetIndexToDrop?.Invoke(index);
    }

    public void SceneObjectDropped(BaseEventData data)
    {
        OnSceneObjectDropped?.Invoke(data);
    }

    public void SetTextureToShortcut(int shortcutIndex, Texture texture)
    {
        if (shortcutIndex >= shortcutsImgs.Length)
            return;

        if (shortcutsImgs[shortcutIndex] != null && texture != null)
            shortcutsImgs[shortcutIndex].SetTexture(texture);
    }

    public void OnQuickBarInputTriggedered(int index)
    {
        OnQuickBarInputTriggered?.Invoke(index);
    }
}
