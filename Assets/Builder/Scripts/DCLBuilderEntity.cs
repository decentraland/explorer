using UnityEngine;
using System;
using DCL.Models;
using DCL.Components;
using DCL.Helpers;

namespace Builder
{
    public class DCLBuilderEntity : MonoBehaviour
    {
        public static Action<DCLBuilderEntity> OnEntityShapeUpdated;
        public static Action<DCLBuilderEntity> OnEntityTransformUpdated;

        public DecentralandEntity rootEntity { protected set; get; }
        public bool hasGizmoComponent
        {
            get
            {
                if (rootEntity != null)
                {
                    return rootEntity.components.ContainsKey(CLASS_ID_COMPONENT.GIZMOS);
                }
                else
                {
                    return false;
                }
            }
        }

        private DCLBuilderSelectionCollider[] meshColliders;

        public void SetEntity(DecentralandEntity entity)
        {
            rootEntity = entity;

            entity.OnShapeUpdated -= OnShapeUpdated;
            entity.OnShapeUpdated += OnShapeUpdated;

            entity.OnTransformChange -= OnTransformUpdated;
            entity.OnTransformChange += OnTransformUpdated;

            if (meshColliders != null)
            {
                for (int i = 0; i < meshColliders.Length; i++)
                {
                    Destroy(meshColliders[i]);
                }
                meshColliders = null;
            }

            if (entity.meshesInfo.currentShape != null)
            {
                OnShapeUpdated(entity);
            }
        }

        public bool IsInsideSceneBoundaries()
        {
            if (rootEntity != null && rootEntity.meshesInfo.renderers != null)
            {
                return rootEntity.scene.IsInsideSceneBoundaries(Utils.GetBoundsFromRenderers(rootEntity.meshesInfo.renderers));
            }
            return false;
        }

        public void SetSelectLayer()
        {
            int selectionLayer = LayerMask.NameToLayer(DCLBuilderRaycast.LAYER_SELECTION);
            if (rootEntity.meshesInfo != null)
            {
                for (int i = 0; i < rootEntity.meshesInfo.renderers.Length; i++)
                {
                    rootEntity.meshesInfo.renderers[i].gameObject.layer = selectionLayer;
                }
            }
        }

        public void SetDefaultLayer()
        {
            int selectionLayer = 0;
            if (rootEntity.meshesInfo != null)
            {
                for (int i = 0; i < rootEntity.meshesInfo.renderers.Length; i++)
                {
                    rootEntity.meshesInfo.renderers[i].gameObject.layer = selectionLayer;
                }
            }
        }

        private void OnDestroy()
        {
            rootEntity.OnShapeUpdated -= OnShapeUpdated;
            rootEntity.OnTransformChange -= OnTransformUpdated;
        }

        private void OnShapeUpdated(DecentralandEntity entity)
        {
            OnEntityShapeUpdated?.Invoke(this);
            ProcessEntityShape(entity);
        }

        private void OnTransformUpdated(DCLTransform.Model transformModel)
        {
            gameObject.transform.localPosition = DCLTransform.model.position;
            gameObject.transform.localRotation = DCLTransform.model.rotation;
            gameObject.transform.localScale = DCLTransform.model.scale;
            OnEntityTransformUpdated?.Invoke(this);
        }

        private void ProcessEntityShape(DecentralandEntity entity)
        {
            if (entity.meshRootGameObject && entity.meshesInfo.renderers.Length > 0 && hasGizmoComponent)
            {
                CreateColliders(entity.meshesInfo);
            }
        }

        private void CreateColliders(DecentralandEntity.MeshesInfo meshInfo)
        {
            meshColliders = new DCLBuilderSelectionCollider[meshInfo.renderers.Length];
            for (int i = 0; i < meshInfo.renderers.Length; i++)
            {
                meshColliders[i] = new GameObject("BuilderSelectionCollider").AddComponent<DCLBuilderSelectionCollider>();
                meshColliders[i].Initialize(this, meshInfo.renderers[i]);
            }
        }
    }
}