using DCL.Helpers;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatHUDView : MonoBehaviour
{
    const string VIEW_PATH = "ChatHUDView";

    public TMP_InputField inputField;
    public RectTransform chatEntriesContainer;

    GameObject chatEntryPrefab;
    ChatHUDController controller;
    [NonSerialized] public List<ChatEntry> entries = new List<ChatEntry>();

    string userName;

    private void Initialize(ChatHUDController controller)
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

        var data = new ChatHUDController.ChatMessage()
        {
            body = msgBody,
            sender = userName,
            messageType = ChatHUDController.ChatMessageType.PUBLIC
        };

        controller.AddChatMessage(data);
    }

    public void AddEntry(ChatHUDController.ChatMessage message)
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
        var entries = chatEntriesContainer.GetComponentsInChildren<ChatEntry>();

        foreach (var entry in entries)
        {
            Destroy(entry.gameObject);
        }
    }
}
