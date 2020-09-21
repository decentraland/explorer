using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CatalogAssetGroupAdapter : MonoBehaviour
{
    public TextMeshProUGUI categoryTxt;
    public GameObject categoryContentGO;
    public System.Action<SceneObject> OnSceneObjectClicked;

    [Header("Prefab References")]
    public GameObject catalogItemAdapterPrefab;


    public void SetContent(string category, List<SceneObject> sceneObjectsList)
    {
        categoryTxt.text = category.ToUpper();
        RemoveAdapters();
        foreach (SceneObject sceneObject in sceneObjectsList)
        {
            CatalogItemAdapter adapter = Instantiate(catalogItemAdapterPrefab, categoryContentGO.transform).GetComponent<CatalogItemAdapter>();
            adapter.SetContent(sceneObject);
            adapter.OnSceneObjectClicked += SceneObjectClicked;
        }
    }

    public void RemoveAdapters()
    {

        for (int i = 0; i < categoryContentGO.transform.childCount; i++)
        {
            GameObject toRemove = categoryContentGO.transform.GetChild(i).gameObject;
            Destroy(toRemove);
        }
    }


    void SceneObjectClicked(SceneObject sceneObjectClicked)
    {
        OnSceneObjectClicked?.Invoke(sceneObjectClicked);
    }
}
