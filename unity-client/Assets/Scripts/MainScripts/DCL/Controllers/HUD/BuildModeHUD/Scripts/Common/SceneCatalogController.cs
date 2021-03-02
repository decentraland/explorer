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

    void Initialize(ISceneCatalogView view, IQuickBarController quickBarController);
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

    private ISceneCatalogView sceneCatalogView;
    private IQuickBarController quickBarController;
    private FavoritesController favoritesController;
    private List<Dictionary<string, List<CatalogItem>>> filterObjects = new List<Dictionary<string, List<CatalogItem>>>();
    private bool isShowingAssetPacks = false;
    private bool isFilterByAssetPacks = true;
    private const string FAVORITE_NAME = "Favorites";

    public void Initialize(ISceneCatalogView sceneCatalogView, IQuickBarController quickBarController)
    {
        this.sceneCatalogView = sceneCatalogView;
        this.quickBarController = quickBarController;
        favoritesController = new FavoritesController(sceneCatalogView.catalogGroupList);

        sceneCatalogView.OnHideCatalogClicked += HideCatalogClicked;
        sceneCatalogView.catalogAssetPackList.OnCatalogPackClick += OnCatalogItemPackSelected;
        sceneCatalogView.catalogGroupList.OnCatalogItemClicked += CatalogItemSelected;
        sceneCatalogView.catalogGroupList.OnResumeInput += ResumeInput;
        sceneCatalogView.catalogGroupList.OnStopInput += StopInput;
        sceneCatalogView.searchInput.onValueChanged.AddListener(OnSearchInputChanged);
        sceneCatalogView.category.onValueChanged.AddListener(CategoryFilter);
        sceneCatalogView.favorites.onValueChanged.AddListener(FavoritesFilter);
        sceneCatalogView.assetPack.onValueChanged.AddListener(AssetsPackFilter);
        sceneCatalogView.OnSceneCatalogBack += SceneCatalogBack;
        quickBarController.OnQuickBarShortcutSelected += QuickBarInput;
        quickBarController.OnCatalogItemSelected += CatalogItemSelected;
    }

    public void Dispose()
    {
        sceneCatalogView.OnHideCatalogClicked -= HideCatalogClicked;
        sceneCatalogView.catalogAssetPackList.OnCatalogPackClick -= OnCatalogItemPackSelected;
        sceneCatalogView.catalogGroupList.OnCatalogItemClicked -= CatalogItemSelected;
        sceneCatalogView.catalogGroupList.OnResumeInput -= ResumeInput;
        sceneCatalogView.catalogGroupList.OnStopInput -= StopInput;
        sceneCatalogView.searchInput.onValueChanged.RemoveListener(OnSearchInputChanged);
        sceneCatalogView.category.onValueChanged.RemoveListener(CategoryFilter);
        sceneCatalogView.favorites.onValueChanged.RemoveListener(FavoritesFilter);
        sceneCatalogView.assetPack.onValueChanged.RemoveListener(AssetsPackFilter);
        sceneCatalogView.OnSceneCatalogBack -= SceneCatalogBack;
        quickBarController.OnQuickBarShortcutSelected -= QuickBarInput;

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
            sceneCatalogView.catalogGroupList.SetContent(filterObjects);
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

    internal void AddNewSceneObjectCategoryToFilter(CatalogItem catalogItem)
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

        sceneCatalogView.catalogGroupList.SetContent(favorites);
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

    internal void SetCatalogAssetPackInListView(CatalogItemPack catalogItemPack)
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

        sceneCatalogView.catalogGroupList.SetContent(contentList);
    }

    internal List<CatalogItem> GetAssetsListByCategory(string category, CatalogItemPack sceneAssetPack)
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
        sceneCatalogView.catalogAssetPackList.SetCategoryStyle();
        sceneCatalogView.catalogAssetPackList.SetContent(BIWCatalogManager.GetCatalogItemPacksFilteredByCategories());
        isShowingAssetPacks = true;
        sceneCatalogView.SetCatalogTitle(BuilderInWorldSettings.CATALOG_ASSET_PACK_TITLE);
        sceneCatalogView.catalogAssetPackList.gameObject.SetActive(true);
        sceneCatalogView.catalogGroupList.gameObject.SetActive(false);
    }

    public void ShowAssetsPacks()
    {
        sceneCatalogView.catalogAssetPackList.SetAssetPackStyle();
        sceneCatalogView.catalogAssetPackList.SetContent(BIWCatalogManager.GetCatalogItemPackList());
        isShowingAssetPacks = true;
        sceneCatalogView.SetCatalogTitle(BuilderInWorldSettings.CATALOG_ASSET_PACK_TITLE);
        RefreshCatalog();
        sceneCatalogView.catalogAssetPackList.gameObject.SetActive(true);
        sceneCatalogView.catalogGroupList.gameObject.SetActive(false);
    }

    public void ShowCatalogContent()
    {
        isShowingAssetPacks = false;
        sceneCatalogView.catalogAssetPackList.gameObject.SetActive(false);
        sceneCatalogView.catalogGroupList.gameObject.SetActive(true);
    }

    public void OpenCatalog()
    {
        RefreshCatalog();
        sceneCatalogView.SetCatalogTitle(BuilderInWorldSettings.CATALOG_ASSET_PACK_TITLE);
        Utils.UnlockCursor();
        sceneCatalogView.SetActive(true);
    }

    public void CloseCatalog()
    {
        sceneCatalogView.CloseCatalog();
    }

    public void RefreshAssetPack()
    {
        sceneCatalogView.catalogGroupList.RefreshDisplay();
    }

    public void RefreshCatalog()
    {
        sceneCatalogView.catalogAssetPackList.SetContent(BIWCatalogManager.GetCatalogItemPackList());
    }

    public CatalogItemAdapter GetLastCatalogItemDragged()
    {
        return sceneCatalogView.catalogGroupList.GetLastCatalogItemDragged();
    }
}
