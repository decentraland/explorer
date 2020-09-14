using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CatalogAssetPackAdapter : MonoBehaviour
{
    public TextMeshProUGUI titleTxt;
    public Image packImg;

    public System.Action<SceneAssetPack> OnSceneAssetPackClick;
    SceneAssetPack sceneAssetPack;
    public void SetContent(SceneAssetPack _sceneAssetPack)
    {
        sceneAssetPack = _sceneAssetPack;
        titleTxt.text = sceneAssetPack.title;
    }


    public void SceneAssetPackClick()
    {
        OnSceneAssetPackClick?.Invoke(sceneAssetPack);
    }
}
