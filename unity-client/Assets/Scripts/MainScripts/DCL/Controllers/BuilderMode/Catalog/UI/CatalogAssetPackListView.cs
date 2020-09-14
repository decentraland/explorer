using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatalogAssetPackListView : MonoBehaviour
{
    public Transform contentPanel;

    public CatalogAssetPackAdapter catalopgAssetPackItemAdapterPrefab;
    public System.Action<SceneAssetPack> OnSceneAssetPackClick;


    public void SetContent(List<SceneAssetPack> sceneAssetPackList)
    {
        //tumbmailImg.sprite = sceneObject;
        //content = sceneAssetPackList;
        foreach(SceneAssetPack sceneAssetPack in sceneAssetPackList)
        {
            CatalogAssetPackAdapter adapter = Instantiate(catalopgAssetPackItemAdapterPrefab, contentPanel).GetComponent<CatalogAssetPackAdapter>();
            adapter.SetContent(sceneAssetPack);
            adapter.OnSceneAssetPackClick += SceneAssetPackClick;
        }
    }




    void SceneAssetPackClick(SceneAssetPack sceneAssetPack)
    {
        OnSceneAssetPackClick?.Invoke(sceneAssetPack);
    }
}
