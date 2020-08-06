using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class UIHoverCallback : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public event Action OnPointerEnter;
    public event Action OnPointerExit;

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        OnPointerEnter?.Invoke();
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        OnPointerExit?.Invoke();
    }
}
