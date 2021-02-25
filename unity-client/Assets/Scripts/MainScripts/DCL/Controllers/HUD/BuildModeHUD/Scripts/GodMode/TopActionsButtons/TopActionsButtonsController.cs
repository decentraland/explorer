public interface ITopActionsButtonsController
{
    void Initialize(TopActionsButtonsView topActionsButtonsView);
    void Dispose();
}

public class TopActionsButtonsController : ITopActionsButtonsController
{
    private TopActionsButtonsView topActionsButtonsView;

    public void Initialize(TopActionsButtonsView topActionsButtonsView)
    {
        this.topActionsButtonsView = topActionsButtonsView;
    }

    public void Dispose()
    {
        
    }
}
