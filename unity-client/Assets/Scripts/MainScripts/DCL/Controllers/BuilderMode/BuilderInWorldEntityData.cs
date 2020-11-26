using DCL.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BuilderInWorldEntityData 
{
    public string entityId;
    public TransformComponent transformComponent;
    public GLTFShapeComponent gltfShapeComponent;


    [System.Serializable]
    public class TransformComponent
    {
        public int componentId => (int)CLASS_ID_COMPONENT.TRANSFORM;
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;
    }

    [System.Serializable]
    public class GLTFShapeComponent
    {
        public int componentId => (int)CLASS_ID.GLTF_SHAPE;
        public string src;
        public string sharedId;
    }
}
