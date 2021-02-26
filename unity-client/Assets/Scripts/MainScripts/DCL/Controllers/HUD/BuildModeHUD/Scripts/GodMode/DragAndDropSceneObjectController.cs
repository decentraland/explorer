public interface IDragAndDropSceneObjectController
{
    event System.Action OnDrop;

    void Initialize(DragAndDropSceneObjectView dragAndDropSceneObjectView);
    void Dispose();
}

public class DragAndDropSceneObjectController : IDragAndDropSceneObjectController
{
    public event System.Action OnDrop;

    private DragAndDropSceneObjectView dragAndDropSceneObjectView;

    public void Initialize(DragAndDropSceneObjectView dragAndDropSceneObjectView)
    {
        this.dragAndDropSceneObjectView = dragAndDropSceneObjectView;

        dragAndDropSceneObjectView.OnDrop += Drop;
    }

    public void Dispose()
    {
        dragAndDropSceneObjectView.OnDrop -= Drop;
    }

    private void Drop()
    {
        OnDrop?.Invoke();
    }
}
