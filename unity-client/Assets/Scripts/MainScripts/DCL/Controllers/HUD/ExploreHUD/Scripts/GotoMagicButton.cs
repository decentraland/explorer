using UnityEngine;
using UnityEngine.EventSystems;
using System;

internal class GotoMagicButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    [SerializeField] Animator buttonAnimator;

    public event Action OnGotoMagicPressed;

    void OnEnable()
    {
        buttonAnimator.SetTrigger("Reset");
    }

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        OnGotoMagicPressed?.Invoke();
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        buttonAnimator.SetTrigger("Highlighted");
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        buttonAnimator.SetTrigger("Normal");
    }
}
