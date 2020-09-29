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
    public System.Action<SceneObject> OnSceneObjectClicked;

    SceneObject sceneObject;

    string loadedThumbnailURL;
    AssetPromise_Texture loadedThumbnailPromise;


    public void SetContent(SceneObject sceneObject)
    {
        this.sceneObject = sceneObject;

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
        }
    }
}
