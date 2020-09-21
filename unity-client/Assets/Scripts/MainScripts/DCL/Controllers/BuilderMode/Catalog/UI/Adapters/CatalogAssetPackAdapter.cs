using System;
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

        CacheController.i.GetSprite("https://builder-api.decentraland.org/v1/storage/assetPacks/" + _sceneAssetPack.thumbnail, SetSprite);
    }


    public void SceneAssetPackClick()
    {
        OnSceneAssetPackClick?.Invoke(sceneAssetPack);
    }

    public void SetSprite(Sprite sprite)
    {
        if (packImg != null)
        {
            packImg.enabled = true;
            packImg.sprite = sprite;
        }
    }
  
}
