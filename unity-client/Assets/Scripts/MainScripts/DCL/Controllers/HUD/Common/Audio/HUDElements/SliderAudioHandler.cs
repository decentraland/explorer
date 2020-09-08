using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SliderAudioHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    Slider slider;
    AudioEvent eventClick, eventRelease, eventValueChanged;

    void Awake()
    {
        eventClick = Resources.Load<AudioEvent>("ScriptableObjects/AudioEvents/HUDCommon/ButtonClick");
        eventRelease = Resources.Load<AudioEvent>("ScriptableObjects/AudioEvents/HUDCommon/ButtonRelease");
        eventValueChanged = Resources.Load<AudioEvent>("ScriptableObjects/AudioEvents/HUDCommon/SliderValueChange");
        slider = GetComponent<Slider>();
        slider.onValueChanged.AddListener(OnValueChanged);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (slider != null)
        {
            if (slider.interactable)
            {
                if (eventClick != null)
                    eventClick.Play(true);
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (slider != null)
        {
            if (slider.interactable)
            {
                if (eventRelease != null)
                    eventRelease.Play(true);
            }
        }
    }

    void OnValueChanged(float value)
    {
        if (eventValueChanged != null)
        {
            eventValueChanged.SetPitch(1f + ((slider.value - slider.minValue) / (slider.maxValue - slider.minValue)) * 1.5f);
            eventValueChanged.Play(true);
        }
    }
}
