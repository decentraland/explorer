using DCL;
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

    public void Initialize(IChatController chatController) // TODO: Try removing the chatController reference and just use the singleton one
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

    public void Configure(string userId)
    {
        if (string.IsNullOrEmpty(userId)) return;

        conversationUserId = userId;
        conversationUserName = UserProfileController.userProfilesCatalog.Get(userId).userName;

        view.chatHudView.CleanAllEntries();

        var messageEntries = chatController.GetEntries().Where((x) => x.messageType == ChatMessage.Type.PRIVATE && (x.sender == conversationUserId || x.recipient == conversationUserId)).ToList();
        foreach (var v in messageEntries)
        {
            OnAddMessage(v);
        }

        // TODO: hook up to the "new message" event to show the new messages as they arrive
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
        view.chatHudView.controller.AddChatMessage(ChatHUDController.ChatMessageToChatEntry(message));
    }

    IEnumerator ForceLayoutDelayed()
    {
        yield return null;
        view.chatHudView.ForceUpdateLayout();
    }
}
