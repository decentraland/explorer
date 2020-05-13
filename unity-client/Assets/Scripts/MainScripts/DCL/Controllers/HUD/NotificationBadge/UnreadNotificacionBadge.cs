using DCL.Interface;
using System.Linq;
using TMPro;
using UnityEngine;

public class UnreadNotificacionBadge : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI notificationText;
    [SerializeField] private GameObject notificationContainer;

    private string userId;
    private long currentTimestampReading;

    private int currentUnreadMessages;
    public int CurrentUnreadMessages
    {
        get => currentUnreadMessages;
        set
        {
            currentUnreadMessages = value;
            RefreshNotificationBadge();
        }
    }

    public void Initialize(string userName)
    {
        userId = userName;
        CommonScriptableObjects.lastReadChatMessages.TryGetValue(userId, out currentTimestampReading);
        CountUnreadMessages();

        ChatController.i.OnAddMessage -= ChatController_OnAddMessage;
        ChatController.i.OnAddMessage += ChatController_OnAddMessage;

        CommonScriptableObjects.lastReadChatMessages.OnAdded -= LastReadChatMessages_OnAdded;
        CommonScriptableObjects.lastReadChatMessages.OnAdded += LastReadChatMessages_OnAdded;
    }

    private void OnDestroy()
    {
        ChatController.i.OnAddMessage -= ChatController_OnAddMessage;
        CommonScriptableObjects.lastReadChatMessages.OnAdded -= LastReadChatMessages_OnAdded;
    }

    private void ChatController_OnAddMessage(ChatMessage newMessage)
    {
        if (newMessage.messageType == ChatMessage.Type.PRIVATE &&
            newMessage.sender == userId)
        {
            // A new message from [userId] is received
            CountUnreadMessages();
        }
    }

    private void LastReadChatMessages_OnAdded(string addedKey, long addedValue)
    {
        if (addedKey == userId)
        {
            // The player reads the latest messages of [userId]
            currentTimestampReading = addedValue;
            CurrentUnreadMessages = 0;
        }
    }

    private void CountUnreadMessages()
    {
        CurrentUnreadMessages = ChatController.i.entries.Count(
            msg => msg.messageType == ChatMessage.Type.PRIVATE &&
            msg.sender == userId &&
            msg.timestamp > (ulong)currentTimestampReading);
    }

    private void RefreshNotificationBadge()
    {
        if (currentUnreadMessages > 0)
        {
            notificationContainer.SetActive(true);
            notificationText.text = currentUnreadMessages.ToString();
        }
        else
        {
            notificationContainer.SetActive(false);
        }
    }
}
