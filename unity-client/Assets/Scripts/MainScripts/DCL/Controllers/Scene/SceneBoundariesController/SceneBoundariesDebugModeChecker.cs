﻿using UnityEngine;
using System.Collections.Generic;
using DCL.Models;
using DCL.Helpers;

namespace DCL.Controllers
{
    public class SceneBoundariesDebugModeChecker : SceneBoundariesChecker
    {
        class InvalidMeshInfo
        {
            public Dictionary<Renderer, Material> originalMaterials = new Dictionary<Renderer, Material>();
            public List<GameObject> wireframeObjects = new List<GameObject>();
            public DecentralandEntity.MeshesInfo meshesInfo;
            public System.Action OnResetMaterials;

            public InvalidMeshInfo(DecentralandEntity.MeshesInfo meshesInfo)
            {
                this.meshesInfo = meshesInfo;
            }

            public void ResetMaterials()
            {
                if (meshesInfo.meshRootGameObject == null) return;

                for (int i = 0; i < meshesInfo.renderers.Length; i++)
                {
                    meshesInfo.renderers[i].sharedMaterial = originalMaterials[meshesInfo.renderers[i]];
                }

                int wireframeObjectscount = wireframeObjects.Count;
                for (int i = 0; i < wireframeObjectscount; i++)
                {
                    Utils.SafeDestroy(wireframeObjects[i]);
                }

                OnResetMaterials?.Invoke();
            }
        }

        const string WIREFRAME_PREFAB_NAME = "Prefabs/WireframeCubeMesh";
        const string INVALID_MESH_MATERIAL_NAME = "Materials/InvalidMesh";

        Material invalidMeshMaterial;
        Dictionary<GameObject, InvalidMeshInfo> invalidMeshesInfo = new Dictionary<GameObject, InvalidMeshInfo>();
        HashSet<Renderer> invalidSubmeshes = new HashSet<Renderer>();

        public SceneBoundariesDebugModeChecker(ParcelScene ownerScene) : base(ownerScene)
        {
            invalidMeshesInfo = new Dictionary<GameObject, InvalidMeshInfo>();
            invalidMeshMaterial = Resources.Load(INVALID_MESH_MATERIAL_NAME) as Material;
        }

        protected override void EvaluateMeshBounds(DecentralandEntity entity)
        {
            Bounds meshBounds = BuildMergedBounds(entity.meshesInfo);

            // 1st check (full mesh AABB)
            bool isInsideBoundaries = scene.IsInsideSceneBoundaries(meshBounds);

            // 2nd check (submeshes AABB)
            if (!isInsideBoundaries)
            {
                isInsideBoundaries = true;

                for (int i = 0; i < entity.meshesInfo.renderers.Length; i++)
                {
                    if (!scene.IsInsideSceneBoundaries(entity.meshesInfo.renderers[i].bounds))
                    {
                        isInsideBoundaries = false;

                        invalidSubmeshes.Add(entity.renderers[i]);
                    }
                }
            }

            UpdateEntityMeshesValidState(entity, isInsideBoundaries, meshBounds);

            UpdateEntityCollidersValidState(entity, isInsideBoundaries);
        }

        protected override void UpdateEntityMeshesValidState(DecentralandEntity entity, bool isInsideBoundaries, Bounds meshBounds)
        {
            if (isInsideBoundaries)
                RemoveInvalidMeshEffect(entity);
            else
                AddInvalidMeshEffect(entity, meshBounds);
        }

        void RemoveInvalidMeshEffect(DecentralandEntity entity)
        {
            if (WasEntityInAValidPosition(entity)) return;

            PoolableObject shapePoolableObjectBehaviour = entity.meshesInfo.meshRootGameObject.GetComponentInChildren<PoolableObject>();
            if (shapePoolableObjectBehaviour != null)
                shapePoolableObjectBehaviour.OnRelease -= invalidMeshesInfo[entity.gameObject].ResetMaterials;

            for (int i = 0; i < entity.renderers.Length; i++)
            {
                if (invalidSubmeshes.Contains(entity.renderers[i]))
                    invalidSubmeshes.Remove(entity.renderers[i]);
            }

            invalidMeshesInfo[entity.gameObject].ResetMaterials();
        }

        void AddInvalidMeshEffect(DecentralandEntity entity, Bounds meshBounds)
        {
            if (!WasEntityInAValidPosition(entity)) return;

            InvalidMeshInfo invalidMeshInfo = new InvalidMeshInfo(entity.meshesInfo);

            invalidMeshInfo.OnResetMaterials = () => { invalidMeshesInfo.Remove(entity.gameObject); };

            PoolableObject shapePoolableObjectBehaviour = entity.meshesInfo.meshRootGameObject.GetComponentInChildren<PoolableObject>();
            if (shapePoolableObjectBehaviour != null)
            {
                shapePoolableObjectBehaviour.OnRelease -= invalidMeshInfo.ResetMaterials;
                shapePoolableObjectBehaviour.OnRelease += invalidMeshInfo.ResetMaterials;
            }

            // Apply invalid material
            Renderer[] entityRenderers = entity.meshesInfo.renderers;
            for (int i = 0; i < entityRenderers.Length; i++)
            {
                // Save original materials
                invalidMeshInfo.originalMaterials.Add(entityRenderers[i], entityRenderers[i].sharedMaterial);

                entityRenderers[i].sharedMaterial = invalidMeshMaterial;

                if (invalidSubmeshes.Contains(entityRenderers[i]))
                {
                    // Wireframe that shows the boundaries to the dev (We don't use the GameObject.Instantiate(prefab, parent) 
                    // overload because we need to set the position and scale before parenting, to deal with scaled objects)
                    GameObject wireframeObject = GameObject.Instantiate(Resources.Load<GameObject>(WIREFRAME_PREFAB_NAME));
                    wireframeObject.transform.position = entityRenderers[i].bounds.center;
                    wireframeObject.transform.localScale = entityRenderers[i].bounds.size;
                    wireframeObject.transform.SetParent(entity.gameObject.transform);
                    invalidMeshInfo.wireframeObjects.Add(wireframeObject);
                }
            }

            invalidMeshesInfo.Add(entity.gameObject, invalidMeshInfo);
        }

        bool WasEntityInAValidPosition(DecentralandEntity entity)
        {
            return !invalidMeshesInfo.ContainsKey(entity.gameObject);
        }
    }
}