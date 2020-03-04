using DCL.Controllers;
using DCL.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL
{
    public class ParcelScenesCleaner
    {
        const float MAX_TIME_BUDGET = 0.01f;
        private struct ParcelEntity
        {
            public ParcelScene scene;
            public DecentralandEntity entity;

            public ParcelEntity(ParcelScene scene, DecentralandEntity entity)
            {
                this.scene = scene;
                this.entity = entity;
            }
        }

        Queue<DecentralandEntity> entitiesMarkedForCleanup = new Queue<DecentralandEntity>();
        Queue<ParcelEntity> rootEntitiesMarkedForCleanup = new Queue<ParcelEntity>();

        Coroutine removeEntitiesCoroutine;

        public void Start()
        {
            removeEntitiesCoroutine = CoroutineStarter.Start(CleanupEntitiesCoroutine());
        }

        public void Stop()
        {
            if (removeEntitiesCoroutine != null)
                CoroutineStarter.Stop(removeEntitiesCoroutine);
        }

        public void MarkForCleanup(DecentralandEntity entity)
        {
            if (entity.markedForCleanup)
                return;

            entity.markedForCleanup = true;

            if (entity.gameObject != null)
                entity.gameObject.SetActive(false);

#if UNITY_EDITOR
            if (entity.gameObject != null)
                entity.gameObject.name += "-MARKED-FOR-CLEANUP";
#endif

            entitiesMarkedForCleanup.Enqueue(entity);
        }

        // When removing all entities from a scene, we need to separate the root entities, as stated in ParcelScene,
        // to avoid traversing a lot of child entities in the same frame and other problems
        public void MarkRootEntityForCleanup(ParcelScene scene, DecentralandEntity entity)
        {
            rootEntitiesMarkedForCleanup.Enqueue(new ParcelEntity(scene, entity));
        }

        public void ForceCleanup()
        {
            ParcelScene scene = null;

            // If we have root entities queued for removal, we call Parcel Scene's RemoveEntity()
            // so that the child entities end up recursively in the entitiesMarkedForCleanup queue
            while (rootEntitiesMarkedForCleanup.Count > 0)
            {
                // If the next scene is different to the last one
                // we removed all the entities from the parcel scene
                if (scene != null && rootEntitiesMarkedForCleanup.Peek().scene != scene)
                    break;

                ParcelEntity parcelEntity = rootEntitiesMarkedForCleanup.Dequeue();

                scene = parcelEntity.scene;
                scene.RemoveEntity(parcelEntity.entity.entityId, false);
            }

            while (entitiesMarkedForCleanup.Count > 0)
            {
                DecentralandEntity entity = entitiesMarkedForCleanup.Dequeue();
                entity.SetParent(null);
                entity.Cleanup();
            }

            if (scene != null)
                GameObject.Destroy(scene.gameObject);

            PoolManager.i.CleanPoolableReferences();
        }

        IEnumerator CleanupEntitiesCoroutine()
        {
            while (true)
            {
                float lastTime = Time.unscaledTime;
                ParcelScene scene = null;

                // If we have root entities queued for removal, we call Parcel Scene's RemoveEntity()
                // so that the child entities end up recursively in the entitiesMarkedForCleanup queue
                while (rootEntitiesMarkedForCleanup.Count > 0)
                {
                    // If the next scene is different to the last one
                    // we removed all the entities from the parcel scene
                    if (scene != null && rootEntitiesMarkedForCleanup.Peek().scene != scene)
                        break;

                    ParcelEntity parcelEntity = rootEntitiesMarkedForCleanup.Dequeue();

                    scene = parcelEntity.scene;
                    scene.RemoveEntity(parcelEntity.entity.entityId, false);

                    if (DCLTime.realtimeSinceStartup - lastTime >= MAX_TIME_BUDGET)
                    {
                        yield return null;
                        lastTime = Time.unscaledTime;
                    }
                }

                while (entitiesMarkedForCleanup.Count > 0)
                {
                    DecentralandEntity entity = entitiesMarkedForCleanup.Dequeue();
                    entity.SetParent(null);
                    entity.Cleanup();

                    if (DCLTime.realtimeSinceStartup - lastTime >= MAX_TIME_BUDGET)
                    {
                        yield return null;
                        lastTime = Time.unscaledTime;
                    }
                }

                if (scene != null)
                    GameObject.Destroy(scene.gameObject);

                PoolManager.i.CleanPoolableReferences();

                yield return null;
            }

        }
    }
}
