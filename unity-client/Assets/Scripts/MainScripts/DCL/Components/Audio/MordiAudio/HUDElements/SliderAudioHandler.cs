using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SliderAudioHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    HUDAudioPlayer audioPlayer;
    Selectable selectable;

    void Start()
    {
        audioPlayer = HUDAudioPlayer.i;
        selectable = GetComponent<Selectable>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (selectable.interactable)
        {
            audioPlayer.Play(HUDAudioPlayer.Sound.buttonClick);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (selectable.interactable)
        {
            audioPlayer.Play(HUDAudioPlayer.Sound.buttonRelease);
        }
    }
}
