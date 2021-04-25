using System;
using DCL.Configuration;
using DCL.Helpers;
using DCL.Models;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DCL.Components
{
    public class OnPointerEventColliders : IDisposable
    {
        public const string COLLIDER_NAME = "OnPointerEventCollider";

        Collider[] colliders;
        Dictionary<Collider, string> colliderNames = new Dictionary<Collider, string>();

        public string GetMeshName(Collider collider)
        {
            if (colliderNames.ContainsKey(collider))
                return colliderNames[collider];

            return null;
        }

        private IDCLEntity ownerEntity;
        private IShape lastShape;

        public void Initialize(IDCLEntity entity)
        {
            Renderer[] rendererList = entity?.meshesInfo?.renderers;

            if (rendererList == null || rendererList.Length == 0)
                return;

            IShape shape = entity.meshesInfo.currentShape;
            
            this.ownerEntity = entity;
            lastShape = shape;

            DestroyColliders();

            if (shape == null)
                return;

            colliders = new Collider[rendererList.Length];

            for (int i = 0; i < colliders.Length; i++)
            {
                colliders[i] = CreateColliders(rendererList[i]);
            }
        }

        Collider CreateColliders(Renderer renderer)
        {
            // Get closest mesh collider child
            var meshCollider = renderer.GetComponentsInChildren<MeshCollider>(true)?.FirstOrDefault(x => x.gameObject.layer == PhysicsLayers.onPointerEventLayer);
            GameObject colliderGo = meshCollider?.gameObject;
            
            if (meshCollider == null)
            {
                colliderGo = new GameObject(COLLIDER_NAME);
                colliderGo.layer = PhysicsLayers.onPointerEventLayer; // to avoid character collisions with onclick collider

                meshCollider = colliderGo.AddComponent<MeshCollider>();
            }
            
            if (!CollidersManager.i.GetColliderInfo(meshCollider, out ColliderInfo info))
                CollidersManager.i.AddOrUpdateEntityCollider(ownerEntity, meshCollider);
            
            meshCollider.sharedMesh = renderer.GetComponent<MeshFilter>().sharedMesh;
            meshCollider.enabled = renderer.enabled;

            if (renderer.transform.parent != null && !colliderNames.ContainsKey(meshCollider))
                colliderNames.Add(meshCollider, renderer.transform.parent.name);
            
            colliderGo.name = COLLIDER_NAME;
            
            // Reset objects position, rotation and scale once it's been parented
            Transform t = colliderGo.transform;

            t.SetParent(renderer.transform);
            t.ResetLocalTRS();

            return meshCollider;
        }

        public void Dispose() { DestroyColliders(); }

        void DestroyColliders()
        {
            if (colliders == null)
                return;

            for (int i = 0; i < colliders.Length; i++)
            {
                Collider collider = colliders[i];

                if (collider != null)
                    UnityEngine.Object.Destroy(collider.gameObject);
            }
        }
    }
}