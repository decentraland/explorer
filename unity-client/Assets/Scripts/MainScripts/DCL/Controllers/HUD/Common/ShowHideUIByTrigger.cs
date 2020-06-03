using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Attaching this component to a canvas, will hide/show it after triggering the ToggleUIVisibility input action.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class ShowHideUIByTrigger : MonoBehaviour
{
    private const string TOGGLE_UI_VISIBILITY_ASSET_NAME = "ToggleUIVisibility";

    private InputAction_Trigger toggleTrigger;
    private CanvasGroup canvasGroup;
    private bool isVisible;

    private void Awake()
    {
        toggleTrigger = Resources.Load<InputAction_Trigger>(TOGGLE_UI_VISIBILITY_ASSET_NAME);
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
        bool anyInputFieldIsSelected = EventSystem.current != null &&
            EventSystem.current.currentSelectedGameObject != null &&
            EventSystem.current.currentSelectedGameObject.GetComponent<TMPro.TMP_InputField>() != null;

        if (anyInputFieldIsSelected)
            return;

        EventSystem.current.SetSelectedGameObject(null);
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
