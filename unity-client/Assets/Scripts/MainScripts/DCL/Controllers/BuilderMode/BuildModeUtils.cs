using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
}
