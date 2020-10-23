using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScrollbarHandleAudioHandler : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler
{
    [SerializeField]
    Selectable selectable;

    private void Awake()
    {
        DestroyImmediate(this);
        return;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (selectable != null && !Input.GetMouseButton(0))
        {
            if (selectable.interactable)
            {
                AudioScriptableObjects.buttonHover.Play(true);
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (selectable != null)
        {
            if (selectable.interactable)
            {
                AudioScriptableObjects.buttonClick.Play(true);
            }
        }
    }
}