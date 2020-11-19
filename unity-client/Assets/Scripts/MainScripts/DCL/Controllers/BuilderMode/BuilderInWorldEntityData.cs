using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BuilderInWorldEntityData 
{
    public string entityId;
    public TransformComponent transformComponent;
    public GTLFShapeComponent gTLFShapeComponent;


    [System.Serializable]
    public class TransformComponent
    {
        public int componentId;
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;
    }

    [System.Serializable]
    public class GTLFShapeComponent
    {
        public int componentId;
        public string src;
        public string sharedId;
    }
}
