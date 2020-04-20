using System;
using System.Collections.Generic;
using UnityEngine;

public class ChatController : MonoBehaviour
{
    public static ChatController i { get; private set; }

    public void Awake()
    {
        i = this;
    }

    public enum ChatMessageType
    {
        NONE,
        PUBLIC,
        PRIVATE,
        SYSTEM
    }

    [System.Serializable]
    public class ChatMessage
    {
        public ChatMessageType messageType;
        public string sender;
        public string recipient;
        public ulong timestamp;
        public string body;
    }

    [NonSerialized] public List<ChatMessage> entries = new List<ChatMessage>();

    public event Action<ChatMessage> OnAddMessage;

    public void AddMessageToChatWindow(string jsonMessage)
    {
        ChatMessage message = JsonUtility.FromJson<ChatMessage>(jsonMessage);

        if (message == null)
            return;

        entries.Add(message);
        OnAddMessage?.Invoke(message);
    }
}
