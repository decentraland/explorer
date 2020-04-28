
using System;
using System.Collections.Generic;
using UnityEngine.Events;
using ChatMessage = ChatController.ChatMessage;
public class ChatHUDController : IDisposable
{
    public const int MAX_CHAT_ENTRIES = 100;

    public ChatHUDView view;

    public UnityAction<string> OnSendMessage;

    public void Initialize(ChatHUDView view = null, UnityAction<string> onSendMessage = null)
    {
        if (view == null)
        {
            this.view = ChatHUDView.Create();
        }
        else
        {
            this.view = view;
        }

        this.view.Initialize(this, onSendMessage);
    }

    public void AddChatMessage(ChatEntry.Model chatEntryModel)
    {
        view.AddEntry(chatEntryModel);

        if (view.entries.Count > MAX_CHAT_ENTRIES)
        {
            UnityEngine.Object.Destroy(view.entries[0].gameObject);
            view.entries.Remove(view.entries[0]);
        }
    }

    public List<ChatMessage> TrimAndSortChatMessages(List<ChatMessage> messages)
    {
        var result = messages;

        result.Sort((x, y) => { return x.timestamp > y.timestamp ? 1 : -1; });

        if (result.Count > MAX_CHAT_ENTRIES)
        {
            int entriesToRemove = (result.Count - MAX_CHAT_ENTRIES);
            result.RemoveRange(0, entriesToRemove);
        }

        return result;
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(this.view.gameObject);
    }

    public static string ConstructGuestName(string id)
    {
        if (string.IsNullOrEmpty(id))
            return "Guest-unknown";

        return "Guest-" + id.Substring(0, 5);
    }

    public static ChatEntry.Model ChatMessageToChatEntry(ChatMessage message)
    {
        ChatEntry.Model model = new ChatEntry.Model();

        var ownProfile = UserProfile.GetOwnUserProfile();

        model.messageType = message.messageType;
        model.bodyText = message.body;

        if (message.recipient != null)
        {
            var recipientProfile = UserProfileController.userProfilesCatalog.Get(message.recipient);
            model.recipientName = recipientProfile != null ? recipientProfile.userName : ConstructGuestName(message.recipient);
        }

        if (message.sender != null)
        {
            var senderProfile = UserProfileController.userProfilesCatalog.Get(message.sender);
            model.senderName = senderProfile != null ? senderProfile.userName : ConstructGuestName(message.sender);
        }

        if (model.messageType == ChatController.ChatMessageType.PRIVATE)
        {
            if (message.recipient == ownProfile.userId)
            {
                model.subType = ChatEntry.Model.SubType.PRIVATE_FROM;
            }
            else if (message.sender == ownProfile.userId)
            {
                model.subType = ChatEntry.Model.SubType.PRIVATE_TO;
            }
            else
            {
                model.subType = ChatEntry.Model.SubType.NONE;
            }
        }

        return model;
    }
}
