using UnityEngine;
using UnityEngine.EventSystems;

public class QuickBarView : MonoBehaviour
{
    public event System.Action<int> OnQuickBarObjectSelected;
    public event System.Action<int> OnSetIndexToDrop;
    public event System.Action<BaseEventData> OnSceneObjectDropped;
    public event System.Action<int> OnQuickBarInputTriggered;

    [SerializeField] internal QuickBarSlot[] shortcutsImgs;
    [SerializeField] internal InputAction_Trigger quickBar1InputAction;
    [SerializeField] internal InputAction_Trigger quickBar2InputAction;
    [SerializeField] internal InputAction_Trigger quickBar3InputAction;
    [SerializeField] internal InputAction_Trigger quickBar4InputAction;
    [SerializeField] internal InputAction_Trigger quickBar5InputAction;
    [SerializeField] internal InputAction_Trigger quickBar6InputAction;
    [SerializeField] internal InputAction_Trigger quickBar7InputAction;
    [SerializeField] internal InputAction_Trigger quickBar8InputAction;
    [SerializeField] internal InputAction_Trigger quickBar9InputAction;

    private void Start()
    {
        quickBar1InputAction.OnTriggered += OnQuickBar1InputTriggedered;
        quickBar2InputAction.OnTriggered += OnQuickBar2InputTriggedered;
        quickBar3InputAction.OnTriggered += OnQuickBar3InputTriggedered;
        quickBar4InputAction.OnTriggered += OnQuickBar4InputTriggedered;
        quickBar5InputAction.OnTriggered += OnQuickBar5InputTriggedered;
        quickBar6InputAction.OnTriggered += OnQuickBar6InputTriggedered;
        quickBar7InputAction.OnTriggered += OnQuickBar7InputTriggedered;
        quickBar8InputAction.OnTriggered += OnQuickBar8InputTriggedered;
        quickBar9InputAction.OnTriggered += OnQuickBar9InputTriggedered;
    }

    private void OnDestroy()
    {
        quickBar1InputAction.OnTriggered -= OnQuickBar1InputTriggedered;
        quickBar2InputAction.OnTriggered -= OnQuickBar2InputTriggedered;
        quickBar3InputAction.OnTriggered -= OnQuickBar3InputTriggedered;
        quickBar4InputAction.OnTriggered -= OnQuickBar4InputTriggedered;
        quickBar5InputAction.OnTriggered -= OnQuickBar5InputTriggedered;
        quickBar6InputAction.OnTriggered -= OnQuickBar6InputTriggedered;
        quickBar7InputAction.OnTriggered -= OnQuickBar7InputTriggedered;
        quickBar8InputAction.OnTriggered -= OnQuickBar8InputTriggedered;
        quickBar9InputAction.OnTriggered -= OnQuickBar9InputTriggedered;
    }

    // TODO (Santi): Called from imspector!
    public void QuickBarObjectSelected(int index)
    {
        OnQuickBarObjectSelected?.Invoke(index);
    }

    // TODO (Santi): Called from imspector!
    public void SetIndexToDrop(int index)
    {
        OnSetIndexToDrop?.Invoke(index);
    }

    // TODO (Santi): Called from imspector!
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

    private void OnQuickBar1InputTriggedered(DCLAction_Trigger action)
    {
        OnQuickBarInputTriggered?.Invoke(0);
    }

    private void OnQuickBar2InputTriggedered(DCLAction_Trigger action)
    {
        OnQuickBarInputTriggered?.Invoke(1);
    }

    private void OnQuickBar3InputTriggedered(DCLAction_Trigger action)
    {
        OnQuickBarInputTriggered?.Invoke(2);
    }

    private void OnQuickBar4InputTriggedered(DCLAction_Trigger action)
    {
        OnQuickBarInputTriggered?.Invoke(3);
    }

    private void OnQuickBar5InputTriggedered(DCLAction_Trigger action)
    {
        OnQuickBarInputTriggered?.Invoke(4);
    }

    private void OnQuickBar6InputTriggedered(DCLAction_Trigger action)
    {
        OnQuickBarInputTriggered?.Invoke(5);
    }

    private void OnQuickBar7InputTriggedered(DCLAction_Trigger action)
    {
        OnQuickBarInputTriggered?.Invoke(6);
    }

    private void OnQuickBar8InputTriggedered(DCLAction_Trigger action)
    {
        OnQuickBarInputTriggered?.Invoke(7);
    }

    private void OnQuickBar9InputTriggedered(DCLAction_Trigger action)
    {
        OnQuickBarInputTriggered?.Invoke(8);
    }
}
