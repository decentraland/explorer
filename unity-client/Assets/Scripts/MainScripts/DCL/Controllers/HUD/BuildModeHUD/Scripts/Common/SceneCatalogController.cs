using DCL.Configuration;
using DCL.Helpers;
using System;
using System.Collections.Generic;

public interface ISceneCatalogController
{
    event Action OnHideCatalogClicked;
    event Action<CatalogItem> OnCatalogItemSelected;
    event Action OnResumeInput;
    event Action OnStopInput;

    void Initialize(SceneCatalogView view, IQuickBarController quickBarController);
    void Dispose();
    void AssetsPackFilter(bool isOn);
    void CategoryFilter(bool isOn);
    void FavoritesFilter(bool isOn);
    void ToggleCatalogExpanse();
    void OnSearchInputChanged(string currentSearchInput);
    List<Dictionary<string, List<CatalogItem>>> FilterAssets(string nameToFilter);
    void QuickBarInput(int quickBarSlot);
    void ShowFavorites();
    void CatalogItemSelected(CatalogItem catalogItem);
    void OnCatalogItemPackSelected(CatalogItemPack catalogItemPack);
    void SceneCatalogBack();
    bool IsCatalogOpen();
    void ShowCategories();
    void ShowAssetsPacks();
    void ShowCatalogContent();
    void OpenCatalog();
    void CloseCatalog();
    void RefreshAssetPack();
    void RefreshCatalog();
    CatalogItemAdapter GetLastCatalogItemDragged();
}

public class SceneCatalogController : ISceneCatalogController
{
    public event Action OnHideCatalogClicked;
    public event Action<CatalogItem> OnCatalogItemSelected;
    public event Action OnResumeInput;
    public event Action OnStopInput;

    private SceneCatalogView sceneCatalogView;
    private List<Dictionary<string, List<CatalogItem>>> filterObjects = new List<Dictionary<string, List<CatalogItem>>>();
    private IQuickBarController quickBarController;
    private FavoritesController favoritesController;
    internal bool isShowingAssetPacks = false;
    private bool isFilterByAssetPacks = true;
    private const string FAVORITE_NAME = "Favorites";

    public void Initialize(
        SceneCatalogView sceneCatalogView,
        IQuickBarController quickBarController)
    {
        this.sceneCatalogView = sceneCatalogView;
        this.quickBarController = quickBarController;
        favoritesController = new FavoritesController(sceneCatalogView.catalogGroupListView);

        sceneCatalogView.OnHideCatalogClicked += HideCatalogClicked;
        sceneCatalogView.catalogAssetPackListView.OnCatalogPackClick += OnCatalogItemPackSelected;
        sceneCatalogView.catalogGroupListView.OnCatalogItemClicked += CatalogItemSelected;
        sceneCatalogView.catalogGroupListView.OnResumeInput += ResumeInput;
        sceneCatalogView.catalogGroupListView.OnStopInput += StopInput;
        sceneCatalogView.searchInputField.onValueChanged.AddListener(OnSearchInputChanged);
        sceneCatalogView.categoryToggle.onValueChanged.AddListener(CategoryFilter);
        sceneCatalogView.favoritesToggle.onValueChanged.AddListener(FavoritesFilter);
        sceneCatalogView.assetPackToggle.onValueChanged.AddListener(AssetsPackFilter);
        sceneCatalogView.OnSceneCatalogBack += SceneCatalogBack;
        quickBarController.OnQuickBarShortcutSelected += QuickBarInput;
        quickBarController.OnCatalogItemSelected += CatalogItemSelected;
    }

    public void Dispose()
    {
        sceneCatalogView.OnHideCatalogClicked -= HideCatalogClicked;
        quickBarController.OnQuickBarShortcutSelected -= QuickBarInput;
        sceneCatalogView.catalogAssetPackListView.OnCatalogPackClick -= OnCatalogItemPackSelected;
        sceneCatalogView.catalogGroupListView.OnCatalogItemClicked -= CatalogItemSelected;
        sceneCatalogView.catalogGroupListView.OnResumeInput -= ResumeInput;
        sceneCatalogView.catalogGroupListView.OnStopInput -= StopInput;
        sceneCatalogView.searchInputField.onValueChanged.RemoveListener(OnSearchInputChanged);
        sceneCatalogView.categoryToggle.onValueChanged.RemoveListener(CategoryFilter);
        sceneCatalogView.favoritesToggle.onValueChanged.RemoveListener(FavoritesFilter);
        sceneCatalogView.assetPackToggle.onValueChanged.RemoveListener(AssetsPackFilter);
        sceneCatalogView.OnSceneCatalogBack -= SceneCatalogBack;

        if (quickBarController != null)
            quickBarController.OnCatalogItemSelected -= CatalogItemSelected;
    }

    public void AssetsPackFilter(bool isOn)
    {
        if (!isOn)
            return;

        isFilterByAssetPacks = true;
        ShowAssetsPacks();
    }

    public void CategoryFilter(bool isOn)
    {
        if (!isOn)
            return;

        isFilterByAssetPacks = false;
        ShowCategories();
    }

    public void FavoritesFilter(bool isOn)
    {
        if (!isOn)
            return;

        ShowFavorites();
    }

    public void ToggleCatalogExpanse()
    {
        sceneCatalogView.ToggleCatalogExpanse();
    }

    public void OnSearchInputChanged(string currentSearchInput)
    {
        if (string.IsNullOrEmpty(currentSearchInput))
        {
            ShowAssetsPacks();
        }
        else
        {
            ShowCatalogContent();
            FilterAssets(currentSearchInput);
            sceneCatalogView.catalogGroupListView.SetContent(filterObjects);
        }
    }

    public List<Dictionary<string, List<CatalogItem>>> FilterAssets(string nameToFilter)
    {
        filterObjects.Clear();
        foreach (CatalogItemPack assetPack in BIWCatalogManager.GetCatalogItemPackList())
        {
            foreach (CatalogItem catalogItem in assetPack.assets)
            {
                if (catalogItem.category.Contains(nameToFilter) || catalogItem.tags.Contains(nameToFilter) || catalogItem.name.Contains(nameToFilter))
                {
                    bool foundCategory = false;
                    foreach (Dictionary<string, List<CatalogItem>> groupedSceneObjects in filterObjects)
                    {
                        if (groupedSceneObjects.ContainsKey(catalogItem.category))
                        {
                            foundCategory = true;
                            if (!groupedSceneObjects[catalogItem.category].Contains(catalogItem))
                                groupedSceneObjects[catalogItem.category].Add(catalogItem);
                        }
                    }
                    if (!foundCategory)
                    {
                        AddNewSceneObjectCategoryToFilter(catalogItem);
                    }
                }
            }
        }
        return filterObjects;
    }

    private void AddNewSceneObjectCategoryToFilter(CatalogItem catalogItem)
    {
        Dictionary<string, List<CatalogItem>> groupedCatalogItems = new Dictionary<string, List<CatalogItem>>();
        groupedCatalogItems.Add(catalogItem.category, new List<CatalogItem>() { catalogItem });
        filterObjects.Add(groupedCatalogItems);
    }

    public void QuickBarInput(int quickBarSlot)
    {
        quickBarController.QuickBarObjectSelected(quickBarSlot);
    }

    public void ShowFavorites()
    {
        sceneCatalogView.SetCatalogTitle(FAVORITE_NAME);
        ShowCatalogContent();

        List<Dictionary<string, List<CatalogItem>>> favorites = new List<Dictionary<string, List<CatalogItem>>>();
        Dictionary<string, List<CatalogItem>> groupedCategoryItems = new Dictionary<string, List<CatalogItem>>();
        groupedCategoryItems.Add(FAVORITE_NAME, favoritesController.GetFavorites());
        favorites.Add(groupedCategoryItems);

        sceneCatalogView.catalogGroupListView.SetContent(favorites);
    }

    public void CatalogItemSelected(CatalogItem catalogItem)
    {
        OnCatalogItemSelected?.Invoke(catalogItem);
    }

    public void ResumeInput()
    {
        OnResumeInput?.Invoke();
    }

    public void StopInput()
    {
        OnStopInput?.Invoke();
    }

    public void HideCatalogClicked()
    {
        OnHideCatalogClicked?.Invoke();
    }

    public void OnCatalogItemPackSelected(CatalogItemPack catalogItemPack)
    {
        ShowCatalogContent();

        SetCatalogAssetPackInListView(catalogItemPack);
    }

    private void SetCatalogAssetPackInListView(CatalogItemPack catalogItemPack)
    {
        sceneCatalogView.SetCatalogTitle(catalogItemPack.title);
        Dictionary<string, List<CatalogItem>> groupedCatalogItem = new Dictionary<string, List<CatalogItem>>();

        foreach (CatalogItem sceneObject in catalogItemPack.assets)
        {
            string titleToUse = sceneObject.categoryName;

            if (!groupedCatalogItem.ContainsKey(titleToUse))
            {
                groupedCatalogItem.Add(titleToUse, GetAssetsListByCategory(titleToUse, catalogItemPack));
            }
        }

        List<Dictionary<string, List<CatalogItem>>> contentList = new List<Dictionary<string, List<CatalogItem>>>
        {
            groupedCatalogItem
        };

        sceneCatalogView.catalogGroupListView.SetContent(contentList);
    }

    private List<CatalogItem> GetAssetsListByCategory(string category, CatalogItemPack sceneAssetPack)
    {
        List<CatalogItem> catalogItemList = new List<CatalogItem>();

        foreach (CatalogItem catalogItem in sceneAssetPack.assets)
        {
            if (category == catalogItem.categoryName)
                catalogItemList.Add(catalogItem);
        }

        return catalogItemList;
    }

    public void SceneCatalogBack()
    {
        if (isShowingAssetPacks)
        {
            sceneCatalogView.CloseCatalog();
        }
        else
        {
            if (isFilterByAssetPacks)
                ShowAssetsPacks();
            else
                ShowCategories();
        }
    }

    public bool IsCatalogOpen()
    {
        return sceneCatalogView.IsCatalogOpen();
    }

    public void ShowCategories()
    {
        sceneCatalogView.catalogAssetPackListView.SetCategoryStyle();
        sceneCatalogView.catalogAssetPackListView.SetContent(BIWCatalogManager.GetCatalogItemPacksFilteredByCategories());
        isShowingAssetPacks = true;
        sceneCatalogView.SetCatalogTitle(BuilderInWorldSettings.CATALOG_ASSET_PACK_TITLE);
        sceneCatalogView.catalogAssetPackListView.gameObject.SetActive(true);
        sceneCatalogView.catalogGroupListView.gameObject.SetActive(false);
    }

    public void ShowAssetsPacks()
    {
        sceneCatalogView.catalogAssetPackListView.SetAssetPackStyle();
        sceneCatalogView.catalogAssetPackListView.SetContent(BIWCatalogManager.GetCatalogItemPackList());
        isShowingAssetPacks = true;
        sceneCatalogView.SetCatalogTitle(BuilderInWorldSettings.CATALOG_ASSET_PACK_TITLE);
        RefreshCatalog();
        sceneCatalogView.catalogAssetPackListView.gameObject.SetActive(true);
        sceneCatalogView.catalogGroupListView.gameObject.SetActive(false);
    }

    public void ShowCatalogContent()
    {
        isShowingAssetPacks = false;
        sceneCatalogView.catalogAssetPackListView.gameObject.SetActive(false);
        sceneCatalogView.catalogGroupListView.gameObject.SetActive(true);
    }

    public void OpenCatalog()
    {
        RefreshCatalog();
        sceneCatalogView.SetCatalogTitle(BuilderInWorldSettings.CATALOG_ASSET_PACK_TITLE);
        Utils.UnlockCursor();
        sceneCatalogView.gameObject.SetActive(true);
    }

    public void CloseCatalog()
    {
        sceneCatalogView.CloseCatalog();
    }

    public void RefreshAssetPack()
    {
        sceneCatalogView.catalogGroupListView.RefreshDisplay();
    }

    public void RefreshCatalog()
    {
        sceneCatalogView.catalogAssetPackListView.SetContent(BIWCatalogManager.GetCatalogItemPackList());
    }

    public CatalogItemAdapter GetLastCatalogItemDragged()
    {
        return sceneCatalogView.catalogGroupListView.GetLastCatalogItemDragged();
    }
}
