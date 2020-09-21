using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DCL.Helpers;
using System;

public class CatalogItemAdapter : MonoBehaviour
{
    public Image tumbmailImg;
    public System.Action<SceneObject> OnSceneObjectClicked;

    SceneObject content;
    public void SetContent(SceneObject sceneObject)
    {
        //tumbmailImg.sprite = sceneObject;
        content = sceneObject;

        CacheController.i.GetSprite("https://builder-api.decentraland.org/v1/storage/contents/" + sceneObject.thumbnail, SetSprite);

        //ExternalCallsController.i.GetContentAsByteArray("https://builder-api.decentraland.org/v1/storage/contents/"+sceneObject.thumbnail, SetSprite);
    }


    public void SceneObjectClicked()
    {
        OnSceneObjectClicked?.Invoke(content);
    }

    public void SetSprite(Sprite sprite)
    {
        if (tumbmailImg != null)
        {
            tumbmailImg.enabled = true;
            tumbmailImg.sprite = sprite;
        }
    }
}
