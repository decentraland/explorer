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
    internal const string FAVORITE_NAME = "Favorites";

    public event Action OnHideCatalogClicked;
    public event Action<CatalogItem> OnCatalogItemSelected;
    public event Action OnResumeInput;
    public event Action OnStopInput;

    internal ISceneCatalogView sceneCatalogView;
    internal IQuickBarController quickBarController;
    internal FavoritesController favoritesController;
    internal BIWSearchBarController biwSearchBarController;
    internal bool isShowingAssetPacks = false;
    internal bool isFilterByAssetPacks = true;

    public void Initialize(ISceneCatalogView sceneCatalogView, IQuickBarController quickBarController)
    {
        this.sceneCatalogView = sceneCatalogView;
        this.quickBarController = quickBarController;
        favoritesController = new FavoritesController(sceneCatalogView.catalogGroupList);
        biwSearchBarController = new BIWSearchBarController(sceneCatalogView);
        sceneCatalogView.OnHideCatalogClicked += HideCatalogClicked;

        if (sceneCatalogView.catalogAssetPackList != null)
            sceneCatalogView.catalogAssetPackList.OnCatalogPackClick += OnCatalogItemPackSelected;

        if (sceneCatalogView.catalogGroupList != null)
        {
            sceneCatalogView.catalogGroupList.OnCatalogItemClicked += CatalogItemSelected;
            sceneCatalogView.catalogGroupList.OnResumeInput += ResumeInput;
            sceneCatalogView.catalogGroupList.OnStopInput += StopInput;
        }

        if (sceneCatalogView.category != null)
            sceneCatalogView.category.onValueChanged.AddListener(CategoryFilter);

        if (sceneCatalogView.favorites != null)
            sceneCatalogView.favorites.onValueChanged.AddListener(FavoritesFilter);

        if (sceneCatalogView.assetPack != null)
            sceneCatalogView.assetPack.onValueChanged.AddListener(AssetsPackFilter);

        sceneCatalogView.OnSceneCatalogBack += SceneCatalogBack;
        quickBarController.OnQuickBarShortcutSelected += QuickBarInput;
        quickBarController.OnCatalogItemSelected += CatalogItemSelected;

        biwSearchBarController.OnFilterChange += AssetsFiltered;
        biwSearchBarController.OnFilterRemove += FilterRemoved;
    }

    public void Dispose()
    {
        sceneCatalogView.OnHideCatalogClicked -= HideCatalogClicked;

        if (sceneCatalogView.catalogAssetPackList != null)
            sceneCatalogView.catalogAssetPackList.OnCatalogPackClick -= OnCatalogItemPackSelected;

        if (sceneCatalogView.catalogGroupList != null)
        {
            sceneCatalogView.catalogGroupList.OnCatalogItemClicked -= CatalogItemSelected;
            sceneCatalogView.catalogGroupList.OnResumeInput -= ResumeInput;
            sceneCatalogView.catalogGroupList.OnStopInput -= StopInput;
        }

        if (sceneCatalogView.category != null)
            sceneCatalogView.category.onValueChanged.RemoveListener(CategoryFilter);

        if (sceneCatalogView.favorites != null)
            sceneCatalogView.favorites.onValueChanged.RemoveListener(FavoritesFilter);

        if (sceneCatalogView.assetPack != null)
            sceneCatalogView.assetPack.onValueChanged.RemoveListener(AssetsPackFilter);

        sceneCatalogView.OnSceneCatalogBack -= SceneCatalogBack;

        quickBarController.OnQuickBarShortcutSelected -= QuickBarInput;
        quickBarController.OnCatalogItemSelected -= CatalogItemSelected;

        biwSearchBarController.OnFilterChange -= AssetsFiltered;
        biwSearchBarController.OnFilterRemove -= FilterRemoved;

        favoritesController.Dispose();
        biwSearchBarController.Dispose();
    }

    public void AssetsFiltered(List<Dictionary<string, List<CatalogItem>>> filterObjects)
    {
        ShowCatalogContent();
        if (sceneCatalogView.catalogGroupList != null)
            sceneCatalogView.catalogGroupList.SetContent(filterObjects);
    }

    public void FilterRemoved() { ShowAssetsPacks(); }

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

    public void ToggleCatalogExpanse() { sceneCatalogView.ToggleCatalogExpanse(); }

    public void QuickBarInput(int quickBarSlot) { quickBarController.QuickBarObjectSelected(quickBarSlot); }

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

    public void CatalogItemSelected(CatalogItem catalogItem) { OnCatalogItemSelected?.Invoke(catalogItem); }

    public void ResumeInput() { OnResumeInput?.Invoke(); }

    public void StopInput() { OnStopInput?.Invoke(); }

    public void HideCatalogClicked() { OnHideCatalogClicked?.Invoke(); }

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

            sceneCatalogView.SetBackBtnSprite(false);
            biwSearchBarController.FreeFilters();
        }
    }

    public bool IsCatalogOpen() { return sceneCatalogView.IsCatalogOpen(); }

    public void ShowCategories()
    {
        if (sceneCatalogView.catalogAssetPackList != null)
        {
            sceneCatalogView.catalogAssetPackList.SetCategoryStyle();
            sceneCatalogView.catalogAssetPackList.SetContent(BIWCatalogManager.GetCatalogItemPacksFilteredByCategories());
            sceneCatalogView.catalogAssetPackList.gameObject.SetActive(true);
        }

        isShowingAssetPacks = true;
        sceneCatalogView.SetCatalogTitle(BuilderInWorldSettings.CATALOG_ASSET_PACK_TITLE);

        if (sceneCatalogView.catalogGroupList != null)
            sceneCatalogView.catalogGroupList.gameObject.SetActive(false);
    }

    public void ShowAssetsPacks()
    {
        if (sceneCatalogView.catalogAssetPackList != null)
        {
            sceneCatalogView.catalogAssetPackList.SetAssetPackStyle();
            sceneCatalogView.catalogAssetPackList.SetContent(BIWCatalogManager.GetCatalogItemPackList());
            sceneCatalogView.catalogAssetPackList.gameObject.SetActive(true);
        }
        isShowingAssetPacks = true;
        sceneCatalogView.SetCatalogTitle(BuilderInWorldSettings.CATALOG_ASSET_PACK_TITLE);
        RefreshCatalog();

        if (sceneCatalogView.catalogGroupList != null)
            sceneCatalogView.catalogGroupList.gameObject.SetActive(false);
    }

    public void ShowCatalogContent()
    {
        isShowingAssetPacks = false;
        if (sceneCatalogView.catalogAssetPackList != null)
            sceneCatalogView.catalogAssetPackList.gameObject.SetActive(false);

        if (sceneCatalogView.catalogGroupList != null)
            sceneCatalogView.catalogGroupList.gameObject.SetActive(true);

        sceneCatalogView.SetBackBtnSprite(true);
    }

    public void OpenCatalog()
    {
        RefreshCatalog();
        sceneCatalogView.SetCatalogTitle(BuilderInWorldSettings.CATALOG_ASSET_PACK_TITLE);
        Utils.UnlockCursor();
        sceneCatalogView.SetActive(true);
    }

    public void CloseCatalog() { sceneCatalogView.CloseCatalog(); }

    public void RefreshAssetPack()
    {
        if (sceneCatalogView.catalogGroupList != null)
            sceneCatalogView.catalogGroupList.RefreshDisplay();
    }

    public void RefreshCatalog()
    {
        if (sceneCatalogView.catalogAssetPackList != null)
            sceneCatalogView.catalogAssetPackList.SetContent(BIWCatalogManager.GetCatalogItemPackList());
    }

    public CatalogItemAdapter GetLastCatalogItemDragged()
    {
        if (sceneCatalogView.catalogGroupList == null)
            return null;

        return sceneCatalogView.catalogGroupList.GetLastCatalogItemDragged();
    }
}