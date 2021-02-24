using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public interface ITooltipController
{
    void Initialize(TooltipView view);
    void Dispose();
    void SetTooltipText(string text);
    void ShowTooltip(BaseEventData data);
    void HideTooltip();
}

public class TooltipController : ITooltipController
{
    private TooltipView view;
    private Coroutine changeAlphaCoroutine;

    public void Initialize(TooltipView view)
    {
        this.view = view;

        view.OnShowTooltip += ShowTooltip;
        view.OnHideTooltip += HideTooltip;
    }

    public void Dispose()
    {
        KillTooltipCoroutine();

        view.OnShowTooltip -= ShowTooltip;
        view.OnHideTooltip -= HideTooltip;
    }

    public void SetTooltipText(string text)
    {
        view.SetText(text);
    }

    public void ShowTooltip(BaseEventData data)
    {
        if (!(data is PointerEventData dataConverted))
            return;

        RectTransform selectedRT = dataConverted.pointerEnter.GetComponent<RectTransform>();
        view.SetTooltipPosition(selectedRT.position - Vector3.up * selectedRT.rect.height);

        KillTooltipCoroutine();

        changeAlphaCoroutine = CoroutineStarter.Start(ChangeAlpha(0, 1));
    }

    public void HideTooltip()
    {
        KillTooltipCoroutine();
        changeAlphaCoroutine = CoroutineStarter.Start(ChangeAlpha(1, 0));
    }

    private IEnumerator ChangeAlpha(float from, float to)
    {
        view.tooltipCG.alpha = from;

        float currentAlpha = from;
        float destinationAlpha = to;

        float fractionOfJourney = 0;
        float speed = view.alphaSpeed;
        while (fractionOfJourney < 1)
        {
            fractionOfJourney += Time.unscaledDeltaTime * speed;
            float lerpedAlpha = Mathf.Lerp(currentAlpha, destinationAlpha, fractionOfJourney);
            view.tooltipCG.alpha = lerpedAlpha;
            yield return null;
        }
        changeAlphaCoroutine = null;
    }

    private void KillTooltipCoroutine()
    {
        if (changeAlphaCoroutine != null)
            CoroutineStarter.Stop(changeAlphaCoroutine);
    }
}
