
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
    public System.Action<string> OnResultReceived;
    public System.Action<SceneObject> OnSceneObjectSelected;
    public System.Action<SceneObject, CatalogItemAdapter> OnSceneObjectFavorite;
    public System.Action OnStopInput, OnResumeInput;

    public Canvas generalCanvas;
    public TextMeshProUGUI catalogTitleTxt;
    public CatalogAssetPackListView catalogAssetPackListView;
    public CatalogGroupListView catalogGroupListView;
    public TMP_InputField searchInputField;
    public RawImage[] shortcutsImgs;

    List<Dictionary<string, List<SceneObject>>> filterObjects = new List<Dictionary<string, List<SceneObject>>>();
    string lastFilterName = "";
    bool catalogInitializaed = false, isShowingAssetPacks = false, isFavoriteFilterActive = false;
    List<SceneObject> favoritesSceneObjects = new List<SceneObject>();
    List<SceneObject> quickBarShortcutsSceneObjects = new List<SceneObject>() { null, null, null,null,null,null,null,null,null };
    private void Start()
    {
        OnResultReceived += AddFullSceneObjectCatalog;
        catalogAssetPackListView.OnSceneAssetPackClick += OnScenePackSelected;
        catalogGroupListView.OnSceneObjectClicked += SceneObjectSelected;
        catalogGroupListView.OnSceneObjectFavorite += AsignFavorite;
        catalogGroupListView.OnAdapterStartDragging += SceneObjectStartDragged;
        catalogGroupListView.OnAdapterDrag += OnDrag;
        catalogGroupListView.OnAdapterEndDrag += OnEndDrag;
        searchInputField.onValueChanged.AddListener(OnSearchInputChanged);
    }

    private void OnDestroy()
    {
        catalogAssetPackListView.OnSceneAssetPackClick -= OnScenePackSelected;
        catalogGroupListView.OnSceneObjectClicked -= SceneObjectSelected;
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
        foreach (SceneAssetPack assetPack in CatalogController.sceneObjectCatalog.GetValues().ToList())
        {
            foreach (SceneObject sceneObject in assetPack.assets)
            {
                if (sceneObject.category.Contains(nameToFilter) || sceneObject.tags.Contains(nameToFilter) || sceneObject.name.Contains(nameToFilter))
                {
                    bool foundCategory = false;
                    foreach (Dictionary<string, List<SceneObject>> groupedSceneObjects in filterObjects)
                    {
                        if (groupedSceneObjects.ContainsKey(sceneObject.category))
                        {
                            foundCategory = true;
                            if (!groupedSceneObjects[sceneObject.category].Contains(sceneObject))
                                groupedSceneObjects[sceneObject.category].Add(sceneObject);
                        }
                    }
                    if (!foundCategory)
                    {
                        Dictionary<string, List<SceneObject>> groupedSceneObjects = new Dictionary<string, List<SceneObject>>();
                        groupedSceneObjects.Add(sceneObject.category, new List<SceneObject>() { sceneObject });
                        filterObjects.Add(groupedSceneObjects);
                    }
                }
            }
        }
    }

    #endregion

    #region DragAndDrop

    int lastIndexDroped = -1;
    GameObject draggedObject;
    public void SetIndexToDrop(int index)
    {
        lastIndexDroped = index;
    }

    void OnDrag(PointerEventData data)
    {
        draggedObject.transform.position = data.position;
    }

    void SceneObjectStartDragged(SceneObject sceneObjectClicked, CatalogItemAdapter adapter, BaseEventData data)
    {
        PointerEventData eventData = data as PointerEventData;
        if(draggedObject== null)
            draggedObject = Instantiate(adapter.gameObject, generalCanvas.transform);
        RectTransform adapterRT = adapter.GetComponent<RectTransform>();
        RectTransform newAdapterRT = draggedObject.GetComponent<RectTransform>();
        CatalogItemAdapter newAdapter = draggedObject.GetComponent<CatalogItemAdapter>();
        newAdapter.canvasGroup.blocksRaycasts = false;
        newAdapter.canvasGroup.alpha = 0.6f;
        newAdapter.SetContent(adapter.GetContent());
        newAdapterRT.sizeDelta = adapterRT.sizeDelta*0.75f;
        OnStopInput?.Invoke();
    }

    void OnEndDrag(PointerEventData data)
    {     
        Destroy(draggedObject,0.1f);
        OnResumeInput?.Invoke();
    }

    public void SceneObjectDropped(BaseEventData data)
    {
     
        CatalogItemAdapter adapter = draggedObject.GetComponent<CatalogItemAdapter>();
        SceneObject sceneObject = adapter.GetContent();
        Texture texture = null;
        if (adapter.thumbnailImg.enabled)
        {
            texture = adapter.thumbnailImg.texture;
            SetQuickBarShortcut(sceneObject, lastIndexDroped, texture);
        }
        Destroy(draggedObject);
    }

    #endregion

    void SetQuickBarShortcut(SceneObject sceneObject, int index,Texture texture)
    {     
        quickBarShortcutsSceneObjects[index] = sceneObject;
        if (index < shortcutsImgs.Length)
        {
            shortcutsImgs[index].texture = texture;
            shortcutsImgs[index].enabled = true;
        }
    }

    #region Favorites

    public void ShowFavorites()
    {
        isFavoriteFilterActive = !isFavoriteFilterActive;

        if (isFavoriteFilterActive)
        {
            string favoriteName = "Favorites";
            List<Dictionary<string, List<SceneObject>>> favorites = new List<Dictionary<string, List<SceneObject>>>();
            foreach (SceneAssetPack assetPack in CatalogController.sceneObjectCatalog.GetValues().ToList())
            {
                foreach (SceneObject sceneObject in assetPack.assets)
                {
                    foreach (SceneObject favObject in favoritesSceneObjects)
                    {
                        if (favObject != null)
                        {
                            if (sceneObject.id == favObject.id && sceneObject.asset_pack_id == favObject.asset_pack_id)
                            {
                                bool foundCategory = false;
                                foreach (Dictionary<string, List<SceneObject>> groupedSceneObjects in favorites)
                                {
                                    if (groupedSceneObjects.ContainsKey(favoriteName))
                                    {
                                        foundCategory = true;
                                        if (!groupedSceneObjects[favoriteName].Contains(sceneObject))
                                            groupedSceneObjects[favoriteName].Add(sceneObject);
                                    }
                                }
                                if (!foundCategory)
                                {
                                    Dictionary<string, List<SceneObject>> groupedSceneObjects = new Dictionary<string, List<SceneObject>>();
                                    groupedSceneObjects.Add(favoriteName, new List<SceneObject>() { sceneObject });
                                    favorites.Add(groupedSceneObjects);
                                }
                                break;

                            }
                        }
                    }
                }
            }
            catalogTitleTxt.text = favoriteName;
            ShowCatalogContent();
            catalogGroupListView.SetContent(favorites);
        }
        else
        {
            ShowAssetsPacks();
        }
       
    }
    public void AsignFavorite(SceneObject sceneObject, CatalogItemAdapter adapter)
    {

        if (!favoritesSceneObjects.Contains(sceneObject))
        {
            favoritesSceneObjects.Add(sceneObject);

            int index = quickBarShortcutsSceneObjects.Count;
            int cont = 0;
            foreach (SceneObject sceneObjectIteration in quickBarShortcutsSceneObjects)
            {
                if (sceneObjectIteration == null)
                {
                    index = cont;
                    break;
                }
                cont++;
            }
            SetQuickBarShortcut(sceneObject,index,adapter.thumbnailImg.texture);

            sceneObject.isFavorite = true;
          
        }
        else
        {
            favoritesSceneObjects.Remove(sceneObject);

            if (quickBarShortcutsSceneObjects.Contains(sceneObject))
            {
                int index = quickBarShortcutsSceneObjects.IndexOf(sceneObject);
                if (index < shortcutsImgs.Length && index != -1)
                {
                    shortcutsImgs[index].enabled = false;
                    quickBarShortcutsSceneObjects[index] = null;
                }
            }
         
            sceneObject.isFavorite = false;
        }

        adapter.SetFavorite(sceneObject.isFavorite);

    }

    public void QuickBarObjectSelected(int index)
    {
        if (quickBarShortcutsSceneObjects.Count > index)
        {
            if (quickBarShortcutsSceneObjects[index] != null)
            {
                OnSceneObjectSelected?.Invoke(quickBarShortcutsSceneObjects[index]);
            }
        }
    }

    #endregion

    void SceneObjectSelected(SceneObject sceneObject)
    {
        OnSceneObjectSelected?.Invoke(sceneObject);
    }

    void OnScenePackSelected(SceneAssetPack sceneAssetPack)
    {
        ShowCatalogContent();

        SetAssetPackInListView(sceneAssetPack);
    }

    void SetAssetPackInListView(SceneAssetPack sceneAssetPack)
    {
        catalogTitleTxt.text = sceneAssetPack.title;
        Dictionary<string, List<SceneObject>> groupedSceneObjects = new Dictionary<string, List<SceneObject>>();

        foreach (SceneObject sceneObject in sceneAssetPack.assets)
        {
            if (!groupedSceneObjects.ContainsKey(sceneObject.category))
            {
                groupedSceneObjects.Add(sceneObject.category, GetAssetsListByCategory(sceneObject.category, sceneAssetPack));
            }
        }
        List<Dictionary<string, List<SceneObject>>> contentList = new List<Dictionary<string, List<SceneObject>>>
        {
            groupedSceneObjects
        };
        catalogGroupListView.SetContent(contentList);
    }

    public void Back()
    {
        if (isShowingAssetPacks)
            CloseCatalog();
        else
            ShowAssetsPacks();
        isFavoriteFilterActive = false;
    }

    public bool IsCatalogOpen()
    {
        return gameObject.activeSelf;
    }

    public void ShowAssetsPacks()
    {
        isShowingAssetPacks = true;
        catalogTitleTxt.text = "Asset Packs";
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
        catalogTitleTxt.text = "Asset Packs";
        Utils.UnlockCursor();
        gameObject.SetActive(true);


        if (!catalogInitializaed)
        {
            CatalogController.sceneObjectCatalog.GetValues();
            ExternalCallsController.i.GetContentAsString("https://builder-api.decentraland.org/v1/assetPacks", AddFullSceneObjectCatalog);
            catalogInitializaed = true;
        }
    
    }

    public void CloseCatalog()
    {
        if(gameObject.activeSelf)
            StartCoroutine(CloseCatalogAfterOneFrame());
    }

    public void AddFullSceneObjectCatalog(string payload)
    {

        JObject jObject = (JObject)JsonConvert.DeserializeObject(payload);
        if (jObject["ok"].ToObject<bool>())
        {

            JArray array = JArray.Parse(jObject["data"].ToString());

            foreach (JObject item in array)
            {
                CatalogController.i.AddSceneObjectToCatalog(item);
            }

            catalogAssetPackListView.SetContent(CatalogController.sceneObjectCatalog.GetValues().ToList());
        }
    }

    List<SceneObject> GetAssetsListByCategory(string category, SceneAssetPack sceneAssetPack)
    {
        List<SceneObject> sceneObjectsList = new List<SceneObject>();

        foreach (SceneObject sceneObject in sceneAssetPack.assets)
        {
            if (category == sceneObject.category) sceneObjectsList.Add(sceneObject);
        }

        return sceneObjectsList;
    }

    IEnumerator CloseCatalogAfterOneFrame()
    {
        yield return null;
        gameObject.SetActive(false);
    }
 
}
