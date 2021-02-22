using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public interface ITooltipView
{
    void SetText(string text);
    void OnHoverEnter(BaseEventData data);
    void OnHoverExit();
}

public class TooltipView : MonoBehaviour, ITooltipView
{
    private const string VIEW_PATH = "ToolTip";

    public float alphaSpeed = 3f;
    public RectTransform tooltipRT;
    public CanvasGroup tooltipCG;
    public TextMeshProUGUI tooltipTxt;

    private Coroutine changeAlphaCoroutine;

    public void SetText(string text)
    {
        tooltipTxt.text = text;
    }

    public void OnHoverEnter(BaseEventData data)
    {
        if (!(data is PointerEventData dataConverted))
            return;

        RectTransform selectedRT = dataConverted.pointerEnter.GetComponent<RectTransform>();
        tooltipRT.position = selectedRT.position - Vector3.up * selectedRT.rect.height;

        KillTooltipCoroutine();

        changeAlphaCoroutine = CoroutineStarter.Start(ChangeAlpha(0, 1));
    }

    public void OnHoverExit()
    {
        KillTooltipCoroutine();
        changeAlphaCoroutine = CoroutineStarter.Start(ChangeAlpha(1, 0));
    }

    private void OnDestroy()
    {
        KillTooltipCoroutine();
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

    private void KillTooltipCoroutine()
    {
        if (changeAlphaCoroutine != null)
            CoroutineStarter.Stop(changeAlphaCoroutine);
    }

    internal static ITooltipView Create()
    {
        var view = Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<TooltipView>();
        view.gameObject.name = "_TooltipView";

        return view;
    }
}
