using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace DCL
{
    public class PoolManager : Singleton<PoolManager>
    {
#if UNITY_EDITOR
        public static bool USE_POOL_CONTAINERS = true;
#else
        public static bool USE_POOL_CONTAINERS = false;
#endif

        public const int DEFAULT_PREWARM_COUNT = 100;
        public static bool enablePrewarm = true;
        public bool initializing { get; private set; }

        public Dictionary<object, Pool> pools = new Dictionary<object, Pool>();
        public Dictionary<GameObject, PoolableObject> poolables = new Dictionary<GameObject, PoolableObject>();
        public bool HasPoolable(PoolableObject poolable)
        {
            return poolables.ContainsValue(poolable);
        }

        public PoolableObject GetPoolable(GameObject gameObject)
        {
            if (poolables.ContainsKey(gameObject))
            {
                return poolables[gameObject];
            }

            return null;
        }

        GameObject container
        {
            get
            {
                EnsureContainer();
                return containerValue;
            }

            set
            {
                containerValue = value;
            }
        }

        GameObject containerValue = null;

        void EnsureContainer()
        {
            if (containerValue == null)
                containerValue = new GameObject("_PoolManager");
        }

        public PoolManager()
        {
            EnsureContainer();

            if (RenderingController.i != null)
            {
                initializing = !RenderingController.i.renderingEnabled;

                if (RenderingController.i != null)
                    RenderingController.i.OnRenderingStateChanged += OnRenderingStateChanged;
            }
            else
            {
                initializing = false;
            }
        }
        void OnRenderingStateChanged(bool renderingEnabled)
        {
            initializing = !renderingEnabled;
        }

        public Pool AddPool(object id, GameObject original, IPooledObjectInstantiator instantiator = null, int maxPrewarmCount = DEFAULT_PREWARM_COUNT)
        {
            if (ContainsPool(id))
            {
                if (Pool.FindPoolInGameObject(original, out Pool poolAlreadyExists))
                {
                    Debug.LogWarning("WARNING: Object is already being contained in an existing pool!. Returning it.");
                    return poolAlreadyExists;
                }

                Pool result = GetPool(id);

                result.AddToPool(original);

                return result;
            }

            if (!enablePrewarm)
                maxPrewarmCount = 0;

            Pool pool = new Pool(id.ToString(), maxPrewarmCount);

            pool.id = id;
            pool.original = original;

            if (USE_POOL_CONTAINERS)
            {
                pool.container.transform.parent = container.transform;
                pool.original.name = "Original";
                pool.original.transform.parent = pool.container.transform;
            }
            else
            {
                pool.original.transform.parent = null;
            }

            pool.original.SetActive(false);

            pool.instantiator = instantiator;

            pools.Add(id, pool);

            return pool;
        }


        public Pool GetPool(object id)
        {
            if (id == null)
            {
                Debug.LogError("GetPool >>> id cannot be null!");
                return null;
            }

            if (pools.ContainsKey(id))
                return pools[id];

            return null;
        }

        public void RemovePool(object id)
        {
            if (id == null)
            {
                Debug.LogError("RemovePool >>> id cannot be null!");
                return;
            }

            if (pools.ContainsKey(id))
            {
                Debug.Log("Will remove pool = " + id);
                pools[id].Cleanup();
                pools.Remove(id);
            }
        }

        public bool ContainsPool(object id)
        {
            if (id == null)
            {
                Debug.LogError("ContainsPool >>> id cannot be null!");
                return false;
            }

            return pools.ContainsKey(id);
        }

        public bool Release(GameObject gameObject)
        {
            if (gameObject == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning("Release >>> gameObject cannot be null!");
#endif
                return false;
            }



            if (poolables.TryGetValue(gameObject, out PoolableObject poolableObject))
            {
                poolableObject.Release();
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogWarning("Release >>> poolable not found in object, destroying it instead!");
#endif
                Object.Destroy(gameObject);
                return false;
            }

            return true;
        }

        public PoolableObject Get(object id)
        {
            if (id == null)
            {
                Debug.LogError("Get >>> id cannot be null!");
                return null;
            }

            Pool pool;

            if (pools.ContainsKey(id))
            {
                pool = pools[id];
            }
            else
            {
                Debug.LogError($"Pool doesn't exist for id {id}!");
                return null;
            }

            return pool.Get();
        }

        public void Cleanup()
        {
            if (pools == null)
                return;

            List<object> idsToRemove = new List<object>(10);

            using (var iterator = pools.GetEnumerator())
            {
                while (iterator.MoveNext())
                {
                    idsToRemove.Add(iterator.Current.Key);
                }
            }

            for (int i = 0; i < idsToRemove.Count; i++)
            {
                RemovePool(idsToRemove[i]);
            }

            if (RenderingController.i != null)
                RenderingController.i.OnRenderingStateChanged -= OnRenderingStateChanged;
        }

        public void ReleaseAllFromPool(object id)
        {
            if (pools.ContainsKey(id))
            {
                pools[id].ReleaseAll();
            }
        }

        List<GameObject> toRemoveAuxList = new List<GameObject>();
        public void CleanPoolableReferences()
        {
            toRemoveAuxList.Clear();

            using (var it = poolables.GetEnumerator())
            {
                while (it.MoveNext())
                {
                    var kvp = it.Current;

                    if (kvp.Value.gameObject == null)
                    {
                        kvp.Value.node?.List.Remove(kvp.Value);
                        kvp.Value.node = null;
                        toRemoveAuxList.Add(kvp.Key);
                    }
                }
            }

            for (int i = 0; i < toRemoveAuxList.Count; i++)
            {
                GameObject key = toRemoveAuxList[i];
                poolables.Remove(key);
            }
        }
    }
}
