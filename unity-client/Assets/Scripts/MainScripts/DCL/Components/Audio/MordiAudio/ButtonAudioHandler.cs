using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonAudioHandler : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField]
    AudioContainer audioContainer;
    AudioEvent eventHover, eventClick, eventRelease;

    void Start()
    {
        eventHover = audioContainer.GetEvent("Hover");
        eventClick = audioContainer.GetEvent("Click");
        eventRelease = audioContainer.GetEvent("Release");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        eventHover.Play();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        eventClick.Play();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        eventRelease.Play();
    }
}