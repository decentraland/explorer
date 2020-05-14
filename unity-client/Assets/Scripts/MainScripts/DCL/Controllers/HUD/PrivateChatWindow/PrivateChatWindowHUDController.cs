using DCL;
using DCL.Interface;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;

public class PrivateChatWindowHUDController : IHUD
{
    internal const string PLAYER_PREFS_LAST_READ_CHAT_MESSAGES = "LastReadChatMessages";

    public PrivateChatWindowHUDView view;
    public bool resetInputFieldOnSubmit = true;

    ChatHUDController chatHudController;
    IChatController chatController;
    string conversationUserId = string.Empty;
    string conversationUserName = string.Empty;

    public void Initialize(IChatController chatController)
    {
        view = PrivateChatWindowHUDView.Create();

        view.chatHudView.inputField.onSelect.RemoveListener(ChatHUDViewInputField_OnSelect);
        view.chatHudView.inputField.onSelect.AddListener(ChatHUDViewInputField_OnSelect);

        chatHudController = new ChatHUDController();
        chatHudController.Initialize(view.chatHudView, SendChatMessage);
        LoadLatestReadChatMessagesStatus();

        this.chatController = chatController;

        if (chatController != null)
        {
            chatController.OnAddMessage -= OnAddMessage;
            chatController.OnAddMessage += OnAddMessage;
        }

        view.chatHudView.ForceUpdateLayout();

        SetVisibility(false);
    }

    public void Configure(string newConversationUserId)
    {
        if (string.IsNullOrEmpty(newConversationUserId) || newConversationUserId == conversationUserId) return;

        conversationUserId = newConversationUserId;
        conversationUserName = UserProfileController.userProfilesCatalog.Get(newConversationUserId).userName;

        view.ConfigureTitle(conversationUserName);

        view.chatHudView.CleanAllEntries();

        var messageEntries = chatController.GetEntries().Where((x) => IsMessageFomCurrentConversation(x)).ToList();
        foreach (var v in messageEntries)
        {
            OnAddMessage(v);
        }
    }

    public void SendChatMessage(string msgBody)
    {
        if (string.IsNullOrEmpty(conversationUserName)) return;

        bool validString = !string.IsNullOrEmpty(msgBody);

        if (msgBody.Length == 1 && (byte)msgBody[0] == 11) //NOTE(Brian): Trim doesn't work. neither IsNullOrWhitespace.
            validString = false;

        if (!validString)
        {
            InitialSceneReferences.i.mouseCatcher.LockCursor();
            return;
        }

        if (resetInputFieldOnSubmit)
        {
            view.chatHudView.ResetInputField();
            view.chatHudView.FocusInputField();
        }

        var data = new ChatMessage()
        {
            body = $"/w {conversationUserName} " + msgBody,
            sender = UserProfile.GetOwnUserProfile().userId,
            messageType = ChatMessage.Type.PRIVATE
        };

        WebInterface.SendChatMessage(data);
    }

    public void SetVisibility(bool visible)
    {
        if (view.gameObject.activeSelf == visible) return;

        view.gameObject.SetActive(visible);

        if (visible)
        {
            // The messages from 'conversationUserId' are marked as read once the private chat is opened
            MarkUserChatMessagesAsRead(conversationUserId);
        }
    }

    public void Dispose()
    {
        view.chatHudView.inputField.onSelect.RemoveListener(ChatHUDViewInputField_OnSelect);

        if (chatController != null)
            chatController.OnAddMessage -= OnAddMessage;

        UnityEngine.Object.Destroy(view);
    }

    void OnAddMessage(ChatMessage message)
    {
        if (!IsMessageFomCurrentConversation(message)) return;

        view.chatHudView.controller.AddChatMessage(ChatHUDController.ChatMessageToChatEntry(message));

        if (view.chatHudView.inputField.isFocused)
        {
            // The messages from 'conversationUserId' are marked as read if the player was already focused on the input field of the private chat
            MarkUserChatMessagesAsRead(conversationUserId);
        }
    }

    bool IsMessageFomCurrentConversation(ChatMessage message)
    {
        return message.messageType == ChatMessage.Type.PRIVATE && (message.sender == conversationUserId || message.recipient == conversationUserId);
    }

    public void ForceFocus()
    {
        SetVisibility(true);
        view.chatHudView.FocusInputField();
    }

    private void MarkUserChatMessagesAsRead(string userId)
    {
        CommonScriptableObjects.lastReadChatMessages.Remove(userId);
        CommonScriptableObjects.lastReadChatMessages.Add(userId, System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        SaveLatestReadChatMessagesStatus();
    }

    private void SaveLatestReadChatMessagesStatus()
    {
        List<KeyValuePair<string, long>> lastReadChatMessagesList = new List<KeyValuePair<string, long>>();
        using (var iterator = CommonScriptableObjects.lastReadChatMessages.GetEnumerator())
        {
            while (iterator.MoveNext())
            {
                lastReadChatMessagesList.Add(new KeyValuePair<string, long>(iterator.Current.Key, iterator.Current.Value));
            }
        }

        PlayerPrefs.SetString(PLAYER_PREFS_LAST_READ_CHAT_MESSAGES, JsonConvert.SerializeObject(lastReadChatMessagesList));
        PlayerPrefs.Save();
    }

    private void LoadLatestReadChatMessagesStatus()
    {
        CommonScriptableObjects.lastReadChatMessages.Clear();

        List<KeyValuePair<string, long>> lastReadChatMessagesList = JsonConvert.DeserializeObject<List<KeyValuePair<string, long>>>(PlayerPrefs.GetString(PLAYER_PREFS_LAST_READ_CHAT_MESSAGES));
        if (lastReadChatMessagesList != null)
        {
            foreach (var item in lastReadChatMessagesList)
            {
                CommonScriptableObjects.lastReadChatMessages.Add(item.Key, item.Value);
            }
        }
    }

    private void ChatHUDViewInputField_OnSelect(string message)
    {
        // The messages from 'conversationUserId' are marked as read if the player clicks on the input field of the private chat
        MarkUserChatMessagesAsRead(conversationUserId);
    }
}
