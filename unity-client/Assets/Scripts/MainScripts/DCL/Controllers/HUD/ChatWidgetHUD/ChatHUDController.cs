
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

    public void AddChatMessage(ChatMessage message, ChatEntry.MessageSubType subType = ChatEntry.MessageSubType.NONE)
    {
        view.AddEntry(message, subType);

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
}
