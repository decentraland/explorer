using UnityEngine;
using UnityEngine.EventSystems;

public class FriendEntryJumpInButtonView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public float hoverTime = 1f;
    public GameObject locationText;

    float hoverCounter = 0f;

    public void OnPointerEnter(PointerEventData eventData)
    {
        hoverCounter = hoverTime;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hoverCounter = 0f;
        locationText.SetActive(false);
    }

    void OnDisable()
    {
        OnPointerExit(null);
    }

    void Update()
    {
        if (locationText.activeSelf || hoverCounter <= 0f) return;

        hoverCounter -= Time.deltaTime;
        if (hoverCounter <= 0f)
        {
            hoverCounter = 0f;

            locationText.SetActive(true);
        }
    }
}
