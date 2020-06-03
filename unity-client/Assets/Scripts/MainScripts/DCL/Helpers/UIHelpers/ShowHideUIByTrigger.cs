using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class ShowHideUIByTrigger : MonoBehaviour
{
    private InputAction_Trigger toggleTrigger;
    private CanvasGroup canvasGroup;
    private bool isVisible;

    private void Awake()
    {
        toggleTrigger = Resources.Load<InputAction_Trigger>("ToggleUIVisibility");
        toggleTrigger.OnTriggered += ToggleTrigger_OnTriggered;

        canvasGroup = GetComponent<CanvasGroup>();
        isVisible = canvasGroup.alpha > 0f;
    }

    private void OnDestroy()
    {
        toggleTrigger.OnTriggered -= ToggleTrigger_OnTriggered;
    }

    private void ToggleTrigger_OnTriggered(DCLAction_Trigger action)
    {
        isVisible = !isVisible;
        SetUIVisibility(isVisible);
    }

    private void SetUIVisibility(bool isVisible)
    {
        canvasGroup.alpha = isVisible ? 1f : 0f;
        canvasGroup.interactable = isVisible;
        canvasGroup.blocksRaycasts = isVisible;
    }
}
