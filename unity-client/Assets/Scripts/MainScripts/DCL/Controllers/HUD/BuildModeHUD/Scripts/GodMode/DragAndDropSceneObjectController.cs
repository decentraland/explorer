public interface IDragAndDropSceneObjectController
{
    void Initialize(DragAndDropSceneObjectView dragAndDropSceneObjectView, BuildModeHUDController buildModeHUDController);
    void Dispose();
}

public class DragAndDropSceneObjectController : IDragAndDropSceneObjectController
{
    private DragAndDropSceneObjectView dragAndDropSceneObjectView;
    private BuildModeHUDController buildModeHUDController;

    public void Initialize(DragAndDropSceneObjectView dragAndDropSceneObjectView, BuildModeHUDController buildModeHUDController)
    {
        this.dragAndDropSceneObjectView = dragAndDropSceneObjectView;
        this.buildModeHUDController = buildModeHUDController;

        dragAndDropSceneObjectView.OnDrop += Drop;
    }

    public void Dispose()
    {
        dragAndDropSceneObjectView.OnDrop -= Drop;
    }

    private void Drop()
    {
        buildModeHUDController.SceneObjectDroppedInView();
    }
}
