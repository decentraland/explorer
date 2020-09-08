using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonAudioHandler : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler, IPointerUpHandler
{
    protected Selectable selectable;
    [SerializeField]
    HUDAudioPlayer.Sound extraClickSound = HUDAudioPlayer.Sound.none;
    [SerializeField]
    AudioEvent extraClickEvent = null;
    [SerializeField]
    bool playHoverSound = true;

    AudioEvent eventClick, eventHover, eventRelease;

    void Start()
    {
        eventClick = Resources.Load<AudioEvent>("ScriptableObjects/AudioEvents/HUDCommon/ButtonClick");
        eventHover = Resources.Load<AudioEvent>("ScriptableObjects/AudioEvents/HUDCommon/ButtonHover");
        eventRelease = Resources.Load<AudioEvent>("ScriptableObjects/AudioEvents/HUDCommon/ButtonRelease");

        selectable = GetComponent<Selectable>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!playHoverSound)
            return;

        if (selectable != null && !Input.GetMouseButton(0))
        {
            if (selectable.interactable)
            {
                if (eventHover != null)
                    eventHover.Play(true);
            }
        }
    }

    public virtual void OnPointerDown(PointerEventData eventData)
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

    public void OnPointerUp(PointerEventData eventData)
    {
        if (selectable != null)
        {
            if (selectable.interactable)
            {
                if (eventRelease != null)
                    eventRelease.Play(true);

                if (extraClickEvent != null)
                    extraClickEvent.Play(true);
            }
        }
    }
}