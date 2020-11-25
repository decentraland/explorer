using DCL;
using DCL.Components;
using DCL.Controllers;
using DCL.Interface;
using DCL.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
using static BuilderInWorldProtocol;

/// <summary>
/// This class will handle all the messages that will be sent to kernel. 
/// </summary>
public class BuilderInWorldBridge : MonoBehaviour
{

    //This is done for optimization purposes, recreating new objects can increase garbaje collection
    TransformComponent entityTransformComponentModel = new TransformComponent();

    StoreSceneStateEvent storeSceneState = new StoreSceneStateEvent();
    ModifyEntityComponentEvent modifyEntityComponentEvent = new ModifyEntityComponentEvent();
    EntityPayload entityPayload = new EntityPayload();
    EntitySingleComponentPayload entitySingleComponentPayload = new EntitySingleComponentPayload();

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
        List<ComponentPayload> list = new List<ComponentPayload>();
        foreach (KeyValuePair<CLASS_ID_COMPONENT, BaseComponent> keyValuePair in entity.components)
        {
            if (keyValuePair.Key == CLASS_ID_COMPONENT.TRANSFORM)
            {
                ComponentPayload componentPayLoad = new ComponentPayload();

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
                ComponentPayload componentPayLoad = new ComponentPayload();

                GLTFShapeComponent entityComponentModel = new GLTFShapeComponent();
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
        RemoveEntityPayload removeEntityPayLoad = new RemoveEntityPayload();
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

    void SendNewEntityToKernel(string sceneId, string entityId, ComponentPayload[] componentsPayload)
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
