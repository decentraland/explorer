
using UnityEngine;

public class WorldChatWindowHUDController : IHUD
{
    ChatHUDController chatHudController;
    public WorldChatWindowHUDView view;
    public WorldChatWindowHUDController()
    {
        chatHudController = new ChatHUDController();

        view = WorldChatWindowHUDView.Create();
        view.chatHudView.Initialize(chatHudController);

        ChatController.i.OnAddMessage -= view.chatHudView.controller.AddChatMessage;
        ChatController.i.OnAddMessage += view.chatHudView.controller.AddChatMessage;
    }
    public void Dispose()
    {
        ChatController.i.OnAddMessage -= view.chatHudView.controller.AddChatMessage;
        Object.Destroy(view);
    }

    public void SetVisibility(bool visible)
    {
        view.gameObject.SetActive(visible);
    }
}
