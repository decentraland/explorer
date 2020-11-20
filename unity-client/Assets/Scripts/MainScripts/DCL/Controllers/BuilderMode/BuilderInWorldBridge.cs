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

    public void AddEntityOnKernel(DecentralandEntity entity,ParcelScene scene)
    {
        List<WebInterface.EntityComponentModel> list = new List<WebInterface.EntityComponentModel>();
        foreach (KeyValuePair<CLASS_ID_COMPONENT, BaseComponent> keyValuePair in entity.components)
        {
            if (keyValuePair.Key == CLASS_ID_COMPONENT.TRANSFORM)
            {
                WebInterface.EntityComponentModel entityComponentModel = new WebInterface.EntityComponentModel();
                entityComponentModel.id = (int)CLASS_ID_COMPONENT.TRANSFORM;
                DCLTransform.model.position = SceneController.i.ConvertUnityToScenePosition(entity.gameObject.transform.position, scene);
                DCLTransform.model.rotation = entity.gameObject.transform.rotation;
                DCLTransform.model.scale = entity.gameObject.transform.localScale;

                entityComponentModel.data = "{\"position\":" + JsonUtility.ToJson(DCLTransform.model.position) + "}"+
                                            //"{\"rotation\":" + JsonUtility.ToJson(DCLTransform.model.rotation.eulerAngles) + "}"+
                                            "{\"scale\":" + JsonUtility.ToJson(DCLTransform.model.scale) + "}";
                //entityComponentModel.data = JsonConvert.SerializeObject(DCLTransform.model);
                list.Add(entityComponentModel);

            }
        }

        foreach (KeyValuePair<Type, BaseDisposable> keyValuePair in entity.GetSharedComponents())
        {
            if (keyValuePair.Value is GLTFShape gtlfShape)
            {
                WebInterface.EntityComponentModel entityComponentModel = new WebInterface.EntityComponentModel();
                entityComponentModel.id = (int)CLASS_ID.GLTF_SHAPE;
                //entityComponentModel.data = "\"src\": \"" + gLTFShape.model.src + "\"";
                entityComponentModel.data = JsonConvert.SerializeObject(gtlfShape.model);
                list.Add(entityComponentModel);
            }
        }

        WebInterface.VERBOSE = true;
        WebInterface.AddEntity(scene.sceneData.id, entity.entityId, list.ToArray());
        WebInterface.VERBOSE = false;
    }

    public void EntityTransformReport(DecentralandEntity entity, ParcelScene scene)
    {

        WebInterface.EntityComponentModel entityComponentModel = new WebInterface.EntityComponentModel();
        entityComponentModel.id = (int)CLASS_ID_COMPONENT.TRANSFORM;
        DCLTransform.model.position = SceneController.i.ConvertUnityToScenePosition(entity.gameObject.transform.position, scene);
        DCLTransform.model.rotation = entity.gameObject.transform.rotation;
        DCLTransform.model.scale = entity.gameObject.transform.localScale;
        entityComponentModel.data = "{\"position\":" + JsonUtility.ToJson(DCLTransform.model.position) + "}" +
                                    "{\"rotation\":" + JsonUtility.ToJson(DCLTransform.model.rotation.eulerAngles) + "}" +
                                    "{\"scale\":" + JsonUtility.ToJson(DCLTransform.model.scale) + "}";


        WebInterface.VERBOSE = true;
        WebInterface.ReportEntityTransform(DCLTransform.model.position, DCLTransform.model.rotation, DCLTransform.model.scale, scene.sceneData.id, entity.entityId);
        WebInterface.VERBOSE = false;
    }

    public void RemoveEntityOnKernel(string entityId,ParcelScene scene)
    {
        WebInterface.VERBOSE = true;
        WebInterface.RemoveEntity(scene.sceneData.id, entityId);
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
        WebInterface.ReportStoreSceneState(scene.sceneData.id);
        WebInterface.VERBOSE = false;
    }

}
