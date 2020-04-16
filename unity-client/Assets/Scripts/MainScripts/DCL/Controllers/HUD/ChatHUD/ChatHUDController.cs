using System.Collections.Generic;
using System.Linq;
using ChatMessage = ChatController.ChatMessage;
using ChatMessageType = ChatController.ChatMessageType;
public class ChatHUDController
{
    const int MAX_CHAT_ENTRIES = 100;

    public ChatHUDView view;

    public ChatHUDController()
    {
        ChatController.i.OnAddMessage -= AddChatMessage;
        ChatController.i.OnAddMessage += AddChatMessage;
    }

    public void AddChatMessage(ChatMessage message)
    {
        if (view.entries.Count > MAX_CHAT_ENTRIES)
        {
            var result = TrimAndSortChatMessages(view.entries.Select((x) => x.message).ToList());
            view.RepopulateAllChatMessages(result);
        }

        view.AddEntry(message);
    }

    public void FilterByType(ChatMessageType type)
    {
        var result = ChatController.i.entries.Where((x) => x.messageType == type).ToList();
        result = TrimAndSortChatMessages(result);
        view.RepopulateAllChatMessages(result);
    }


    public List<ChatMessage> TrimAndSortChatMessages(List<ChatMessage> messages)
    {
        var result = messages;

        result.Sort((x, y) => { return x.timestamp > y.timestamp ? 1 : -1; });

        if (result.Count > MAX_CHAT_ENTRIES)
        {
            int entriesToRemove = MAX_CHAT_ENTRIES - result.Count;
            result.RemoveRange(0, entriesToRemove);
        }

        return result;
    }
}
