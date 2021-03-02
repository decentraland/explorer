using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public interface ITooltipView
{
    float alphaTranstionSpeed { get; }

    event Action OnHideTooltip;
    event Action<BaseEventData> OnShowTooltip;

    void SetText(string text);
    void SetTooltipPosition(Vector3 pos);
    void SetTooltipAlpha(float alphaValue);
}

public class TooltipView : MonoBehaviour, ITooltipView
{
    public float alphaTranstionSpeed => alphaSpeed;

    public event System.Action<BaseEventData> OnShowTooltip;
    public event System.Action OnHideTooltip;

    [SerializeField] internal float alphaSpeed = 3f;
    [SerializeField] internal RectTransform tooltipRT;
    [SerializeField] internal CanvasGroup tooltipCG;
    [SerializeField] internal TextMeshProUGUI tooltipTxt;

    public void SetTooltipPosition(Vector3 pos)
    {
        tooltipRT.position = pos;
    }

    public void SetText(string text)
    {
        tooltipTxt.text = text;
    }

    public void SetTooltipAlpha(float alphaValue)
    {
        tooltipCG.alpha = alphaValue;
    }
}
