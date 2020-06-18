using UnityEngine;
using System;

public class ControlsHUDView : MonoBehaviour
{
    [SerializeField] internal InputAction_Trigger toggleAction;
    [SerializeField] internal InputAction_Trigger closeAction;
    [SerializeField] internal ShowHideAnimator showHideAnimator;
    [SerializeField] internal Button_OnPointerDown closeButton;

    public event Action onToggleActionTriggered;
    public event Action onCloseActionTriggered;

    private void Awake()
    {
        toggleAction.OnTriggered += OnToggleActionTriggered;
        closeAction.OnTriggered += OnCloseActionTriggered;
        closeButton.onPointerDown += () => OnCloseActionTriggered(DCLAction_Trigger.CloseWindow);
    }

    private void OnDestroy()
    {
        toggleAction.OnTriggered -= OnToggleActionTriggered;
        closeAction.OnTriggered -= OnCloseActionTriggered;
    }

    private void OnToggleActionTriggered(DCLAction_Trigger action)
    {
        onToggleActionTriggered?.Invoke();
    }

    private void OnCloseActionTriggered(DCLAction_Trigger action)
    {
        onCloseActionTriggered?.Invoke();
    }
}
