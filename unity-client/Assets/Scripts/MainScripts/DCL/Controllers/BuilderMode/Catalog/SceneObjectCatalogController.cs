
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

    public TextMeshProUGUI catalogTitleTxt;
    public Button backBtn;
    public GameObject catalogUIGO;
    public CatalogAssetPackListView catalogAssetPackListView;
    public CatalogGroupListView catalogGroupListView;


    bool catalogInitializaed = false;
    private void Start()
    {
        OnResultReceived += AddFullSceneObjectCatalog;
        catalogAssetPackListView.OnSceneAssetPackClick += OnScenePackSelected;
        catalogGroupListView.OnSceneObjectClicked += SceneObjectSelected;
    }

    private void OnDestroy()
    {
        catalogAssetPackListView.OnSceneAssetPackClick -= OnScenePackSelected;
        catalogGroupListView.OnSceneObjectClicked -= SceneObjectSelected;
    }

    void SceneObjectSelected(SceneObject sceneObject)
    {
        Debug.Log("Object selected " + sceneObject.name);
        OnSceneObjectSelected?.Invoke(sceneObject);


    }

    void OnScenePackSelected(SceneAssetPack sceneAssetPack)
    {
        catalogAssetPackListView.gameObject.SetActive(false);
        catalogGroupListView.gameObject.SetActive(true);
        backBtn.gameObject.SetActive(true);

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

    public bool IsCatalogOpen()
    {
        return catalogUIGO.activeSelf;
    }
    public void OpenCatalog()
    {
        catalogTitleTxt.text = "Asset Packs";
        Utils.UnlockCursor();
        catalogUIGO.SetActive(true);
        catalogAssetPackListView.gameObject.SetActive(true);
        catalogGroupListView.gameObject.SetActive(false);
        backBtn.gameObject.SetActive(false);

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
        Utils.LockCursor();
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
