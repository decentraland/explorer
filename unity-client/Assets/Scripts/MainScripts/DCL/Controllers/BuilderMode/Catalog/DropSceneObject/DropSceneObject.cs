using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DropSceneObject : MonoBehaviour
{
    GameObject draggedObject;


    public void SceneObjectDropped(BaseEventData data)
    {
        Debug.Log("Droped on scene");
        //CatalogItemAdapter adapter = draggedObject.GetComponent<CatalogItemAdapter>();
        //SceneObject sceneObject = adapter.GetContent();
        //Texture texture = null;
        //if (adapter.thumbnailImg.enabled)
        //{
        //    texture = adapter.thumbnailImg.texture;
        //}
        //Destroy(draggedObject);
    }

}
