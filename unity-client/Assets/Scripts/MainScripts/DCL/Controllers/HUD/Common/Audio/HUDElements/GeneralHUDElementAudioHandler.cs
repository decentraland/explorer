using UnityEngine;
using UnityEngine.EventSystems;

public class GeneralHUDElementAudioHandler : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField]
    HUDAudioPlayer.Sound hoverSound = HUDAudioPlayer.Sound.buttonHover;
    [SerializeField]
    protected bool playHover = true, playClick = true, playRelease = true;

    AudioEvent eventClick, eventHover, eventRelease;

    public virtual void Awake()
    {
        eventClick = Resources.Load<AudioEvent>("ScriptableObjects/AudioEvents/HUDCommon/ButtonClick");
        eventHover = Resources.Load<AudioEvent>("ScriptableObjects/AudioEvents/HUDCommon/ButtonHover");
        eventRelease = Resources.Load<AudioEvent>("ScriptableObjects/AudioEvents/HUDCommon/ButtonRelease");
    }

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        if (!playHover)
            return;

        if (!Input.GetMouseButton(0))
        {
            if (eventHover != null)
                eventHover.Play(true);
        }
    }

    public virtual void OnPointerDown(PointerEventData eventData)
    {
        if (!playClick)
            return;

        if (eventClick != null)
            eventClick.Play(true);
    }

    public virtual void OnPointerUp(PointerEventData eventData)
    {
        if (!playRelease)
            return;

        if (eventRelease != null)
            eventRelease.Play(true);
    }
}