using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DCL.Helpers;

public class CatalogItemAdapter : MonoBehaviour
{
    public Image tumbmailImg;
    public System.Action<SceneObject> OnSceneObjectClicked;

    SceneObject content;
    public void SetContent(SceneObject sceneObject)
    {
        //tumbmailImg.sprite = sceneObject;
        content = sceneObject;

    }


    public void SceneObjectClicked()
    {
        Debug.Log("Clicked ");
        OnSceneObjectClicked?.Invoke(content);
    }
}
