using UnityEngine;
using UnityEngine.EventSystems;

public interface IQuickBarController
{
    event System.Action<int> OnQuickBarShortcutSelected;
    event System.Action<CatalogItem> OnCatalogItemSelected;

    void Initialize(IQuickBarView view, ISceneCatalogController sceneCatalogController);
    void Dispose();
    int GetSlotsCount();
    CatalogItem QuickBarObjectSelected(int index);
    void SetIndexToDrop(int index);
    void SceneObjectDropped(BaseEventData data);
    void QuickBarInput(int quickBarSlot);
}

public class QuickBarController : IQuickBarController
{
    public event System.Action<int> OnQuickBarShortcutSelected;
    public event System.Action<CatalogItem> OnCatalogItemSelected;

    private IQuickBarView quickBarView;
    private ISceneCatalogController sceneCatalogController;

    private CatalogItem[] quickBarShortcutsCatalogItems = new CatalogItem[AMOUNT_OF_QUICK_SLOTS];
    private int lastIndexDroped = -1;

    const int AMOUNT_OF_QUICK_SLOTS = 9;

    public void Initialize(IQuickBarView quickBarView, ISceneCatalogController sceneCatalogController)
    {
        this.quickBarView = quickBarView;
        this.sceneCatalogController = sceneCatalogController;

        quickBarView.OnQuickBarObjectSelected += OnQuickBarObjectSelected;
        quickBarView.OnSetIndexToDrop += SetIndexToDrop;
        quickBarView.OnSceneObjectDropped += SceneObjectDropped;
        quickBarView.OnQuickBarInputTriggered += QuickBarInput;
    }

    public void Dispose()
    {
        quickBarView.OnQuickBarObjectSelected -= OnQuickBarObjectSelected;
        quickBarView.OnSetIndexToDrop -= SetIndexToDrop;
        quickBarView.OnSceneObjectDropped -= SceneObjectDropped;
        quickBarView.OnQuickBarInputTriggered -= QuickBarInput;
    }

    public int GetSlotsCount()
    {
        return AMOUNT_OF_QUICK_SLOTS;
    }

    public CatalogItem QuickBarObjectSelected(int index)
    {
        if (quickBarShortcutsCatalogItems.Length > index && quickBarShortcutsCatalogItems[index] != null)
        {
            OnCatalogItemSelected?.Invoke(quickBarShortcutsCatalogItems[index]);
            return quickBarShortcutsCatalogItems[index];
        }

        return null;
    }

    private void OnQuickBarObjectSelected(int obj)
    {
        QuickBarObjectSelected(obj);
    }

    public void SetIndexToDrop(int index)
    {
        lastIndexDroped = index;
    }

    public void SceneObjectDropped(BaseEventData data)
    {
        CatalogItemAdapter adapter = sceneCatalogController.GetLastCatalogItemDragged();
        CatalogItem catalogItem = adapter.GetContent();

        if (adapter.thumbnailImg.enabled)
        {
            Texture texture = adapter.thumbnailImg.texture;
            SetQuickBarShortcut(catalogItem, lastIndexDroped, texture);
        }
    }

    private void SetQuickBarShortcut(CatalogItem catalogItem, int index, Texture texture)
    {
        quickBarShortcutsCatalogItems[index] = catalogItem;
        quickBarView.SetTextureToShortcut(index, texture);
    }

    public void QuickBarInput(int quickBarSlot)
    {
        OnQuickBarShortcutSelected?.Invoke(quickBarSlot);
    }
}
