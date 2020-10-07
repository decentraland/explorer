
using DCL.Helpers;
using DCL.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class SceneObjectCatalogController : MonoBehaviour 
{
    public System.Action<string> OnResultReceived;
    public System.Action<SceneObject> OnSceneObjectSelected;
    public System.Action<SceneObject, CatalogItemAdapter> OnSceneObjectFavorite;

    public TextMeshProUGUI catalogTitleTxt;
    public GameObject catalogUIGO;
    public CatalogAssetPackListView catalogAssetPackListView;
    public CatalogGroupListView catalogGroupListView;
    public TMP_InputField searchInputField;
    public RawImage[] shortcutsImgs;

    List<Dictionary<string, List<SceneObject>>> filterObjects = new List<Dictionary<string, List<SceneObject>>>();
    string lastFilterName = "";
    bool catalogInitializaed = false, isShowingAssetPacks = false;
    List<SceneObject> favoritesShortcutsSceneObjects = new List<SceneObject>();
    private void Start()
    {
        OnResultReceived += AddFullSceneObjectCatalog;
        catalogAssetPackListView.OnSceneAssetPackClick += OnScenePackSelected;
        catalogGroupListView.OnSceneObjectClicked += SceneObjectSelected;
        catalogGroupListView.OnSceneObjectFavorite += AsignFavorite;
        searchInputField.onValueChanged.AddListener(OnSearchInputChanged);
    }

    private void OnDestroy()
    {
        catalogAssetPackListView.OnSceneAssetPackClick -= OnScenePackSelected;
        catalogGroupListView.OnSceneObjectClicked -= SceneObjectSelected;
    }

    void OnSearchInputChanged(string currentSearchInput)
    {
        if (string.IsNullOrEmpty(currentSearchInput)) ShowAssetsPacks();
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
                            if (!groupedSceneObjects[sceneObject.category].Contains(sceneObject)) groupedSceneObjects[sceneObject.category].Add(sceneObject);
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


    public void AsignFavorite(SceneObject sceneObject, CatalogItemAdapter adapter)
    {
        if (favoritesShortcutsSceneObjects.Count < 9)
        {

            if (!favoritesShortcutsSceneObjects.Contains(sceneObject))
            {
                int index = favoritesShortcutsSceneObjects.Count;
                int cont = 0;
                foreach (SceneObject sceneObjectIteration in favoritesShortcutsSceneObjects)
                {
                    if (sceneObjectIteration == null)
                    {
                        index = cont;
                        break;
                    }
                    cont++;
                }
                if (index >= favoritesShortcutsSceneObjects.Count) favoritesShortcutsSceneObjects.Add(sceneObject);
                else favoritesShortcutsSceneObjects[index] = sceneObject;
                shortcutsImgs[index].texture = adapter.thumbnailImg.texture;
                shortcutsImgs[index].enabled = true;
                sceneObject.isFavorite = true;
            }
            else
            {
                int index = favoritesShortcutsSceneObjects.IndexOf(sceneObject);
                shortcutsImgs[index].enabled = false;
                favoritesShortcutsSceneObjects[index] = null;
                sceneObject.isFavorite = false;
            }

            adapter.SetFavorite(sceneObject.isFavorite);
        }
    }

    public void FavotiteObjectSelected(int index)
    {
        if (favoritesShortcutsSceneObjects[index] != null)
        {
            OnSceneObjectSelected?.Invoke(favoritesShortcutsSceneObjects[index]);
        }
    }
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
        if (isShowingAssetPacks) CloseCatalog();
        else ShowAssetsPacks();
    }
    public bool IsCatalogOpen()
    {
        return catalogUIGO.activeSelf;
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
        catalogUIGO.SetActive(true);
        //catalogAssetPackListView.gameObject.SetActive(true);
        //catalogGroupListView.gameObject.SetActive(false);
        //backBtn.gameObject.SetActive(false);

        if (!catalogInitializaed)
        {
            CatalogController.sceneObjectCatalog.GetValues();
            //StartCoroutine(GetCatalog());
            ExternalCallsController.i.GetContentAsString("https://builder-api.decentraland.org/v1/assetPacks", AddFullSceneObjectCatalog);
            catalogInitializaed = true;
        }
    
    }

    public void CloseCatalog()
    {
        catalogUIGO.SetActive(false);
 
    }

    [ContextMenu ("Iterate catalog")]
    public void IterateCatalog()
    {
        foreach(SceneAssetPack pack in CatalogController.sceneObjectCatalog.GetValues())
        {
            foreach(SceneObject sceneObject in pack.assets)
            {
                Debug.Log("ID " + sceneObject);

            }
        }
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


    //IEnumerator GetCatalog()
    //{
    //    UnityWebRequest www = UnityWebRequest.Get("https://builder-api.decentraland.org/v1/assetPacks");
    //    yield return www.SendWebRequest();

    //    if (www.isNetworkError || www.isHttpError)
    //    {
    //        Debug.Log(www.error);
    //    }
    //    else
    //    {
    //        // Show results as text
    //        Debug.Log(www.downloadHandler.text);

    //        // Or retrieve results as binary data
    //        byte[] byteArray = www.downloadHandler.data;

    //        string result = System.Text.Encoding.UTF8.GetString(byteArray);
    //        OnResultReceived?.Invoke(result);
    //    }
    //}
}
