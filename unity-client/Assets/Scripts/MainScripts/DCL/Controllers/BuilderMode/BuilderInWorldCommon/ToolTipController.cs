using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ToolTipController : MonoBehaviour
{
    public float alphaSpeed = 3f;
    public RectTransform tooltipRT;
    public CanvasGroup tooltipCG;
    public TextMeshProUGUI tooltipTxt;

    Coroutine changeAlphaCor;

    public void Desactivate()
    {
        if (changeAlphaCor != null)
            StopCoroutine(changeAlphaCor);
        changeAlphaCor = StartCoroutine(ChangeAlpha(1, 0));
    }

    public void OnHoverEnter(BaseEventData data)
    {
    
        PointerEventData dataConverted = data as PointerEventData;

        RectTransform selectedRT = dataConverted.pointerEnter.GetComponent<RectTransform>();

        
        tooltipRT.position = selectedRT.position-Vector3.up*selectedRT.rect.height;
        if (changeAlphaCor != null)
            StopCoroutine(changeAlphaCor);
        changeAlphaCor = StartCoroutine(ChangeAlpha(0, 1));
    }

    public void SetText(string text)
    {
        tooltipTxt.text = text;
    }

    public void OnHoverExit()
    {
        Desactivate();
    }

    IEnumerator ChangeAlpha(float from, float to)
    {
        tooltipCG.alpha = from;

        float currentAlpha = from;
        float destinationAlpha = to;

        float fractionOfJourney = 0;
        float speed = alphaSpeed;
        bool exit = false;
        while (!exit)
        {
            fractionOfJourney += Time.unscaledDeltaTime * speed;
            float lerpedAlpha = Mathf.Lerp(currentAlpha, destinationAlpha, fractionOfJourney);
            tooltipCG.alpha = lerpedAlpha;
            yield return null;
            if (fractionOfJourney >= 1)
                exit = true;
        }
    }
}
