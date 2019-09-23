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

        public DecentralandEntity rootEntity { private set; get; }
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

        private MeshCollider[] meshColliders;

        public void SetEntity(DecentralandEntity entity)
        {
            rootEntity = entity;

            rootEntity.OnShapeUpdated -= OnShapeUpdated;
            rootEntity.OnShapeUpdated += OnShapeUpdated;

            rootEntity.OnTransformChange -= OnTransformUpdated;
            rootEntity.OnTransformChange += OnTransformUpdated;

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
            gameObject.layer = LayerMask.NameToLayer(OnPointerEventColliders.COLLIDER_LAYER);

            meshColliders = new MeshCollider[meshInfo.renderers.Length];
            for (int i = 0; i < meshInfo.renderers.Length; i++)
            {
                meshColliders[i] = gameObject.AddComponent<MeshCollider>();
                SetupMeshCollider(meshColliders[i], meshInfo.renderers[i]);
            }
        }

        private void SetupMeshCollider(MeshCollider meshCollider, Renderer renderer)
        {
            meshCollider.sharedMesh = renderer.GetComponent<MeshFilter>().sharedMesh;
            meshCollider.enabled = renderer.enabled;
        }
    }
}