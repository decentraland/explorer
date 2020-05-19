using DCL.Helpers;
using DCL.Interface;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ChatEntry : MonoBehaviour, IPointerClickHandler
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
        public string otherUserId;
        public ulong timestamp;

        public SubType subType;
    }

    [SerializeField] internal float timeToFade = 10;
    [SerializeField] internal float fadeDuration = 5;

    [SerializeField] internal TextMeshProUGUI username;
    [SerializeField] internal TextMeshProUGUI body;

    [SerializeField] internal Color worldMessageColor = Color.white;
    [SerializeField] internal Color privateMessageColor = Color.white;
    [SerializeField] internal Color systemColor = Color.white;
    [SerializeField] CanvasGroup group;

    bool fadeEnabled = false;
    double fadeoutStartTime;

    public Model model { get; private set; }

    public event UnityAction<string> OnPress;

    public void Populate(Model chatEntryModel)
    {
        this.model = chatEntryModel;

        string userString = GetDefaultSenderString(chatEntryModel.senderName);

        if (chatEntryModel.subType == Model.SubType.PRIVATE_FROM)
        {
            userString = $"<b>From {chatEntryModel.senderName}:</b>";
        }
        else
        if (chatEntryModel.subType == Model.SubType.PRIVATE_TO)
        {
            userString = $"<b>To {chatEntryModel.recipientName}:</b>";
        }

        switch (chatEntryModel.messageType)
        {
            case ChatMessage.Type.PUBLIC:
                body.color = worldMessageColor;

                if (username != null)
                    username.color = worldMessageColor;
                break;
            case ChatMessage.Type.PRIVATE:
                body.color = worldMessageColor;

                if (username != null)
                    username.color = privateMessageColor;
                break;
            case ChatMessage.Type.SYSTEM:
                body.color = systemColor;

                if (username != null)
                    username.color = systemColor;
                break;
        }

        chatEntryModel.bodyText = RemoveTabs(chatEntryModel.bodyText);

        if (username != null)
        {
            username.text = userString;
            body.text = $"{userString} {chatEntryModel.bodyText}";
        }
        else
        {
            body.text = $"{chatEntryModel.bodyText}";
        }

        Utils.ForceUpdateLayout(transform as RectTransform);

        if (fadeEnabled)
            group.alpha = 0;
    }

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        if (model.messageType != ChatMessage.Type.PRIVATE) return;

        OnPress?.Invoke(model.otherUserId);
    }

    public void SetFadeout(bool enabled)
    {
        if (!enabled)
        {
            group.alpha = 1;
            fadeEnabled = false;
            return;
        }

        fadeoutStartTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0;
        fadeEnabled = true;
    }

    private void Update()
    {
        if (!fadeEnabled) return;

        //NOTE(Brian): Small offset using normalized Y so we keep the cascade effect
        double yOffset = (transform as RectTransform).anchoredPosition.y / (double)Screen.height * 2.0;

        double fadeTime = Math.Max(model.timestamp / 1000.0, fadeoutStartTime) + timeToFade - yOffset;
        double currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0;

        if (currentTime > fadeTime)
        {
            double timeSinceFadeTime = currentTime - fadeTime;
            group.alpha = Mathf.Clamp01(1 - (float)(timeSinceFadeTime / fadeDuration));
        }
        else
        {
            group.alpha += (1 - group.alpha) * 0.05f;
        }
    }

    string RemoveTabs(string text)
    {
        if (string.IsNullOrEmpty(text))
            return "";

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
