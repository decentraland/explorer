public interface IShortcutsController
{
    void Initialize(ShortcutsView publishPopupView);
    void Dispose();
    void SetActive(bool isActive);
}

public class ShortcutsController : IShortcutsController
{
    private ShortcutsView publishPopupView;

    public void Initialize(ShortcutsView publishPopupView)
    {
        this.publishPopupView = publishPopupView;
    }

    public void Dispose()
    {
    }

    public void SetActive(bool isActive)
    {
        publishPopupView.SetActive(isActive);
    }
}
