using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public interface IToolTipController
{
    void Initialize(float alphaSpeed, RectTransform tooltipRT, CanvasGroup tooltipCG, TextMeshProUGUI tooltipTxt);
    void SetTooltipText(string text);
    void ShowTooltip(BaseEventData data);
    void HideTooltip();
    void KillTooltipCoroutine();
}

public class ToolTipController : IToolTipController
{
    private float alphaSpeed = 3f;
    private RectTransform tooltipRT;
    private CanvasGroup tooltipCG;
    private TextMeshProUGUI tooltipTxt;
    private Coroutine changeAlphaCoroutine;

    public void Initialize(float alphaSpeed, RectTransform tooltipRT, CanvasGroup tooltipCG, TextMeshProUGUI tooltipTxt)
    {
        this.alphaSpeed = alphaSpeed;
        this.tooltipRT = tooltipRT;
        this.tooltipCG = tooltipCG;
        this.tooltipTxt = tooltipTxt;
    }

    public void SetTooltipText(string text)
    {
        tooltipTxt.text = text;
    }

    public void ShowTooltip(BaseEventData data)
    {
        if (!(data is PointerEventData dataConverted))
            return;

        RectTransform selectedRT = dataConverted.pointerEnter.GetComponent<RectTransform>();
        tooltipRT.position = selectedRT.position - Vector3.up * selectedRT.rect.height;

        KillTooltipCoroutine();

        changeAlphaCoroutine = CoroutineStarter.Start(ChangeAlpha(0, 1));

    }

    public void HideTooltip()
    {
        KillTooltipCoroutine();
        changeAlphaCoroutine = CoroutineStarter.Start(ChangeAlpha(1, 0));
    }

    public void KillTooltipCoroutine()
    {
        if (changeAlphaCoroutine != null)
            CoroutineStarter.Stop(changeAlphaCoroutine);
    }

    private IEnumerator ChangeAlpha(float from, float to)
    {
        tooltipCG.alpha = from;

        float currentAlpha = from;
        float destinationAlpha = to;

        float fractionOfJourney = 0;
        float speed = alphaSpeed;
        while (fractionOfJourney < 1)
        {
            fractionOfJourney += Time.unscaledDeltaTime * speed;
            float lerpedAlpha = Mathf.Lerp(currentAlpha, destinationAlpha, fractionOfJourney);
            tooltipCG.alpha = lerpedAlpha;
            yield return null;
        }
        changeAlphaCoroutine = null;
    }
}
