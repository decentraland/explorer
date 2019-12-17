using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InteractionHoverCanvasController : MonoBehaviour
{
    public Vector3 offset = new Vector3(0f, 0f, 0f);
    public Canvas canvas;
    public RectTransform backgroundTransform;
    public Image icon;
    public TextMeshProUGUI text;

    Camera mainCamera;

    void Awake()
    {
        mainCamera = Camera.main;
    }

    public void Setup(Sprite feedbackIconSprite, string feedbackText)
    {
        if (feedbackIconSprite != null)
            icon.sprite = feedbackIconSprite;

        text.text = feedbackText;

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

    void LateUpdate()
    {
        if (!canvas.enabled) return;

        UpdateSizeAndPos();
    }

    public void UpdateSizeAndPos()
    {
        Vector3 screenPoint = mainCamera.WorldToViewportPoint(transform.parent.position + offset);

        if (screenPoint.z > 0)
        {
            RectTransform canvasRect = (RectTransform)canvas.transform;
            float width = canvasRect.rect.width;
            float height = canvasRect.rect.height;
            screenPoint.Scale(new Vector3(width, height, 0));

            ((RectTransform)backgroundTransform).anchoredPosition = screenPoint;
        }
    }
}
