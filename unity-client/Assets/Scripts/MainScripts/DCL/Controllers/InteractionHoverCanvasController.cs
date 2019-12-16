using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InteractionHoverCanvasController : MonoBehaviour
{
    public Vector3 offset = new Vector3(0, 1f, 0);
    public RectTransform backgroundTransform;
    public Image icon;
    public TextMeshProUGUI text;

    Canvas canvas;
    Camera mainCamera;

    void Awake()
    {
        mainCamera = Camera.main;
        canvas = GetComponent<Canvas>();
    }

    public void Setup(Sprite feedbackIconSprite, string feedbackText)
    {
        if (feedbackIconSprite != null)
            icon.sprite = feedbackIconSprite;

        text.text = feedbackText;

        UpdateSizeAndPos();

        Hide();
    }

    public void Show()
    {
        canvas.enabled = true;
    }

    public void Hide()
    {
        canvas.enabled = false;
    }

    public void UpdateSizeAndPos()
    {
        Vector3 screenPoint = mainCamera == null ? Vector3.zero : mainCamera.WorldToViewportPoint(transform.position + offset);
        // uiContainer.alpha = 1.0f + (1.0f - (screenPoint.z / NAME_VANISHING_POINT_DISTANCE));

        if (screenPoint.z > 0)
        {
            /* if (!uiContainer.gameObject.activeSelf)
            {
                uiContainer.gameObject.SetActive(true);
            } */

            RectTransform canvasRect = (RectTransform)canvas.transform;
            float width = canvasRect.rect.width;
            float height = canvasRect.rect.height;
            screenPoint.Scale(new Vector3(width, height, 0));
            ((RectTransform)transform).anchoredPosition = screenPoint;
        }
        /* else
        {
            if (uiContainer.gameObject.activeSelf)
            {
                uiContainer.gameObject.SetActive(false);
            }
        } */
    }
}
