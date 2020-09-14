using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatalogGroupListView : MonoBehaviour
{
    public Transform contentPanel;

    public CatalogAssetGroupAdapter categoryItemAdapterPrefab;
    public System.Action<SceneObject> OnSceneObjectClicked;


    public Dictionary<string, List<SceneObject>> groupedSceneObjects;

    public void SetContent(SceneAssetPack sceneAssetPack)
    {
        LoadCatalogAssetGroup(sceneAssetPack);

        foreach (KeyValuePair<string, List<SceneObject>> assetPackGroup in groupedSceneObjects)
        {
            CatalogAssetGroupAdapter adapter = Instantiate(categoryItemAdapterPrefab, contentPanel).GetComponent<CatalogAssetGroupAdapter>();
            adapter.SetContent(assetPackGroup.Key, assetPackGroup.Value);
            adapter.OnSceneObjectClicked += OnSceneObjectClicked;
        }

    }

    void LoadCatalogAssetGroup(SceneAssetPack sceneAssetPack)
    {

        groupedSceneObjects = new Dictionary<string, List<SceneObject>>();

        foreach (SceneObject sceneObject in sceneAssetPack.assets)
        {
            if (!groupedSceneObjects.ContainsKey(sceneObject.category))
            {
                groupedSceneObjects.Add(sceneObject.category, GetAssetsListByCategory(sceneObject.category, sceneAssetPack));
            }
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

}
