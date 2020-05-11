using DCL;
using DCL.Interface;
using System.Collections;
using UnityEngine;

public class PrivateChatWindowHUDController : IHUD
{
    private ChatHUDController chatHudController;
    public PrivateChatWindowHUDView view;

    private IChatController chatController;

    internal bool resetInputFieldOnSubmit = true;

    public void Initialize(IChatController chatController, IMouseCatcher mouseCatcher)
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

    public void SendChatMessage(string msgBody)
    {
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
            body = msgBody,
            sender = UserProfile.GetOwnUserProfile().userId,
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
