﻿using DCL.Components;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using System.Collections;
using UnityEngine;

namespace DCL
{
    public class UUIDComponent<ModelType> : UUIDComponent where ModelType : UUIDComponent.Model, new()
    {
        new public ModelType model
        {
            get { return base.model as ModelType; }
            set { base.model = value; }
        }
    }

    public class UUIDComponent : BaseComponent
    {
        [System.Serializable]
        public class Model
        {
            public string type;
            public string uuid;
        }

        public Model model = new Model();

        public virtual void Setup(ParcelScene scene, DecentralandEntity entity, string uuid, string type)
        {
        }

        public void SetForEntity(ParcelScene scene, DecentralandEntity entity, UUIDComponent.Model model)
        {
            SetForEntity(scene, entity, model.uuid, model.type);
        }

        public void SetForEntity(ParcelScene scene, DecentralandEntity entity, string uuid, string type)
        {
            switch (type)
            {
                case OnClickComponent.NAME:
                    SetUpComponent<OnClickComponent>(scene, entity, uuid, type);
                    return;
                case OnPointerDownComponent.NAME:
                    SetUpComponent<OnPointerDownComponent>(scene, entity, uuid, type);
                    return;
                case OnPointerUpComponent.NAME:
                    SetUpComponent<OnPointerUpComponent>(scene, entity, uuid, type);
                    return;
            }

            Debug.LogWarning($"Cannot create UUIDComponent of type '{type}'.");
        }

        public void RemoveFromEntity(DecentralandEntity entity, string type)
        {
            switch (type)
            {
                case OnClickComponent.NAME:
                    RemoveComponent<OnClickComponent>(entity);
                    break;
                case OnPointerDownComponent.NAME:
                    RemoveComponent<OnPointerDownComponent>(entity);
                    break;
                case OnPointerUpComponent.NAME:
                    RemoveComponent<OnPointerUpComponent>(entity);
                    break;
            }

            Debug.LogWarning($"Cannot remove UUIDComponent of type '{type}'.");
        }

        protected virtual void RemoveComponent<T>(DecentralandEntity entity) where T : UUIDComponent
        {
            var currentComponent = entity.gameObject.GetComponent<T>();

            if (currentComponent != null)
            {
#if UNITY_EDITOR
                UnityEngine.Object.DestroyImmediate(currentComponent);
#else
                UnityEngine.Object.Destroy(currentComponent);
#endif
            }
        }

        private void SetUpComponent<T>(ParcelScene scene, DecentralandEntity entity, string uuid, string type)
            where T : UUIDComponent
        {
            var currentComponent = entity.gameObject.GetOrCreateComponent<T>();

            currentComponent.Setup(scene, entity, uuid, type);
        }

        public override IEnumerator ApplyChanges(string newJson)
        {
            model = SceneController.i.SafeFromJson<Model>(newJson);

            if (!string.IsNullOrEmpty(model.uuid))
            {
                SetForEntity(scene, entity, model.uuid, model.type);
            }

            return null;
        }
    }
}
