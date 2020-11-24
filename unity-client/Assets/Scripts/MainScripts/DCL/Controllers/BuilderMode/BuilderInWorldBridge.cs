using DCL;
using DCL.Components;
using DCL.Controllers;
using DCL.Interface;
using DCL.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

public class BuilderInWorldBridge : MonoBehaviour
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
    public class GenericComponent
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
    public class GTLShapeComponent : GenericComponent
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
    public class EntityTransformPayload<T>
    {
        public string entityId;
        public int componentId = (int)CLASS_ID_COMPONENT.TRANSFORM;
        public T data;
    }

    [System.Serializable]
    public class EntityPayLoad
    {
        public string entityId;
        public ComponentPayLoad[] components;
    }

    [System.Serializable]
    public class ComponentPayLoad
    {
        public int componentId;
        public GenericComponent data;
    }

    [System.Serializable]
    public class EntitySingleComponentPayLoad
    {
        public string entityId;
        public int componentId;
        public GenericComponent data;
    }

    [System.Serializable]
    public class RemoveEntityPayLoad
    {
        public string entityId;
    }

    [System.Serializable]
    public class RemoveEntityComponentsPayLoad
    {
        public string entityId;
        public string componentId;
    }

    [System.Serializable]
    public class AddEntityEvent
    {
        public string type = "AddEntity";
        public EntityPayLoad payload;
    }

    [System.Serializable]
    public class ModifyEntityComponentEvent
    {
        public string type = "SetComponent";
        public EntitySingleComponentPayLoad payload;
    }

    [System.Serializable]
    public class RemoveEntityEvent
    {
        public string type = "RemoveEntity";
        public RemoveEntityPayLoad payload;
    }

    [System.Serializable]
    public class RemoveEntityComponentsEvent
    {
        public string type = "RemoveComponent";
        public RemoveEntityComponentsPayLoad payload;
    }

    [System.Serializable]
    public class StoreSceneStateEvent
    {
        public string type = "StoreSceneState";
        public string payload = "";
    }

    #endregion

    //This is done for optimization purposes, recreating new objects can increase garbaje collection
    TransformComponent entityTransformComponentModel = new TransformComponent();

    StoreSceneStateEvent storeSceneState = new StoreSceneStateEvent();
    ModifyEntityComponentEvent modifyEntityComponentEvent = new ModifyEntityComponentEvent();
    EntityPayLoad entityPayload = new EntityPayLoad();
    EntitySingleComponentPayLoad entitySingleComponentPayload = new EntitySingleComponentPayLoad();

    public void ChangedEntityName(DCLBuilderInWorldEntity entity, ParcelScene scene)
    {
        entitySingleComponentPayload.entityId = entity.rootEntity.entityId;
        entitySingleComponentPayload.componentId = (int) CLASS_ID.NAME;

        NameComponent nameComponent = new NameComponent();
        nameComponent.value = entity.descriptiveName;

        entitySingleComponentPayload.data = nameComponent;

        modifyEntityComponentEvent.payload = entitySingleComponentPayload;

        WebInterface.SceneEvent<ModifyEntityComponentEvent> sceneEvent = new WebInterface.SceneEvent<ModifyEntityComponentEvent>();
        sceneEvent.sceneId = scene.sceneData.id;
        sceneEvent.eventType = "stateEvent";
        sceneEvent.payload = modifyEntityComponentEvent;

        string messasage = JsonConvert.SerializeObject(sceneEvent, Formatting.None, new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        });

        WebInterface.BuilderInWorldMessage("SceneEvent", messasage);
    }

    public void AddEntityOnKernel(DecentralandEntity entity, ParcelScene scene)
    {
        List<ComponentPayLoad> list = new List<ComponentPayLoad>();
        foreach (KeyValuePair<CLASS_ID_COMPONENT, BaseComponent> keyValuePair in entity.components)
        {
            if (keyValuePair.Key == CLASS_ID_COMPONENT.TRANSFORM)
            {
                ComponentPayLoad componentPayLoad = new ComponentPayLoad();

                componentPayLoad.componentId = (int) CLASS_ID_COMPONENT.TRANSFORM;
                TransformComponent entityComponentModel = new TransformComponent();

                entityComponentModel.position = SceneController.i.ConvertUnityToScenePosition(entity.gameObject.transform.position, scene);
                entityComponentModel.rotation = new QuaternionRepresentantion(entity.gameObject.transform.rotation);
                entityComponentModel.scale = entity.gameObject.transform.localScale;

                componentPayLoad.data = entityComponentModel;

                list.Add(componentPayLoad);

            }
        }

        foreach (KeyValuePair<Type, BaseDisposable> keyValuePair in entity.GetSharedComponents())
        {
            if (keyValuePair.Value is GLTFShape gtlfShape)
            {
                ComponentPayLoad componentPayLoad = new ComponentPayLoad();

                GTLShapeComponent entityComponentModel = new GTLShapeComponent();
                componentPayLoad.componentId = (int)CLASS_ID.GLTF_SHAPE;
                entityComponentModel.src = gtlfShape.model.src;
                componentPayLoad.data = entityComponentModel;

                list.Add(componentPayLoad);
            }
        }

        SendNewEntityToKernel(scene.sceneData.id, entity.entityId, list.ToArray());
    }

    public void EntityTransformReport(DecentralandEntity entity, ParcelScene scene)
    {
        entitySingleComponentPayload.entityId = entity.entityId;
        entitySingleComponentPayload.componentId = (int) CLASS_ID_COMPONENT.TRANSFORM;

        entityTransformComponentModel.position = SceneController.i.ConvertUnityToScenePosition(entity.gameObject.transform.position, scene);
        entityTransformComponentModel.rotation = new QuaternionRepresentantion(entity.gameObject.transform.rotation);
        entityTransformComponentModel.scale = entity.gameObject.transform.localScale;

        entitySingleComponentPayload.data = entityTransformComponentModel;

        modifyEntityComponentEvent.payload = entitySingleComponentPayload;

        WebInterface.SceneEvent<ModifyEntityComponentEvent> sceneEvent = new WebInterface.SceneEvent<ModifyEntityComponentEvent>();
        sceneEvent.sceneId = scene.sceneData.id;
        sceneEvent.eventType = "stateEvent";
        sceneEvent.payload = modifyEntityComponentEvent;

        string messasage = JsonConvert.SerializeObject(sceneEvent, Formatting.None, new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        });


        WebInterface.BuilderInWorldMessage("SceneEvent", messasage);
    }

    public void RemoveEntityOnKernel(string entityId, ParcelScene scene)
    {
        RemoveEntityEvent removeEntityEvent = new RemoveEntityEvent();
        RemoveEntityPayLoad removeEntityPayLoad = new RemoveEntityPayLoad();
        removeEntityPayLoad.entityId = entityId;
        removeEntityEvent.payload = removeEntityPayLoad;

        WebInterface.SendSceneEvent(scene.sceneData.id, "stateEvent", removeEntityEvent);
    }

    public void StartKernelEditMode(ParcelScene scene)
    {
        WebInterface.ReportControlEvent(new WebInterface.StartStatefulMode(scene.sceneData.id));
    }

    public void ExitKernelEditMode(ParcelScene scene)
    {
        WebInterface.ReportControlEvent(new WebInterface.StopStatefulMode(scene.sceneData.id));
    }

    public void PublishScene(ParcelScene scene)
    {
        WebInterface.SendSceneEvent(scene.sceneData.id, "stateEvent", storeSceneState);
    }

    void SendNewEntityToKernel(string sceneId, string entityId, ComponentPayLoad[] componentsPayload)
    {
        AddEntityEvent addEntityEvent = new AddEntityEvent();
        entityPayload.entityId = entityId;
        entityPayload.components = componentsPayload;

        addEntityEvent.payload = entityPayload;

        WebInterface.SceneEvent<AddEntityEvent> sceneEvent = new WebInterface.SceneEvent<AddEntityEvent>();
        sceneEvent.sceneId = sceneId;
        sceneEvent.eventType = "stateEvent";
        sceneEvent.payload = addEntityEvent;


        string messasage = Newtonsoft.Json.JsonConvert.SerializeObject(sceneEvent, Formatting.None, new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        });

        WebInterface.BuilderInWorldMessage("SceneEvent", messasage);
    }
}
