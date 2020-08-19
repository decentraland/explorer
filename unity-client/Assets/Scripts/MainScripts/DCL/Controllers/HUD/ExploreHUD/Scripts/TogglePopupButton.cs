using System;
using UnityEngine;
using UnityEngine.EventSystems;

internal class TogglePopupButton : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    public event Action OnPressed;

    [SerializeField] ShowHideAnimator tooltip;

    void Awake()
    {
        tooltip.gameObject.SetActive(false);
    }

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        OnPressed?.Invoke();
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        if (!tooltip.gameObject.activeSelf)
        {
            tooltip.gameObject.SetActive(true);
        }
        tooltip.Show();
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        tooltip.Hide();
    }
}
