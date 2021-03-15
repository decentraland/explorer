using System.Collections.Generic;
using DCL.Components;
using DCL.Models;

namespace DCL.Controllers
{
    public static class SceneUtils
    {
        public static IDCLEntity DuplicateEntity(ParcelScene scene, IDCLEntity entity)
        {
            if (!scene.entities.ContainsKey(entity.entityId)) return null;

            IDCLEntity newEntity = scene.CreateEntity(System.Guid.NewGuid().ToString());

            if (entity.children.Count > 0)
            {
                using (var iterator = entity.children.GetEnumerator())
                {
                    while (iterator.MoveNext())
                    {
                        IDCLEntity childDuplicate = DuplicateEntity(scene, iterator.Current.Value);
                        childDuplicate.SetParent(newEntity);
                    }
                }
            }

            if (entity.parent != null)
                scene.SetEntityParent(newEntity.entityId, entity.parent.entityId);

            DCLTransform.model.position = WorldStateUtils.ConvertUnityToScenePosition(entity.gameObject.transform.position);
            DCLTransform.model.rotation = entity.gameObject.transform.rotation;
            DCLTransform.model.scale = entity.gameObject.transform.lossyScale;

            foreach (KeyValuePair<CLASS_ID_COMPONENT, IEntityComponent> component in entity.components)
            {
                scene.EntityComponentCreateOrUpdateFromUnity(newEntity.entityId, component.Key, DCLTransform.model);
            }

            foreach (KeyValuePair<System.Type, ISharedComponent> component in entity.sharedComponents)
            {
                ISharedComponent sharedComponent = scene.SharedComponentCreate(System.Guid.NewGuid().ToString(), component.Value.GetClassId());
                string jsonModel = Newtonsoft.Json.JsonConvert.SerializeObject(component.Value.GetModel());
                sharedComponent.UpdateFromJSON(jsonModel);
                scene.SharedComponentAttach(newEntity.entityId, sharedComponent.id);
            }

            //NOTE: (Adrian) Evaluate if all created components should be handle as equals instead of different
            // foreach (KeyValuePair<string, UUIDComponent> component in entity.uuidComponents)
            // {
            //     scene.EntityComponentCreateOrUpdateFromUnity(newEntity.entityId, CLASS_ID_COMPONENT.UUID_CALLBACK, component.Value.GetModel());
            // }

            return newEntity;
        }
    }
}