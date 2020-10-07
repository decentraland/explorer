using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatalogGroupListView : ListView<Dictionary<string, List<SceneObject>>>
{

    public CatalogAssetGroupAdapter categoryItemAdapterPrefab;
    public System.Action<SceneObject> OnSceneObjectClicked;
    public System.Action<SceneObject, CatalogItemAdapter> OnSceneObjectFavorite;



    public override void AddAdapters()
    {
        base.AddAdapters();

        foreach (Dictionary<string, List<SceneObject>> assetPackGroups in contentList)
        {
            foreach (KeyValuePair<string, List<SceneObject>> assetPackGroup in assetPackGroups)
            {
                CatalogAssetGroupAdapter adapter = Instantiate(categoryItemAdapterPrefab, contentPanelTransform).GetComponent<CatalogAssetGroupAdapter>();
                adapter.SetContent(assetPackGroup.Key, assetPackGroup.Value);
                adapter.OnSceneObjectClicked += SceneObjectSelected;
                adapter.OnSceneObjectFavorite += SceneObjectFavorite;
            }
        }         
    }



   void SceneObjectSelected(SceneObject sceneObject)
    {

        OnSceneObjectClicked?.Invoke(sceneObject);


    }
    void SceneObjectFavorite(SceneObject sceneObject,CatalogItemAdapter adapter)
    {

        OnSceneObjectFavorite?.Invoke(sceneObject, adapter);


    }
}
