using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonAudioHandler : GeneralHUDElementAudioHandler
{
    protected Selectable selectable;
    [SerializeField]
    HUDAudioPlayer.Sound extraClickSound = HUDAudioPlayer.Sound.none;
    [SerializeField]
    AudioEvent extraClickEvent = null;
    [SerializeField]
    bool playHoverSound = true;

    public override void Awake()
    {
        base.Awake();

        selectable = GetComponent<Selectable>();
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        if (selectable != null)
        {
            if (selectable.interactable)
            {
                base.OnPointerEnter(eventData);
            }
        }
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        if (selectable != null)
        {
            if (selectable.interactable)
            {
                base.OnPointerDown(eventData);
            }
        }
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        if (selectable != null)
        {
            if (selectable.interactable)
            {
                base.OnPointerUp(eventData);

                if (extraClickEvent != null)
                    extraClickEvent.Play(true);
            }
        }
    }
}