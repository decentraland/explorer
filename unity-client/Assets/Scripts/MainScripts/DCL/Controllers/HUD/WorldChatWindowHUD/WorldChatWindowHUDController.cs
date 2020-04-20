
using DCL.Interface;
using UnityEngine;

public class WorldChatWindowHUDController : IHUD
{
    ChatHUDController chatHudController;
    public WorldChatWindowHUDView view;

    string userName;

    public WorldChatWindowHUDController()
    {
        chatHudController = new ChatHUDController();

        view = WorldChatWindowHUDView.Create();
        view.chatHudView.Initialize(chatHudController, SendChatMessage);
        chatHudController.view = view.chatHudView;

        ChatController.i.OnAddMessage -= view.chatHudView.controller.AddChatMessage;
        ChatController.i.OnAddMessage += view.chatHudView.controller.AddChatMessage;

        userName = "NO_USER";

        var profileUserName = UserProfile.GetOwnUserProfile().userName;

        if (!string.IsNullOrEmpty(profileUserName))
            userName = profileUserName;
    }
    public void Dispose()
    {
        ChatController.i.OnAddMessage -= view.chatHudView.controller.AddChatMessage;
        Object.Destroy(view);
    }

    //NOTE(Brian): Send chat responsibilities must be on the chatHud containing window like this one, this way we ensure
    //             it can be reused by the private messaging windows down the road.
    public void SendChatMessage(string msgBody)
    {
        if (string.IsNullOrEmpty(msgBody))
            return;

        view.chatHudView.ResetInputField();
        view.chatHudView.FocusInputField();

        var data = new ChatController.ChatMessage()
        {
            body = msgBody,
            sender = userName,
        };

        WebInterface.SendChatMessage(data);
    }

    public void SetVisibility(bool visible)
    {
        view.gameObject.SetActive(visible);
    }
}
