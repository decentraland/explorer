
using DCL.Configuration;
using DCL.Helpers;
using DCL.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

public class SceneObjectCatalogController : MonoBehaviour 
{
    public event System.Action<string> OnResultReceived;
    public event System.Action<CatalogItem> OnCatalogItemSelected;
    public event System.Action<CatalogItem, CatalogItemAdapter> OnCatalogItemFavorite;

    [Header("Prefab References")]
    public TextMeshProUGUI catalogTitleTxt;
    public CatalogAssetPackListView catalogAssetPackListView;
    public CatalogGroupListView catalogGroupListView;
    public TMP_InputField searchInputField;
    public FavoritesController favoritesController;
    public QuickBarView quickBarView;
    public Toggle categoryToggle;
    public Toggle favoritesToggle;
    public Toggle assetPackToggle;

    [Header("Catalog RectTransforms")]
    public RectTransform panelRT;
    public RectTransform headerRT;
    public RectTransform searchBarRT;
    public RectTransform assetPackRT;
    public RectTransform categoryRT;

    [Header("MinSize Catalog RectTransforms")]

    public RectTransform panelMinSizeRT;
    public RectTransform headerMinSizeRT;
    public RectTransform searchBarMinSizeRT;
    public RectTransform assetPackMinSizeRT;

    [Header("MaxSize Catalog RectTransforms")]

    public RectTransform panelMaxSizeRT;
    public RectTransform headerMaxSizeRT;
    public RectTransform searchBarMaxSizeRT;
    public RectTransform assetPackMaxSizeRT;

    List<Dictionary<string, List<CatalogItem>>> filterObjects = new List<Dictionary<string, List<CatalogItem>>>();
    List<CatalogItemPack> categoryList;

    string lastFilterName = "";
    bool catalogInitializaed = false, isShowingAssetPacks = false, isFavoriteFilterActive = false;

    bool isCatalogExpanded = false;

    bool isFilterByAssetPacks = true;

    const string favoriteName = "Favorites";
    QuickBarController quickBarController;

    private void Start()
    {
        quickBarController = new QuickBarController(quickBarView);
        favoritesController = new FavoritesController(catalogGroupListView);

        quickBarView.OnQuickBarShortcutSelected += QuickBarInput;
        catalogAssetPackListView.OnCatalogPackClick += OnCatalogItemPackSelected;
        catalogGroupListView.OnCatalogItemClicked += SceneObjectSelected;
        searchInputField.onValueChanged.AddListener(OnSearchInputChanged);

     
        quickBarController.OnCatalogItemSelected += SceneObjectSelected;

        categoryToggle.onValueChanged.AddListener(CategoryFilter);
        favoritesToggle.onValueChanged.AddListener(FavoritesFilter);
        assetPackToggle.onValueChanged.AddListener(AssetsPackFilter);


    }

    private void OnDestroy()
    {
        quickBarView.OnQuickBarShortcutSelected -= QuickBarInput;
        catalogAssetPackListView.OnCatalogPackClick -= OnCatalogItemPackSelected;
        catalogGroupListView.OnCatalogItemClicked -= SceneObjectSelected;
        if(quickBarController != null)
            quickBarController.OnCatalogItemSelected -= SceneObjectSelected;
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
        if(isCatalogExpanded)
        {
            BuilderInWorldUtils.CopyRectTransform(panelRT, panelMinSizeRT);
            BuilderInWorldUtils.CopyRectTransform(headerRT, headerMinSizeRT);
            BuilderInWorldUtils.CopyRectTransform(searchBarRT, searchBarMinSizeRT);
            BuilderInWorldUtils.CopyRectTransform(assetPackRT, assetPackMinSizeRT);
            BuilderInWorldUtils.CopyRectTransform(categoryRT, assetPackMinSizeRT);
        }
        else
        {
            BuilderInWorldUtils.CopyRectTransform(panelRT, panelMaxSizeRT);
            BuilderInWorldUtils.CopyRectTransform(headerRT, headerMaxSizeRT);
            BuilderInWorldUtils.CopyRectTransform(searchBarRT, searchBarMaxSizeRT);
            BuilderInWorldUtils.CopyRectTransform(assetPackRT, assetPackMaxSizeRT);
            BuilderInWorldUtils.CopyRectTransform(categoryRT, assetPackMaxSizeRT);
        }

        isCatalogExpanded = !isCatalogExpanded;
    }

    #region Filter
    void OnSearchInputChanged(string currentSearchInput)
    {
        if (string.IsNullOrEmpty(currentSearchInput))
        {
            ShowAssetsPacks();
        }
        else
        {
            ShowCatalogContent();
            FilterAssets(currentSearchInput);
            catalogGroupListView.SetContent(filterObjects);
        }
        lastFilterName = currentSearchInput;
    }

    void FilterAssets(string nameToFilter)
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
    }

    void AddNewSceneObjectCategoryToFilter(CatalogItem sceneObject)
    {
        Dictionary<string, List<CatalogItem>> groupedSceneObjects = new Dictionary<string, List<CatalogItem>>();
        groupedSceneObjects.Add(sceneObject.category, new List<CatalogItem>() { sceneObject });
        filterObjects.Add(groupedSceneObjects);
    }

    #endregion

    void QuickBarInput(int quickBarSlot)
    {
        quickBarController.QuickBarObjectSelected(quickBarSlot);
    }

    public void ToggleFavorites()
    {
        isFavoriteFilterActive = !isFavoriteFilterActive;

        if (!isFavoriteFilterActive)
        {
            ShowAssetsPacks();
            return;
        }
        ShowFavorites();
    }

    void ShowFavorites()
    {
        catalogTitleTxt.text = favoriteName;
        ShowCatalogContent();

        List<Dictionary<string, List<CatalogItem>>> favorites = new List<Dictionary<string, List<CatalogItem>>>();
        Dictionary<string, List<CatalogItem>> groupedSceneObjects = new Dictionary<string, List<CatalogItem>>();
        groupedSceneObjects.Add(favoriteName, favoritesController.GetFavorites());
        favorites.Add(groupedSceneObjects);

        catalogGroupListView.SetContent(favorites);
    }
    
    void SceneObjectSelected(CatalogItem catalogItem)
    {
        OnCatalogItemSelected?.Invoke(catalogItem);
    }

    void OnCatalogItemPackSelected(CatalogItemPack sceneAssetPack)
    {
        ShowCatalogContent();

        SetAssetPackInListView(sceneAssetPack);
    }

    void SetAssetPackInListView(CatalogItemPack sceneAssetPack)
    {
        catalogTitleTxt.text = sceneAssetPack.title;
        Dictionary<string, List<CatalogItem>> groupedSceneObjects = new Dictionary<string, List<CatalogItem>>();

        foreach (CatalogItem sceneObject in sceneAssetPack.assets)
        {
            string titleToUse = sceneObject.categoryName;

            if (!groupedSceneObjects.ContainsKey(titleToUse))
            {          
                groupedSceneObjects.Add(titleToUse, GetAssetsListByCategory(titleToUse, sceneAssetPack));
            }
        }

        List<Dictionary<string, List<CatalogItem>>> contentList = new List<Dictionary<string, List<CatalogItem>>>
        {
            groupedSceneObjects
        };

        catalogGroupListView.SetContent(contentList);
    }

    public void Back()
    {
        if (isShowingAssetPacks)
        {
            CloseCatalog();
        }
        else
        {
            if (isFilterByAssetPacks)
                ShowAssetsPacks();
            else
                ShowCategories();
        }

        isFavoriteFilterActive = false;
    }

    public bool IsCatalogOpen()
    {
        return gameObject.activeSelf;
    }

    //void FilterCategories()
    //{
    //    categoryList = new List<CatalogItemPack>();
    //    var assetPacks = AssetCatalogBridge.sceneAssetPackCatalog.GetValues().ToList();

    //    Dictionary<string, CatalogItemPack> assetPackDic = new Dictionary<string, CatalogItemPack>();

    //    foreach(SceneAssetPack assetPack in assetPacks)
    //    {
    //        foreach(SceneObject sceneObject in assetPack.assets)
    //        {
    //            if (!assetPackDic.ContainsKey(sceneObject.category))
    //            {
    //                SceneAssetPack categoryAssetPack = new SceneAssetPack();
    //                categoryAssetPack.thumbnail = sceneObject.category;
    //                categoryAssetPack.title = sceneObject.category;
    //                categoryAssetPack.assets = new List<SceneObject>();
    //                sceneObject.titleToShow = assetPack.title;
    //                categoryAssetPack.assets.Add(sceneObject);

    //                if(!string.IsNullOrEmpty(categoryAssetPack.title))
    //                {
    //                    if (categoryAssetPack.title.Length == 1)
    //                        categoryAssetPack.title = categoryAssetPack.title.ToUpper();
    //                    else
    //                        categoryAssetPack.title =  char.ToUpper(categoryAssetPack.title[0]) + categoryAssetPack.title.Substring(1);
    //                }

    //                assetPackDic.Add(sceneObject.category, categoryAssetPack);
    //                continue;
    //            }
    //            else
    //            {
    //                sceneObject.titleToShow = assetPack.title;
    //                assetPackDic[sceneObject.category].assets.Add(sceneObject);
    //            }
    //        }
    //    }

    //    categoryList = assetPackDic.Values.ToList();
    //}

    public void ShowCategories()
    {
        //if(categoryList == null)
        //{
        //    FilterCategories();
        //}

        catalogAssetPackListView.SetCategoryStyle();
        catalogAssetPackListView.SetContent(categoryList);
        isShowingAssetPacks = true;
        catalogTitleTxt.text = BuilderInWorldSettings.CATALOG_ASSET_PACK_TITLE;
        catalogAssetPackListView.gameObject.SetActive(true);
        catalogGroupListView.gameObject.SetActive(false);
    }

    public void ShowAssetsPacks()
    {
        catalogAssetPackListView.SetAssetPackStyle();
        catalogAssetPackListView.SetContent(BIWCatalogManager.GetCatalogItemPackList());
        isShowingAssetPacks = true;
        catalogTitleTxt.text = BuilderInWorldSettings.CATALOG_ASSET_PACK_TITLE;
        RefreshCatalog();
        catalogAssetPackListView.gameObject.SetActive(true);
        catalogGroupListView.gameObject.SetActive(false);
    }

    public void ShowCatalogContent()
    {
        isShowingAssetPacks = false;
        catalogAssetPackListView.gameObject.SetActive(false);
        catalogGroupListView.gameObject.SetActive(true);
    }

    public void OpenCatalog()
    {
        RefreshCatalog();
        catalogTitleTxt.text = BuilderInWorldSettings.CATALOG_ASSET_PACK_TITLE;
        Utils.UnlockCursor();
        gameObject.SetActive(true);   
    }

    public void CloseCatalog()
    {
        if(gameObject.activeSelf)
            StartCoroutine(CloseCatalogAfterOneFrame());
    }

    public void RefreshAssetPack()
    {
        catalogGroupListView.RefreshDisplay();
    }

    public void RefreshCatalog()
    {
        catalogAssetPackListView.SetContent(BIWCatalogManager.GetCatalogItemPackList());
    }


    List<CatalogItem> GetAssetsListByCategory(string category, CatalogItemPack sceneAssetPack)
    {
        List<CatalogItem> catalogItemList = new List<CatalogItem>();

        foreach (CatalogItem catalogItem in sceneAssetPack.assets)
        {
            if (category == catalogItem.categoryName)
                catalogItemList.Add(catalogItem);
        }

        return catalogItemList;
    }

    IEnumerator CloseCatalogAfterOneFrame()
    {
        yield return null;
        gameObject.SetActive(false);
    }
}
