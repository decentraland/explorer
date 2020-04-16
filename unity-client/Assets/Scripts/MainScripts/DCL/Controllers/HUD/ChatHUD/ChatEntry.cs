using TMPro;
using UnityEngine;

public class ChatEntry : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI username;
    [SerializeField] private TextMeshProUGUI body;

    public Color worldMessageColor = Color.white;
    public Color privateMessageColor = Color.white;
    public Color systemColor = Color.white;

    public ChatController.ChatMessage message;

    public void Populate(ChatController.ChatMessage chatMessage)
    {
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
                break;
            case ChatController.ChatMessageType.SYSTEM:
                body.color = username.color = systemColor;
                break;
        }

        username.text = userString;
        body.text = $"{userString} {chatMessage.body}";
    }
}
