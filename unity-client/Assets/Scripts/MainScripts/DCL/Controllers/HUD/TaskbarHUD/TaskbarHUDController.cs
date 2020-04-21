using UnityEngine;

public class TaskbarHUDController : IHUD
{
    internal TaskbarHUDView view;
    WorldChatWindowHUDController worldChatWindowHud;

    public TaskbarHUDController()
    {
        view = TaskbarHUDView.Create(this);
    }

    public void AddChatWindow(WorldChatWindowHUDController controller)
    {
        if (controller == null || controller.view == null)
        {
            Debug.LogWarning("AddChatWindow >>> World Chat Window doesn't exist yet!");
            return;
        }

        if (controller.view.transform.parent == view.windowContainer)
            return;

        controller.view.transform.SetParent(view.windowContainer, false);
        worldChatWindowHud = controller;
        view.OnAddChatWindow(ToggleChatWindow);
    }

    private void ToggleChatWindow()
    {
        worldChatWindowHud.SetVisibility(!worldChatWindowHud.view.gameObject.activeSelf);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(view.gameObject);
    }

    public void SetVisibility(bool visible)
    {
        view.SetVisibility(visible);
    }
}
