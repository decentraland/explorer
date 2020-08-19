using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GeneralHUDElementAudioHandler : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler, IPointerUpHandler
{
    HUDAudioPlayer audioPlayer;
    [SerializeField]
    bool playHover = false;

    void Start()
    {
        audioPlayer = HUDAudioPlayer.i;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (playHover)
        {
            audioPlayer.Play(HUDAudioPlayer.Sound.buttonHover);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        audioPlayer.Play(HUDAudioPlayer.Sound.buttonClick);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        audioPlayer.Play(HUDAudioPlayer.Sound.buttonRelease);
    }
}