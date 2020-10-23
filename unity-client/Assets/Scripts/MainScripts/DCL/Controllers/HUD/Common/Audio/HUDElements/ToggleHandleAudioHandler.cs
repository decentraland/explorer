using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ToggleHandleAudioHandler : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField]
    Toggle toggle;

    private void Awake()
    {
        DestroyImmediate(this);
        return;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (toggle != null && !Input.GetMouseButton(0))
        {
            if (toggle.interactable)
            {
                AudioScriptableObjects.buttonHover.Play(true);
            }
        }
    }
}