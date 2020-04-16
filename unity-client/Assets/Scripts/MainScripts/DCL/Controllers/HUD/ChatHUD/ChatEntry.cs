using TMPro;
using UnityEngine;

public class ChatEntry : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI username;
    [SerializeField] private TextMeshProUGUI body;

    public Color worldMessageColor = Color.white;
    public Color privateMessageColor = Color.white;
    public Color systemColor = Color.white;

    public void Populate(ChatHUDController.ChatMessage chatMessage)
    {
        string userString = "";

        if (!string.IsNullOrEmpty(chatMessage.sender))
            userString = $"<b>{chatMessage.sender}:</b>";

        switch (chatMessage.messageType)
        {
            case ChatHUDController.ChatMessageType.NONE:
                break;
            case ChatHUDController.ChatMessageType.PUBLIC:
                body.color = username.color = worldMessageColor;
                break;
            case ChatHUDController.ChatMessageType.PRIVATE:
                body.color = username.color = privateMessageColor;
                break;
            case ChatHUDController.ChatMessageType.SYSTEM:
                body.color = username.color = systemColor;
                break;
        }

        username.text = userString;
        body.text = $"{userString} {chatMessage.body}";
    }
}
