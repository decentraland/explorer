using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ToggleAudioHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    Toggle toggle;
    AudioEvent eventClick, eventEnable, eventDisable;

    void Awake()
    {
        eventClick = Resources.Load<AudioEvent>("ScriptableObjects/AudioEvents/HUDCommon/ButtonClick");
        eventEnable = Resources.Load<AudioEvent>("ScriptableObjects/AudioEvents/HUDCommon/Enable");
        eventDisable = Resources.Load<AudioEvent>("ScriptableObjects/AudioEvents/HUDCommon/Disable");

        toggle = GetComponent<Toggle>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (toggle != null)
        {
            if (toggle.interactable)
            {
                if (eventClick != null)
                    eventClick.Play(true);
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (toggle != null)
        {
            if (toggle.interactable)
            {
                if (toggle.isOn)
                {
                    if (eventDisable != null)
                        eventDisable.Play(true);
                }
                else
                {
                    if (eventEnable != null)
                        eventEnable.Play(true);
                }
            }
        }
    }
}