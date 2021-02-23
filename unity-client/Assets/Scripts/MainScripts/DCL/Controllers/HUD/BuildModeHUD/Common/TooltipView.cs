using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipView : MonoBehaviour
{
    public event System.Action<BaseEventData> OnShowTooltip;
    public event System.Action OnHideTooltip;

    [SerializeField] internal float alphaSpeed = 3f;
    [SerializeField] internal RectTransform tooltipRT;
    [SerializeField] internal CanvasGroup tooltipCG;
    [SerializeField] internal TextMeshProUGUI tooltipTxt;

    public void SetText(string text)
    {
        tooltipTxt.text = text;
    }

    public void SetTooltipPosition(Vector3 pos)
    {
        tooltipRT.position = pos;
    }

    public void OnHoverEnter(BaseEventData data)
    {
        OnShowTooltip?.Invoke(data);
    }

    public void OnHoverExit()
    {
        OnHideTooltip?.Invoke();
    }
}
