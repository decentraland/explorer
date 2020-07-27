using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DCL;
using DCL.Controllers;
using UnityEngine;

public class NativePayloads
{
    public class CreateEntity
    {
        public string entityId;
    }

    public class RemoveEntity
    {
        public string entityId;
    }
}

public class EntryPoint_World
{
    private SceneController sceneController;

    delegate void CreateEntityDelegate(string sceneId, string entityId);

    delegate void RemoveEntityDelegate(string sceneId, string entityId);

    delegate void SendSceneReadyDelegate(string sceneId);

    public EntryPoint_World(SceneController sceneController)
    {
        this.sceneController = sceneController;
        SetCallbacks(CreateEntity, RemoveEntity, SendSceneReady);
    }

    MessagingBus.QueuedSceneMessage_Scene GetSceneMessageInstance()
    {
        if (sceneController.sceneMessagesPool.Count > 0)
            return sceneController.sceneMessagesPool.Dequeue();

        return new MessagingBus.QueuedSceneMessage_Scene();
    }

    void CreateEntity(string sceneId, string entityId)
    {
        MessagingBus.QueuedSceneMessage_Scene queuedMessage = GetSceneMessageInstance();
        NativePayloads.CreateEntity payload = new NativePayloads.CreateEntity {entityId = entityId};

        queuedMessage.payload = payload;
        queuedMessage.sceneId = sceneId;
        queuedMessage.tag = sceneId;
        queuedMessage.type = MessagingBus.QueuedSceneMessage.Type.SCENE_MESSAGE;
        queuedMessage.method = MessagingTypes.ENTITY_CREATE;

        sceneController.EnqueueSceneMessage(queuedMessage);
    }

    void RemoveEntity(string sceneId, string entityId)
    {
        MessagingBus.QueuedSceneMessage_Scene queuedMessage = GetSceneMessageInstance();
        NativePayloads.RemoveEntity payload = new NativePayloads.RemoveEntity();

        payload.entityId = entityId;
        queuedMessage.payload = payload;
        queuedMessage.sceneId = sceneId;
        queuedMessage.tag = sceneId;
        queuedMessage.type = MessagingBus.QueuedSceneMessage.Type.SCENE_MESSAGE;
        queuedMessage.method = MessagingTypes.ENTITY_DESTROY;

        sceneController.EnqueueSceneMessage(queuedMessage);
    }

    void SendSceneReady(string sceneId)
    {
        MessagingBus.QueuedSceneMessage_Scene queuedMessage = GetSceneMessageInstance();
        queuedMessage.sceneId = sceneId;
        queuedMessage.type = MessagingBus.QueuedSceneMessage.Type.SCENE_MESSAGE;
        queuedMessage.method = MessagingTypes.INIT_DONE;

        sceneController.EnqueueSceneMessage(queuedMessage);
    }


    [DllImport("__Internal")]
    private static extern void SetCallbacks(
        CreateEntityDelegate CreateEntity,
        RemoveEntityDelegate RemoveEntity,
        SendSceneReadyDelegate SendSceneReady
    );
}