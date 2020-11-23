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
        public int componentId;
    }

    public class TransformComponent : GenericComponent
    {
        public Vector3 position;

        public QuaternionRepresentantion rotation;

        public Vector3 scale;

        public TransformComponent()
        {
            componentId = (int)CLASS_ID_COMPONENT.TRANSFORM;
        }
    }

    public class GTLShapeComponent : GenericComponent
    {
        public string src;

        public GTLShapeComponent()
        {
            componentId = (int)CLASS_ID.GLTF_SHAPE;
        }
    }

    public class NameComponent : GenericComponent
    {
        public string value;

        public NameComponent()
        {
            componentId = (int) CLASS_ID.NAME;
        }
    }

    #endregion

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
        public GenericComponent[] data;
    };

    [System.Serializable]
    public class EntitySingleComponentPayLoad
    {
        public string entityId;
        public GenericComponent data;
    };

    [System.Serializable]
    public class RemoveEntityPayLoad
    {
        public string entityId;
    };

    [System.Serializable]
    public class RemoveEntityComponentsPayLoad
    {
        public string entityId;
        public string componentId;
    };

    [System.Serializable]
    public class AddEntityEvent
    {
        public string type = "AddEntity";
        public EntityPayLoad payload;
    };

    [System.Serializable]
    public class ModifyEntityComponentEvent
    {
        public string type = "SetComponent";
        public EntitySingleComponentPayLoad payload;
    };

    [System.Serializable]
    public class RemoveEntityEvent
    {
        public string type = "RemoveEntity";
        public RemoveEntityPayLoad payload;
    };

    [System.Serializable]
    public class RemoveEntityComponentsEvent
    {
        public string type = "RemoveComponent";
        public RemoveEntityComponentsPayLoad payload;
    };

    [System.Serializable]
    public class StoreSceneStateEvent
    {
        public string type = "StoreSceneState";
        public string payload = "";
    };

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

        WebInterface.VERBOSE = true;
        WebInterface.BuilderInWorldMessage("SceneEvent", messasage);
        WebInterface.VERBOSE = false;
    }

    public void AddEntityOnKernel(DecentralandEntity entity, ParcelScene scene)
    {
        List<GenericComponent> list = new List<GenericComponent>();
        foreach (KeyValuePair<CLASS_ID_COMPONENT, BaseComponent> keyValuePair in entity.components)
        {
            if (keyValuePair.Key == CLASS_ID_COMPONENT.TRANSFORM)
            {
                TransformComponent entityComponentModel = new TransformComponent();
                //entityComponentModel.componentId = (int)CLASS_ID_COMPONENT.TRANSFORM;

                entityComponentModel.position = SceneController.i.ConvertUnityToScenePosition(entity.gameObject.transform.position, scene);
                entityComponentModel.rotation = new QuaternionRepresentantion(entity.gameObject.transform.rotation);
                entityComponentModel.scale = entity.gameObject.transform.localScale;


                DCLTransform.model.position = SceneController.i.ConvertUnityToScenePosition(entity.gameObject.transform.position, scene);
                DCLTransform.model.rotation = entity.gameObject.transform.rotation;
                DCLTransform.model.scale = entity.gameObject.transform.localScale;

                list.Add(entityComponentModel);

            }
        }

        foreach (KeyValuePair<Type, BaseDisposable> keyValuePair in entity.GetSharedComponents())
        {
            if (keyValuePair.Value is GLTFShape gtlfShape)
            {
                GTLShapeComponent entityComponentModel = new GTLShapeComponent();
                //entityComponentModel.componentId = (int)CLASS_ID.GLTF_SHAPE;

                entityComponentModel.src = gtlfShape.model.src;
                list.Add(entityComponentModel);
            }
        }

        SendNewEntityToKernel(scene.sceneData.id, entity.entityId, list.ToArray());
    }

    public void EntityTransformReport(DecentralandEntity entity, ParcelScene scene)
    {
        entitySingleComponentPayload.entityId = entity.entityId;

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

        WebInterface.VERBOSE = true;
        WebInterface.BuilderInWorldMessage("SceneEvent", messasage);
        WebInterface.VERBOSE = false;
    }

    public void RemoveEntityOnKernel(string entityId, ParcelScene scene)
    {
        RemoveEntityEvent removeEntityEvent = new RemoveEntityEvent();
        RemoveEntityPayLoad removeEntityPayLoad = new RemoveEntityPayLoad();
        removeEntityPayLoad.entityId = entityId;
        removeEntityEvent.payload = removeEntityPayLoad;

        WebInterface.VERBOSE = true;
        WebInterface.SendSceneEvent(scene.sceneData.id, "stateEvent", removeEntityEvent);
        WebInterface.VERBOSE = false;
    }

    public void StartKernelEditMode(ParcelScene scene)
    {
        WebInterface.VERBOSE = true;
        WebInterface.ReportControlEvent(new WebInterface.StartStatefulMode(scene.sceneData.id));
        WebInterface.VERBOSE = false;
    }

    public void ExitKernelEditMode(ParcelScene scene)
    {
        WebInterface.VERBOSE = true;
        WebInterface.ReportControlEvent(new WebInterface.StopStatefulMode(scene.sceneData.id));
        WebInterface.VERBOSE = false;
    }

    public void PublishScene(ParcelScene scene)
    {
        WebInterface.VERBOSE = true;
        WebInterface.SendSceneEvent(scene.sceneData.id, "stateEvent", storeSceneState);
        WebInterface.VERBOSE = false;
    }

    void SendNewEntityToKernel(string sceneId, string entityId, GenericComponent[] components)
    {
        AddEntityEvent addEntityEvent = new AddEntityEvent();
        entityPayload.entityId = entityId;
        entityPayload.data = components;

        addEntityEvent.payload = entityPayload;

        WebInterface.SceneEvent<AddEntityEvent> sceneEvent = new WebInterface.SceneEvent<AddEntityEvent>();
        sceneEvent.sceneId = sceneId;
        sceneEvent.eventType = "stateEvent";
        sceneEvent.payload = addEntityEvent;


        string messasage = Newtonsoft.Json.JsonConvert.SerializeObject(sceneEvent, Formatting.None, new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        });

        WebInterface.VERBOSE = true;
        WebInterface.BuilderInWorldMessage("SceneEvent", messasage);
        WebInterface.VERBOSE = false;
    }
}
