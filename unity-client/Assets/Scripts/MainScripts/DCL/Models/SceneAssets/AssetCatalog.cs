using DCL;
using DCL.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetCatalog : MonoBehaviour
{
    public static bool VERBOSE = false;
    public static AssetCatalog i { get; private set; }

    private static SceneAssetPackDictionary sceneAssetPackCatalogValue;
    public static SceneAssetPackDictionary sceneAssetPackCatalog
    {
        get
        {
            if (sceneAssetPackCatalogValue == null)
            {
                sceneAssetPackCatalogValue = Resources.Load<SceneAssetPackDictionary>("SceneAssetPackCatalog");
            }

            return sceneAssetPackCatalogValue;
        }
    }

    private static SceneObjectDictionary sceneObjectCatalogValue;
    public static SceneObjectDictionary sceneObjectCatalog
    {
        get
        {
            if (sceneAssetPackCatalogValue == null)
            {
                sceneObjectCatalogValue = Resources.Load<SceneObjectDictionary>("SceneObjectCatalog");
            }

            return sceneObjectCatalogValue;
        }
    }

    public void Awake()
    {
        i = this;
    }

    public static ContentProvider GetContentProviderForAssetIdInSceneAsetPackCatalog(string assetId)
    {
        foreach (SceneAssetPack assetPack in sceneAssetPackCatalog.GetValues())
        {
            foreach (SceneObject sceneObject in assetPack.assets)
            {
                if (sceneObject.id == assetId)
                {
                    return CreateContentProviderForSceneObject(sceneObject);
                }
            }
        }
        return null;
    }

    public static ContentProvider GetContentProviderForAssetIdInSceneObjectCatalog(string assetId)
    {
        ContentProvider contentProvider = null;
        if (sceneObjectCatalogValue.ContainsKey(assetId))        
            contentProvider = CreateContentProviderForSceneObject(sceneObjectCatalogValue.Get(assetId));
        
        return contentProvider;
    }

    static ContentProvider CreateContentProviderForSceneObject(SceneObject sceneObject)
    {
        ContentProvider contentProvider = new ContentProvider();
        contentProvider.baseUrl = BuilderInWorldSettings.BASE_URL_CATALOG;
        foreach (KeyValuePair<string, string> content in sceneObject.contents)
        {
            ContentServerUtils.MappingPair mappingPair = new ContentServerUtils.MappingPair();
            mappingPair.file = content.Key;
            mappingPair.hash = content.Value;
            contentProvider.contents.Add(mappingPair);
        }

        contentProvider.BakeHashes();
        return contentProvider;
    }

    public static SceneObject GetSceneObjectById(string id)
    {
        foreach (SceneObject sceneObject in sceneObjectCatalogValue.GetValues())
        {
            if (sceneObject.id == id) return sceneObject;
        }

        return null;
    }

    public void AddFullSceneObjectCatalog(string payload)
    {
        JObject jObject = (JObject)JsonConvert.DeserializeObject(payload);
        if (jObject["ok"].ToObject<bool>())
        {

            JArray array = JArray.Parse(jObject["data"].ToString());

            foreach (JObject item in array)
            {
                i.AddSceneAssetPackToCatalog(item);
            }
        }
    }

    public static void AddSceneObjectToCatalog(SceneObject sceneObject)
    {
        sceneObjectCatalogValue.Add(sceneObject.id, sceneObject);
    }

    public void AddSceneAssetPackToCatalog(JObject payload)
    {

        SceneAssetPack sceneAssetPack = JsonConvert.DeserializeObject<SceneAssetPack>(payload.ToString());

        if (VERBOSE)
            Debug.Log("add sceneObject: " + payload);


        sceneAssetPackCatalogValue.Add(sceneAssetPack.id, sceneAssetPack);
    }
}
