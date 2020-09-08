using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ToggleHandleAudioHandler : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField]
    Toggle toggle;

    AudioEvent eventHover;

    void Awake()
    {
        eventHover = Resources.Load<AudioEvent>("ScriptableObjects/AudioEvents/HUDCommon/ButtonHover");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (toggle != null && !Input.GetMouseButton(0))
        {
            if (toggle.interactable)
            {
                if (eventHover != null)
                    eventHover.Play(true);
            }
        }
    }
}