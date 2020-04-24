using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatEntry : MonoBehaviour
{
    [SerializeField] internal TextMeshProUGUI username;
    [SerializeField] internal TextMeshProUGUI body;

    public Color worldMessageColor = Color.white;
    public Color privateMessageColor = Color.white;
    public Color systemColor = Color.white;

    public ChatController.ChatMessage message;

    public enum MessageSubType
    {
        NONE,
        PRIVATE_TO,
        PRIVATE_FROM,
    }

    public void Populate(ChatController.ChatMessage chatMessage, MessageSubType subType = MessageSubType.NONE)
    {
        if (chatMessage == null)
            return;

        this.message = chatMessage;
        string userString = "";

        if (!string.IsNullOrEmpty(chatMessage.sender))
            userString = $"<b>{chatMessage.sender}:</b>";

        switch (chatMessage.messageType)
        {
            case ChatController.ChatMessageType.NONE:
                break;
            case ChatController.ChatMessageType.PUBLIC:
                body.color = username.color = worldMessageColor;
                break;
            case ChatController.ChatMessageType.PRIVATE:
                body.color = username.color = privateMessageColor;

                if (subType == MessageSubType.PRIVATE_FROM)
                {
                    userString = $"<b>[From {chatMessage.sender}]:</b>";
                }
                else
                if (subType == MessageSubType.PRIVATE_TO)
                {
                    userString = $"<b>[To {chatMessage.recipient}]:</b>";
                }

                break;
            case ChatController.ChatMessageType.SYSTEM:
                body.color = username.color = systemColor;
                break;
        }

        //NOTE(Brian): ContentSizeFitter doesn't fare well with tabs, so i'm replacing these
        //             with spaces.
        chatMessage.body = chatMessage.body.Replace("\t", "    ");

        username.text = userString;
        body.text = $"{userString} {chatMessage.body}";

        LayoutRebuilder.ForceRebuildLayoutImmediate(body.transform as RectTransform);
        LayoutRebuilder.ForceRebuildLayoutImmediate(username.transform as RectTransform);
    }
}
