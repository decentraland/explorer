using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScrollbarHandleAudioHandler : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler
{
    [SerializeField]
    Selectable selectable;

    AudioEvent eventClick, eventHover;

    void Start()
    {
        eventClick = Resources.Load<AudioEvent>("ScriptableObjects/AudioEvents/HUDCommon/ButtonClick");
        eventHover = Resources.Load<AudioEvent>("ScriptableObjects/AudioEvents/HUDCommon/ButtonHover");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (selectable != null && !Input.GetMouseButton(0))
        {
            if (selectable.interactable)
            {
                if (eventHover != null)
                    eventHover.Play(true);
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (selectable != null)
        {
            if (selectable.interactable)
            {
                if (eventClick != null)
                    eventClick.Play(true);
            }
        }
    }
}