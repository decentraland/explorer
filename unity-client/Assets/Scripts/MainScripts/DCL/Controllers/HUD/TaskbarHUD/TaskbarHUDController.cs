public class TaskbarHUDController : IHUD
{
    TaskbarHUDView view;

    public TaskbarHUDController()
    {
        view = TaskbarHUDView.Create(this);
    }

    public void AddFriendsWindow()
    {
        // stub
    }

    public void AddChatWindow(WorldChatWindowHUDController controller)
    {

    }

    public void Dispose()
    {
    }

    public void SetVisibility(bool visible)
    {
        view.SetVisibility(visible);
    }
}
