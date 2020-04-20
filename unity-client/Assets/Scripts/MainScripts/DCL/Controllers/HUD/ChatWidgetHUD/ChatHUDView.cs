using DCL.Helpers;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ChatMessage = ChatController.ChatMessage;
using ChatMessageType = ChatController.ChatMessageType;


public class ChatHUDView : MonoBehaviour
{
    public TMP_InputField inputField;
    public RectTransform chatEntriesContainer;

    public ScrollRect scrollRect;
    GameObject chatEntryPrefab;
    public ChatHUDController controller;
    [NonSerialized] public List<ChatEntry> entries = new List<ChatEntry>();

    string userName;

    public void Initialize(ChatHUDController controller)
    {
        this.controller = controller;

        chatEntryPrefab = Resources.Load("Chat Entry") as GameObject;
        inputField.onSubmit.AddListener(SendChatMessage);

        userName = "NO_USER";

        var profileUserName = UserProfile.GetOwnUserProfile().userName;

        if (!string.IsNullOrEmpty(profileUserName))
            userName = profileUserName;
    }

    void SendChatMessage(string msgBody)
    {
        if (string.IsNullOrEmpty(msgBody))
            return;

        inputField.text = "";
        inputField.caretColor = Color.white;
        inputField.Select();
        inputField.ActivateInputField();

        var data = new ChatMessage()
        {
            body = msgBody,
            sender = userName,
            messageType = ChatMessageType.PUBLIC
        };

        controller.SendChatMessage(data);
        controller.AddChatMessage(data);
    }

    public void AddEntry(ChatMessage message)
    {
        ChatEntry chatEntry = Instantiate(chatEntryPrefab, chatEntriesContainer).GetComponent<ChatEntry>();
        chatEntry.Populate(message);
        entries.Add(chatEntry);

        Utils.InverseTransformChildTraversal<RectTransform>(
        (x) =>
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(x);
        },
        chatEntriesContainer);
    }

    public void CleanAllEntries()
    {
        foreach (var entry in entries)
        {
            Destroy(entry.gameObject);
        }

        entries.Clear();
    }

    public void RepopulateAllChatMessages(List<ChatMessage> entriesList)
    {
        CleanAllEntries();

        int entriesCount = entriesList.Count;

        for (int i = 0; i < entriesCount; i++)
        {
            AddEntry(entriesList[i]);
        }
    }

}
