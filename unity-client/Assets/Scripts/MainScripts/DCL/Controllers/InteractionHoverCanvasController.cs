using UnityEngine;
using UnityEngine.UI;
using DCL.Interface;
using TMPro;

public class InteractionHoverCanvasController : MonoBehaviour
{
    public Vector3 offset = new Vector3(0f, 0f, 0f);
    public Canvas canvas;
    public RectTransform backgroundTransform;
    public TextMeshProUGUI text;
    public TextMeshProUGUI iconText;
    public Image iconImage;
    public Sprite pointerDownIconSprite;

    Camera mainCamera;

    void Awake()
    {
        mainCamera = Camera.main;
    }

    public void Setup(WebInterface.ACTION_BUTTON button, string feedbackText)
    {
        text.text = feedbackText;

        ConfigureIcon(button);

        Hide();
    }

    void ConfigureIcon(WebInterface.ACTION_BUTTON button)
    {
        // When we allow for custom input key bindings this implementation will change
        switch (button)
        {
            case WebInterface.ACTION_BUTTON.POINTER:
                iconText.text = "";
                iconImage.sprite = pointerDownIconSprite;

                iconImage.SetNativeSize();
                iconImage.rectTransform.anchorMin = new Vector2(0, 0);
                iconImage.rectTransform.anchorMax = new Vector2(1, 1);
                iconImage.rectTransform.sizeDelta = Vector2.zero;
                iconImage.rectTransform.pivot = new Vector2(0.5f, 0.5f);

                iconImage.color = new Color(iconImage.color.r, iconImage.color.g, iconImage.color.b, 1f);
                break;
            case WebInterface.ACTION_BUTTON.PRIMARY:
                iconText.text = "E";
                iconImage.color = new Color(iconImage.color.r, iconImage.color.g, iconImage.color.b, 0f);
                break;
            case WebInterface.ACTION_BUTTON.SECONDARY:
                iconText.text = "F";
                iconImage.color = new Color(iconImage.color.r, iconImage.color.g, iconImage.color.b, 0f);
                break;
        }
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
        if (mainCamera == null)
            mainCamera = Camera.main;

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
