using UnityEngine;
using DCL.Interface;
using TMPro;

public class InteractionHoverCanvasController : MonoBehaviour
{
    public Vector3 offset = new Vector3(0f, 0f, 0f);
    public Canvas canvas;
    public RectTransform backgroundTransform;
    public TextMeshProUGUI text;

    Camera mainCamera;
    GameObject hoverIcon;

    const string POINTER_ICON_PREFAB_PATH = "PointerButtonHoverIcon";
    const string PRIMARY_ICON_PREFAB_PATH = "PrimaryButtonHoverIcon";
    const string SECONDARY_ICON_PREFAB_PATH = "SecondaryButtonHoverIcon";

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

        if (hoverIcon != null)
            Destroy(hoverIcon);

        string prefabPath;
        switch (button)
        {
            case WebInterface.ACTION_BUTTON.PRIMARY:
                prefabPath = PRIMARY_ICON_PREFAB_PATH;
                break;
            case WebInterface.ACTION_BUTTON.SECONDARY:
                prefabPath = SECONDARY_ICON_PREFAB_PATH;
                break;
            default: // WebInterface.ACTION_BUTTON.POINTER
                prefabPath = POINTER_ICON_PREFAB_PATH;
                break;
        }

        hoverIcon = Object.Instantiate(Resources.Load(prefabPath), backgroundTransform) as GameObject;
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
