using DCL.Interface;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class RectTransformExtensions
{
    public static int CountCornersVisibleFrom(this RectTransform rectTransform, RectTransform viewport)
    {
        Vector3[] viewCorners = new Vector3[4];
        viewport.GetWorldCorners(viewCorners);

        Vector2 size = new Vector2(viewport.rect.size.x * viewport.lossyScale.x, viewport.rect.size.y * viewport.lossyScale.y);
        Rect screenBounds = new Rect(viewCorners[0], size); // Screen space bounds (assumes camera renders across the entire screen)

        Vector3[] objectCorners = new Vector3[4];
        rectTransform.GetWorldCorners(objectCorners);

        int visibleCorners = 0;

        for (var i = 0; i < viewCorners.Length; i++)
        {
            if (i != viewCorners.Length - 1)
                Debug.DrawLine(viewCorners[i], viewCorners[i + 1], Color.blue, 1.0f);
        }

        for (var i = 0; i < objectCorners.Length; i++) // For each corner in rectTransform
        {
            if (screenBounds.Contains(objectCorners[i])) // If the corner is inside the screen
            {
                visibleCorners++;
            }
        }

        for (var i = 0; i < objectCorners.Length; i++) // For each corner in rectTransform
        {
            if (i != objectCorners.Length - 1)
                Debug.DrawLine(objectCorners[i], objectCorners[i + 1], visibleCorners > 0 ? Color.green : Color.red, 1.0f);
        }

        return visibleCorners;
    }
}

public class ChatEntry : MonoBehaviour
{
    public struct Model
    {
        public enum SubType
        {
            NONE,
            PRIVATE_FROM,
            PRIVATE_TO
        }

        public ChatMessage.Type messageType;
        public string bodyText;
        public string senderName;
        public string recipientName;
        public SubType subType;

        public ulong timestamp;
    }

    [SerializeField] internal TextMeshProUGUI username;
    [SerializeField] internal TextMeshProUGUI body;

    [SerializeField] internal Color worldMessageColor = Color.white;
    [SerializeField] internal Color privateMessageColor = Color.white;
    [SerializeField] internal Color systemColor = Color.white;

    public Model model { get; private set; }

    public void Populate(Model chatEntryModel)
    {
        this.model = chatEntryModel;

        string userString = GetDefaultSenderString(chatEntryModel.senderName);

        if (chatEntryModel.subType == Model.SubType.PRIVATE_FROM)
        {
            userString = $"<b>[From {chatEntryModel.senderName}]:</b>";
        }
        else
        if (chatEntryModel.subType == Model.SubType.PRIVATE_TO)
        {
            userString = $"<b>[To {chatEntryModel.recipientName}]:</b>";
        }

        switch (chatEntryModel.messageType)
        {
            case ChatMessage.Type.PUBLIC:
                this.body.color = username.color = worldMessageColor;
                break;
            case ChatMessage.Type.PRIVATE:
                this.body.color = username.color = privateMessageColor;
                break;
            case ChatMessage.Type.SYSTEM:
                this.body.color = username.color = systemColor;
                break;
        }

        chatEntryModel.bodyText = RemoveTabs(chatEntryModel.bodyText);

        username.text = userString;
        body.text = $"{userString} {chatEntryModel.bodyText}";

        LayoutRebuilder.ForceRebuildLayoutImmediate(body.transform as RectTransform);
        LayoutRebuilder.ForceRebuildLayoutImmediate(username.transform as RectTransform);

        if (this.enabled)
            group.alpha = 0;
    }

    [SerializeField] CanvasGroup group;

    const float TIME_TO_FADE = 10;
    const float FADE_DURATION = 5;

    public void SetFadeout(bool enabled)
    {
        if (!enabled)
        {
            group.alpha = 1;
            this.enabled = false;
            return;
        }

        this.enabled = true;
    }

    private void Update()
    {
        double fadeTime = (double)(model.timestamp / 1000.0) + TIME_TO_FADE;
        double currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0;

        if (currentTime > fadeTime)
        {
            double timeSinceFadeTime = currentTime - fadeTime;
            group.alpha = Mathf.Clamp01(1 - (float)(timeSinceFadeTime / FADE_DURATION));
        }
        else
        {
            group.alpha += (1 - group.alpha) * 0.05f;
        }
    }

    string RemoveTabs(string text)
    {
        //NOTE(Brian): ContentSizeFitter doesn't fare well with tabs, so i'm replacing these
        //             with spaces.
        return text.Replace("\t", "    ");
    }

    string GetDefaultSenderString(string sender)
    {
        if (!string.IsNullOrEmpty(sender))
            return $"<b>{sender}:</b>";

        return "";
    }

}
