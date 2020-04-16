using System.Collections.Generic;

public class ChatHUDController
{
    const int MAX_CHAT_ENTRIES = 100;

    public enum ChatMessageType
    {
        NONE,
        PUBLIC,
        PRIVATE,
        SYSTEM
    }

    public class ChatMessage
    {
        public ChatMessageType messageType;
        public string sender;
        public string recipient;
        public ulong timestamp;
        public string body;
    }

    public ChatHUDView view;

    List<ChatMessage> entries = new List<ChatMessage>();

    public void AddChatMessage(ChatMessage message)
    {
        if (view.entries.Count > MAX_CHAT_ENTRIES)
        {
            TrimAndSortChatMessages();
        }

        entries.Add(message);
        view.AddEntry(message);
    }


    public List<ChatMessage> TrimAndSortChatMessages()
    {
        var result = new List<ChatMessage>(entries);

        result.Sort((x, y) => { return x.timestamp > y.timestamp ? 1 : -1; });

        if (result.Count > MAX_CHAT_ENTRIES)
        {
            int entriesToRemove = MAX_CHAT_ENTRIES - entries.Count;
            result.RemoveRange(0, entriesToRemove);
        }

        RepopulateAllChatMessages(result);
        return result;
    }

    public void RepopulateAllChatMessages(List<ChatMessage> entriesList)
    {
        view.CleanAllEntries();

        int entriesCount = entriesList.Count;

        for (int i = 0; i < entriesCount; i++)
        {
            entries.Add(entriesList[i]);
            view.AddEntry(entriesList[i]);
        }
    }

}
