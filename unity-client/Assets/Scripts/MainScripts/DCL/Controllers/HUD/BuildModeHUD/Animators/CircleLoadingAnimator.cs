using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CircleLoadingAnimator : MonoBehaviour
{
    public float animSpeed = 6f;
    Image fillImage;

    Coroutine coroutine;
 

    private void OnEnable()
    {
        if (fillImage == null)
            fillImage = GetComponent<Image>();

        coroutine = StartCoroutine(LoadinAnim());
    }

    private void OnDisable()
    {
        StopCoroutine(coroutine);
    }


    IEnumerator LoadinAnim()
    {
        fillImage.fillAmount = 0;
        float currentSpeed = animSpeed * Time.deltaTime;
        while(true)
        {
            fillImage.fillAmount += currentSpeed;

            if (fillImage.fillAmount >= 1)
                currentSpeed = -animSpeed * Time.deltaTime;
            else if (fillImage.fillAmount <= 0)
                currentSpeed = animSpeed * Time.deltaTime;

            yield return null;
        }
    }
}
