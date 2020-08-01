using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonAudioHandler : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler, IPointerUpHandler
{
    Selectable selectable;
    AudioEvent eventHover, eventClick, eventRelease;

    void Start()
    {
        AudioContainer ac = HUDAudioPlayer.i.audioContainer;
        eventHover = ac.GetEvent("ButtonHover");
        eventClick = ac.GetEvent("ButtonClick");
        eventRelease = ac.GetEvent("ButtonRelease");

        selectable = GetComponent<Selectable>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (selectable.interactable) {
            eventHover.Play();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (selectable.interactable) {
            eventClick.Play();
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (selectable.interactable) {
            eventRelease.Play();
        }
    }
}