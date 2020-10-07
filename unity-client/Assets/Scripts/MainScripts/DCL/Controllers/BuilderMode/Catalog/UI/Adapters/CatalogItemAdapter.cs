using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DCL.Helpers;
using System;
using DCL;

public class CatalogItemAdapter : MonoBehaviour
{
    public RawImage thumbnailImg;
    public Image favImg;

    public Color offFavoriteColor, onFavoriteColor;

    public System.Action<SceneObject> OnSceneObjectClicked;
    public System.Action<SceneObject, CatalogItemAdapter> OnSceneObjectFavorite;

    SceneObject sceneObject;

    string loadedThumbnailURL;
    AssetPromise_Texture loadedThumbnailPromise;


    public void SetContent(SceneObject sceneObject)
    {
        this.sceneObject = sceneObject;

        if(sceneObject.isFavorite) favImg.color = onFavoriteColor;
        else favImg.color = offFavoriteColor;
        GetThumbnail();
    }



    private void GetThumbnail()
    {
        var url = sceneObject?.ComposeThumbnailUrl();

        if (url == loadedThumbnailURL)
            return;

        if (sceneObject == null || string.IsNullOrEmpty(url))
            return;

        string newLoadedThumbnailURL = url;
        var newLoadedThumbnailPromise =  new AssetPromise_Texture(url);


        newLoadedThumbnailPromise.OnSuccessEvent += SetThumbnail;
        newLoadedThumbnailPromise.OnFailEvent += x => { Debug.Log($"Error downloading: {url}"); };

        AssetPromiseKeeper_Texture.i.Keep(newLoadedThumbnailPromise);


        AssetPromiseKeeper_Texture.i.Forget(loadedThumbnailPromise);
        loadedThumbnailPromise = newLoadedThumbnailPromise;
        loadedThumbnailURL = newLoadedThumbnailURL;
    }


    public void SetFavorite(bool isOn)
    {
        if (isOn) favImg.color = onFavoriteColor;
        else favImg.color = offFavoriteColor;
    }

  

    public void FavoriteIconClicked()
    {
        
        OnSceneObjectFavorite?.Invoke(sceneObject, this);
    }
    public void SceneObjectClicked()
    {
        OnSceneObjectClicked?.Invoke(sceneObject);
    }

    public void SetThumbnail(Asset_Texture texture)
    {
        if (thumbnailImg != null)
        {
            thumbnailImg.enabled = true;
            thumbnailImg.texture = texture.texture;
            favImg.gameObject.SetActive(true);
        }
    }
}
