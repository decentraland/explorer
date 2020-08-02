using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SliderHandleAudioHandler : MonoBehaviour, IPointerEnterHandler
{
    HUDAudioPlayer audioPlayer;
    [SerializeField]
    Slider slider;

    void Start()
    {
        audioPlayer = HUDAudioPlayer.i;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (slider.interactable && !Input.GetMouseButton(0))
        {
            audioPlayer.Play(HUDAudioPlayer.Sound.buttonHover);
        }
    }
}