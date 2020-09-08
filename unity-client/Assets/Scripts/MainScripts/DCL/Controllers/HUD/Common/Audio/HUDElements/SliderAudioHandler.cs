using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SliderAudioHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    Slider slider;
    AudioEvent eventValueChanged;

    void Awake()
    {
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
                AudioScriptableObjects.buttonClick.Play(true);
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (slider != null)
        {
            if (slider.interactable)
            {
                AudioScriptableObjects.buttonRelease.Play(true);
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
