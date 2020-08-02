using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AvatarEditorCategoryToggleAudioHandler : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler, IPointerUpHandler
{
    HUDAudioPlayer audioPlayer;
    Selectable selectable;

    void Start()
    {
        audioPlayer = HUDAudioPlayer.i;
        selectable = GetComponent<Selectable>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (selectable.interactable && !Input.GetMouseButton(0))
        {
            audioPlayer.Play(HUDAudioPlayer.Sound.buttonHover);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (selectable.interactable)
        {
            audioPlayer.Play(HUDAudioPlayer.Sound.buttonClick);
            audioPlayer.ResetListItemAppearPitch();
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
