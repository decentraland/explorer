using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public static partial class BuildModeUtils 
{
    public static void CopyGameObjectStatus(GameObject gameObjectToCopy, GameObject gameObjectToReceive, bool copyParent = true, bool localRotation = true)
    {
        if (copyParent) gameObjectToReceive.transform.SetParent(gameObjectToCopy.transform.parent);
        gameObjectToReceive.transform.position = gameObjectToCopy.transform.position;
        if (localRotation) gameObjectToReceive.transform.localRotation = gameObjectToCopy.transform.localRotation;
        else gameObjectToReceive.transform.rotation = gameObjectToCopy.transform.rotation;
        gameObjectToReceive.transform.localScale = gameObjectToCopy.transform.lossyScale;
    }

    public static bool IsPointerOverUIElement()
    {
        var eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        return results.Count > 1;
    }
}
