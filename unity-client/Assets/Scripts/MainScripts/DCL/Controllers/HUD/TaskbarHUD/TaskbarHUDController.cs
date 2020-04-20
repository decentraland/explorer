public class TaskbarHUDController : IHUD
{
    TaskbarHUDView view;
    WorldChatWindowHUDController worldChatWindowHud;

    public TaskbarHUDController()
    {
        view = TaskbarHUDView.Create(this);
    }

    public void AddChatWindow(WorldChatWindowHUDController controller)
    {
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
