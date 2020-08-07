using UnityEngine;
using System;

public class ControlsHUDView : MonoBehaviour
{
    [SerializeField] internal InputAction_Trigger toggleAction;
    [SerializeField] internal InputAction_Trigger closeAction;
    [SerializeField] internal ShowHideAnimator showHideAnimator;
    [SerializeField] internal Button_OnPointerDown closeButton;

    public event Action onToggleActionTriggered;
    public event Action<bool> onCloseActionTriggered;

    private void Awake()
    {
        toggleAction.OnTriggered += OnToggleActionTriggered;
        closeAction.OnTriggered += OnCloseActionTriggered;
        closeButton.onPointerDown += () => Close(true);
    }

    private void OnDestroy()
    {
        toggleAction.OnTriggered -= OnToggleActionTriggered;
        closeAction.OnTriggered -= OnCloseActionTriggered;
    }

    private void OnToggleActionTriggered(DCLAction_Trigger action)
    {
        onToggleActionTriggered?.Invoke();
        HUDAudioPlayer.i.Play(HUDAudioPlayer.Sound.dialogAppear);
    }

    private void OnCloseActionTriggered(DCLAction_Trigger action)
    {
        Close(false);
    }

    private void Close(bool closedByButtonPress)
    {
        onCloseActionTriggered?.Invoke(closedByButtonPress);
        HUDAudioPlayer.i.Play(HUDAudioPlayer.Sound.dialogClose);
    }
}
