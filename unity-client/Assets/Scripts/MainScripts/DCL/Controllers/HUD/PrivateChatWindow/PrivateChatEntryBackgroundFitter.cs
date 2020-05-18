using UnityEngine;
using TMPro;

[ExecuteInEditMode]
public class PrivateChatEntryBackgroundFitter : MonoBehaviour
{
    public RectTransform rectTransform;
    public RectTransform parentContainerRectTransform;
    public TextMeshProUGUI messageText;

    void Update()
    {
        Vector2 textSize = new Vector2(messageText.textBounds.size.x + messageText.margin.x * 2, messageText.textBounds.size.y + messageText.margin.y * 2);

        if (parentContainerRectTransform)
        {
            parentContainerRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, textSize.x);
            parentContainerRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, textSize.y);
            parentContainerRectTransform.ForceUpdateRectTransforms();
        }

        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, textSize.x);
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, textSize.y);
        rectTransform.ForceUpdateRectTransforms();
    }
}
