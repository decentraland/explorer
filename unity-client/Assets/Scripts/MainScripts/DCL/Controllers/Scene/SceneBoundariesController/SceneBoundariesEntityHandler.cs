using DCL.Helpers;
using DCL.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace DCL.Controllers
{
    public class SceneBoundariesEntityHandler
    {
        public bool DEBUG_MODE = false;
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

            public void ResetMaterials(DecentralandEntity.MeshesInfo meshesInfo)
            {
                this.meshesInfo = meshesInfo;
                ResetMaterials();
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
        const string INVALID_SUBMESH_MATERIAL_NAME = "Materials/InvalidSubMesh";

        Material invalidMeshMaterial;
        Material invalidSubMeshMaterial;
        Dictionary<GameObject, InvalidMeshInfo> invalidMeshesInfo = new Dictionary<GameObject, InvalidMeshInfo>();
        HashSet<Renderer> invalidSubmeshes = new HashSet<Renderer>();

        public SceneBoundariesEntityHandler()
        {
            invalidMeshesInfo = new Dictionary<GameObject, InvalidMeshInfo>();
            invalidMeshMaterial = Resources.Load(INVALID_MESH_MATERIAL_NAME) as Material;
            invalidSubMeshMaterial = Resources.Load(INVALID_SUBMESH_MATERIAL_NAME) as Material;
        }

        public void UpdateEntityMeshesValidState(DecentralandEntity entity, bool isInsideBoundaries)
        {
            if (entity.scene.IsEditModeActive() || DEBUG_MODE)
            {
                if (isInsideBoundaries)
                    RemoveInvalidMeshEffect(entity);
                else
                    AddInvalidMeshEffect(entity);
            }
            else
            {
                SetEntityMeshesVisibilityState(entity, isInsideBoundaries);
            }
        }
        void SetEntityMeshesVisibilityState(DecentralandEntity entity, bool isInsideBoundaries)
        {
            if (entity.meshesInfo.renderers[0] == null) return;

            if (isInsideBoundaries != entity.meshesInfo.renderers[0].enabled && entity.meshesInfo.currentShape.IsVisible())
            {
                for (int i = 0; i < entity.meshesInfo.renderers.Length; i++)
                {
                    if (entity.meshesInfo.renderers[i] != null)
                        entity.meshesInfo.renderers[i].enabled = isInsideBoundaries;
                }
            }
        }

        public void UpdateEntityCollidersValidState(DecentralandEntity entity, bool isInsideBoundaries)
        {
            int collidersCount = entity.meshesInfo.colliders.Count;
            if (collidersCount > 0 && isInsideBoundaries != entity.meshesInfo.colliders[0].enabled && entity.meshesInfo.currentShape.HasCollisions())
            {
                for (int i = 0; i < collidersCount; i++)
                {
                    if (entity.meshesInfo.colliders[i] != null)
                        entity.meshesInfo.colliders[i].enabled = isInsideBoundaries;
                }
            }
        }
        public void RemoveInvalidMeshEffect(DecentralandEntity entity)
        {
            if (WasEntityInAValidPosition(entity)) return;

            PoolableObject shapePoolableObjectBehaviour = PoolManager.i.GetPoolable(entity.meshesInfo.meshRootGameObject);
            if (shapePoolableObjectBehaviour != null)
                shapePoolableObjectBehaviour.OnRelease -= invalidMeshesInfo[entity.gameObject].ResetMaterials;

            for (int i = 0; i < entity.renderers.Length; i++)
            {
                if (invalidSubmeshes.Contains(entity.renderers[i]))
                    invalidSubmeshes.Remove(entity.renderers[i]);
            }

            invalidMeshesInfo[entity.gameObject].ResetMaterials();
        }

        void AddInvalidMeshEffect(DecentralandEntity entity)
        {
            if (!WasEntityInAValidPosition(entity)) return;

            InvalidMeshInfo invalidMeshInfo = new InvalidMeshInfo(entity.meshesInfo);

            invalidMeshInfo.OnResetMaterials = () => { invalidMeshesInfo.Remove(entity.gameObject); };

            PoolableObject shapePoolableObjectBehaviour = PoolManager.i.GetPoolable(entity.meshesInfo.meshRootGameObject);
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

                if (!invalidSubmeshes.Contains(entityRenderers[i]))
                {
                    // Wireframe that shows the boundaries to the dev (We don't use the GameObject.Instantiate(prefab, parent)
                    // overload because we need to set the position and scale before parenting, to deal with scaled objects)
                    GameObject wireframeObject = GameObject.Instantiate(Resources.Load<GameObject>(WIREFRAME_PREFAB_NAME));
                    wireframeObject.transform.position = entityRenderers[i].bounds.center;
                    wireframeObject.transform.localScale = entityRenderers[i].bounds.size * 1.01f;
                    wireframeObject.transform.SetParent(entity.gameObject.transform);

                    entityRenderers[i].sharedMaterial = invalidSubMeshMaterial;

                    invalidMeshInfo.wireframeObjects.Add(wireframeObject);
                    invalidSubmeshes.Add(entityRenderers[i]);
                }
                else
                {
                    entityRenderers[i].sharedMaterial = invalidMeshMaterial;
                }
            }

            invalidMeshesInfo.Add(entity.gameObject, invalidMeshInfo);
        }

        public bool WasEntityInAValidPosition(DecentralandEntity entity)
        {
            return !invalidMeshesInfo.ContainsKey(entity.gameObject);
        }

        public Dictionary<Renderer, Material> GetOriginalMaterials(DecentralandEntity entity)
        {
            if (invalidMeshesInfo.ContainsKey(entity.gameObject))
            {
                return invalidMeshesInfo[entity.gameObject].originalMaterials;
            }
            return null;
        }
    }
}
