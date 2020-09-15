
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

    public TextMeshProUGUI catalogTitleTxt;
    public Button backBtn;
    public GameObject catalogUIGO;
    public CatalogAssetPackListView catalogAssetPackListView;
    public CatalogGroupListView catalogGroupListView;
   


    private void Start()
    {
        OnResultReceived += AddFullSceneObjectCatalog;
        CatalogController.sceneObjectCatalog.GetValues();
        StartCoroutine(GetCatalog());
        catalogAssetPackListView.OnSceneAssetPackClick += OnScenePackSelected;
        catalogGroupListView.OnSceneObjectClicked += OnSceneObjectSelected;
    }

    private void OnDestroy()
    {
        catalogAssetPackListView.OnSceneAssetPackClick -= OnScenePackSelected;
        catalogGroupListView.OnSceneObjectClicked -= OnSceneObjectSelected;
    }

    void OnSceneObjectSelected(SceneObject sceneObject)
    {
        Debug.Log("Object selected " + sceneObject.name);
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

    public void OpenCatalog()
    {
        catalogTitleTxt.text = "Asset Packs";
        catalogUIGO.SetActive(true);
        catalogAssetPackListView.gameObject.SetActive(true);
        catalogGroupListView.gameObject.SetActive(false);
        backBtn.gameObject.SetActive(false);
        catalogAssetPackListView.SetContent(CatalogController.sceneObjectCatalog.GetValues().ToList());
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


        JArray array = JArray.Parse(jObject["data"].ToString());

        foreach(JObject item in array)
        {
            CatalogController.i.AddSceneObjectToCatalog(item);
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


    IEnumerator GetCatalog()
    {
        UnityWebRequest www = UnityWebRequest.Get("https://builder-api.decentraland.org/v1/assetPacks");
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            // Show results as text
            Debug.Log(www.downloadHandler.text);

            // Or retrieve results as binary data
            byte[] byteArray = www.downloadHandler.data;

            string result = System.Text.Encoding.UTF8.GetString(byteArray);
            OnResultReceived?.Invoke(result);
        }
    }
}
