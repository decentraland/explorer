using UnityEngine;

public class TaskbarHUDController : IHUD
{
    TaskbarHUDView view;
    WorldChatWindowHUDController worldChatWindowHud;

    public TaskbarHUDController()
    {
        view = TaskbarHUDView.Create(this);
    }

    public void ToggleChatWindow()
    {
        worldChatWindowHud.SetVisibility(!worldChatWindowHud.view.gameObject.activeSelf);
    }

    public void AddChatWindow(WorldChatWindowHUDController controller)
    {
        controller.view.transform.SetParent(view.windowContainer);
        worldChatWindowHud = controller;
        view.OnAddChatWindow();
    }

    public void Dispose()
    {
        Object.Destroy(view.gameObject);
    }

    public void SetVisibility(bool visible)
    {
        view.SetVisibility(visible);
    }
}
