﻿using DCL;
using DCL.Interface;
using System.Collections;
using UnityEngine;
using System.Linq;

public class PrivateChatWindowHUDController : IHUD
{
    public PrivateChatWindowHUDView view;

    ChatHUDController chatHudController;
    IChatController chatController;
    bool resetInputFieldOnSubmit = true;
    string conversationUserId = string.Empty;
    string conversationUserName = string.Empty;

    public void Initialize(IChatController chatController)
    {
        view = PrivateChatWindowHUDView.Create();

        chatHudController = new ChatHUDController();
        chatHudController.Initialize(view.chatHudView, SendChatMessage);

        this.chatController = chatController;

        if (chatController != null)
        {
            chatController.OnAddMessage -= OnAddMessage;
            chatController.OnAddMessage += OnAddMessage;
        }

        view.chatHudView.ForceUpdateLayout();
    }

    public void Configure(string newConversationUserId)
    {
        if (string.IsNullOrEmpty(newConversationUserId) || newConversationUserId == conversationUserId) return;

        conversationUserId = newConversationUserId;
        conversationUserName = UserProfileController.userProfilesCatalog.Get(newConversationUserId).userName;

        view.chatHudView.CleanAllEntries();

        var messageEntries = chatController.GetEntries().Where((x) => x.messageType == ChatMessage.Type.PRIVATE && IsMessageFomCurrentConversation(x)).ToList();
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
        view.gameObject.SetActive(visible);

        if (visible)
        {
            view.StartCoroutine(ForceLayoutDelayed());
        }
    }

    public void Dispose()
    {
        if (chatController != null)
            chatController.OnAddMessage -= OnAddMessage;

        Object.Destroy(view);
    }

    void OnAddMessage(ChatMessage message)
    {
        if (!IsMessageFomCurrentConversation(message)) return;

        view.chatHudView.controller.AddChatMessage(ChatHUDController.ChatMessageToChatEntry(message));
    }

    bool IsMessageFomCurrentConversation(ChatMessage message)
    {
        return message.sender == conversationUserId || message.recipient == conversationUserId;
    }

    IEnumerator ForceLayoutDelayed()
    {
        yield return null;
        view.chatHudView.ForceUpdateLayout();
    }
}
