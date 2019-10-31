using UnityEngine;
using System;
using System.Collections;
using DCL.Models;
using DCL.Components;
using DCL.Helpers;

namespace Builder
{
    public class DCLBuilderEntity : MonoBehaviour
    {
        public static Action<DCLBuilderEntity> OnEntityShapeUpdated;
        public static Action<DCLBuilderEntity> OnEntityTransformUpdated;
        public static Action<DCLBuilderEntity> OnEntityAddedWithTransform;

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
        private Action onShapeLoaded;

        private bool isTransformComponentSet;
        private bool isShapeComponentSet;

        private Vector3 scaleTarget;
        private bool isScalingAnimation = false;

        public void SetEntity(DecentralandEntity entity)
        {
            rootEntity = entity;

            entity.OnShapeUpdated -= OnShapeUpdated;
            entity.OnShapeUpdated += OnShapeUpdated;

            entity.OnTransformChange -= OnTransformUpdated;
            entity.OnTransformChange += OnTransformUpdated;

            isTransformComponentSet = false;
            isShapeComponentSet = false;

            if (meshColliders != null)
            {
                for (int i = 0; i < meshColliders.Length; i++)
                {
                    Destroy(meshColliders[i].gameObject);
                }
                meshColliders = null;
            }

            if (HasShape())
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
            return true;
        }

        public void SetSelectLayer()
        {
            int selectionLayer = LayerMask.NameToLayer(DCLBuilderRaycast.LAYER_SELECTION);
            if (rootEntity.meshesInfo != null)
            {
                for (int i = 0; i < rootEntity.meshesInfo.renderers.Length; i++)
                {
                    if (rootEntity.meshesInfo.renderers[i])
                    {
                        rootEntity.meshesInfo.renderers[i].gameObject.layer = selectionLayer;
                    }
                }
            }
        }

        public void SetDefaultLayer()
        {
            int selectionLayer = 0;
            if (rootEntity.meshesInfo != null && rootEntity.meshesInfo.renderers != null)
            {
                for (int i = 0; i < rootEntity.meshesInfo.renderers.Length; i++)
                {
                    if (rootEntity.meshesInfo.renderers[i])
                    {
                        rootEntity.meshesInfo.renderers[i].gameObject.layer = selectionLayer;
                    }
                }
            }
        }

        public bool HasShape()
        {
            return isShapeComponentSet;
        }

        public bool HasRenderer()
        {
            return rootEntity.meshesInfo != null && rootEntity.meshesInfo.renderers != null;
        }

        public void SetOnShapeLoaded(Action onShapeLoad)
        {
            if (HasShape())
            {
                if (onShapeLoad != null) onShapeLoad();
            }
            else
            {
                onShapeLoaded = onShapeLoad;
            }
        }

        private void OnDestroy()
        {
            rootEntity.OnShapeUpdated -= OnShapeUpdated;
            rootEntity.OnTransformChange -= OnTransformUpdated;
        }

        private void OnShapeUpdated(DecentralandEntity entity)
        {
            isShapeComponentSet = true;
            OnEntityShapeUpdated?.Invoke(this);
            ProcessEntityShape(entity);

            if (hasGizmoComponent)
            {
                scaleTarget = DCLTransform.model.scale;
                StartCoroutine(ScaleAnimationRoutine(0.3f));
            }

            if (onShapeLoaded != null)
            {
                onShapeLoaded();
                onShapeLoaded = null;
            }
        }

        private void OnTransformUpdated(DCLTransform.Model transformModel)
        {
            gameObject.transform.localPosition = DCLTransform.model.position;
            gameObject.transform.localRotation = DCLTransform.model.rotation;

            if (isScalingAnimation)
            {
                scaleTarget = DCLTransform.model.scale;
            }
            else
            {
                gameObject.transform.localScale = DCLTransform.model.scale;
            }

            if (!isTransformComponentSet)
            {
                isTransformComponentSet = true;
                OnEntityAddedWithTransform?.Invoke(this);
            }

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

        private IEnumerator ScaleAnimationRoutine(float seconds)
        {
            float startingTime = Time.time;
            float normalizedTime = 0;
            Vector3 scale = Vector3.zero;

            gameObject.transform.localScale = scale;
            isScalingAnimation = true;

            while (Time.time - startingTime <= seconds)
            {
                normalizedTime = (Time.time - startingTime) / seconds;
                scale = Vector3.Lerp(scale, scaleTarget, normalizedTime);
                gameObject.transform.localScale = scale;
                yield return null;
            }
            gameObject.transform.localScale = scaleTarget;
            isScalingAnimation = false;
        }
    }
}