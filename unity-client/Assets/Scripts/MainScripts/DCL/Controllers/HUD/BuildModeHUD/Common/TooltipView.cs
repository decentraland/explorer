using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipView : MonoBehaviour
{
    public float alphaSpeed = 3f;
    public RectTransform tooltipRT;
    public CanvasGroup tooltipCG;
    public TextMeshProUGUI tooltipTxt;

    private ToolTipController tooltipController;

    public void Initialize(ToolTipController controller)
    {
        tooltipController = controller;
        tooltipController.Initialize(alphaSpeed, tooltipRT, tooltipCG, tooltipTxt);
    }

    private void OnDestroy()
    {
        tooltipController?.KillTooltipCoroutine();
    }

    public void OnHoverEnter(BaseEventData data)
    {
        tooltipController?.ShowTooltip(data);
    }

    public void SetText(string text)
    {
        tooltipController?.SetTooltipText(text);
    }

    public void OnHoverExit()
    {
        tooltipController?.HideTooltip();
    }
}
