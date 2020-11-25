using DCL.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuilderInWorldProtocol 
{
    #region Class Declarations

    [System.Serializable]
    public class QuaternionRepresentantion
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public QuaternionRepresentantion(Quaternion quaternion)
        {

            x = quaternion.x;
            y = quaternion.y;
            z = quaternion.z;
            w = quaternion.w;
        }
    }

    #region Components

    [System.Serializable]
    public abstract class GenericComponent
    {

    }

    [System.Serializable]
    public class TransformComponent : GenericComponent
    {
        public Vector3 position;

        public QuaternionRepresentantion rotation;

        public Vector3 scale;

    }

    [System.Serializable]
    public class GLTFShapeComponent : GenericComponent
    {
        public string src;
    }

    [System.Serializable]
    public class NameComponent : GenericComponent
    {
        public string value;
    }

    #endregion

    [System.Serializable]
    public class EntityPayload
    {
        public string entityId;
        public ComponentPayload[] components;
    }

    [System.Serializable]
    public class ComponentPayload
    {
        public int componentId;
        public GenericComponent data;
    }

    [System.Serializable]
    public class EntitySingleComponentPayload
    {
        public string entityId;
        public int componentId;
        public GenericComponent data;
    }

    [System.Serializable]
    public class RemoveEntityPayload
    {
        public string entityId;
    }

    [System.Serializable]
    public class RemoveEntityComponentsPayload
    {
        public string entityId;
        public string componentId;
    }

    [System.Serializable]
    public class AddEntityEvent
    {
        public string type = "AddEntity";
        public EntityPayload payload;
    }

    [System.Serializable]
    public class ModifyEntityComponentEvent
    {
        public string type = "SetComponent";
        public EntitySingleComponentPayload payload;
    }

    [System.Serializable]
    public class RemoveEntityEvent
    {
        public string type = "RemoveEntity";
        public RemoveEntityPayload payload;
    }

    [System.Serializable]
    public class RemoveEntityComponentsEvent
    {
        public string type = "RemoveComponent";
        public RemoveEntityComponentsPayload payload;
    }

    [System.Serializable]
    public class StoreSceneStateEvent
    {
        public string type = "StoreSceneState";
        public string payload = "";
    }

    #endregion
}
