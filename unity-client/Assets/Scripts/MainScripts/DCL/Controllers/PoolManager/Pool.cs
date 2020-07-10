using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DCL.Helpers;
using UnityEngine.Assertions;

namespace DCL
{
    public interface IPooledObjectInstantiator
    {
        bool IsValid(GameObject original);
        GameObject Instantiate(GameObject gameObject);
    }

    public class Pool : ICleanable
    {
        public delegate void OnReleaseAllDlg(Pool pool);

        public const int PREWARM_ACTIVE_MULTIPLIER = 2;
        public object id;
        public GameObject original;
        public GameObject container;
        public bool persistent;

        public System.Action<Pool> OnCleanup;

        public IPooledObjectInstantiator instantiator;

        private readonly LinkedList<PoolableObject> unusedObjects = new LinkedList<PoolableObject>();
        private readonly LinkedList<PoolableObject> usedObjects = new LinkedList<PoolableObject>();
        private int maxPrewarmCount = 0;

        public float lastGetTime { get; private set; }

        public int objectsCount => unusedObjectsCount + usedObjectsCount;

        public int unusedObjectsCount
        {
            get { return unusedObjects.Count; }
        }

        public int usedObjectsCount
        {
            get { return usedObjects.Count; }
        }

        public Pool(string name, int maxPrewarmCount)
        {
#if UNITY_EDITOR
            Application.quitting += OnIsQuitting;
#endif
            if (PoolManager.USE_POOL_CONTAINERS)
                container = new GameObject("Pool - " + name);

            this.maxPrewarmCount = maxPrewarmCount;
        }

        public void ForcePrewarm()
        {
            for (int i = 0; i < maxPrewarmCount; i++)
                Instantiate();
        }

        public PoolableObject Get()
        {
            if (PoolManager.i.initializing)
            {
                int count = usedObjectsCount;

                for (int i = unusedObjectsCount; i < Mathf.Min(count * PREWARM_ACTIVE_MULTIPLIER, maxPrewarmCount); i++)
                {
                    Instantiate();
                }

                Instantiate();
            }
            else if (unusedObjects.Count == 0)
            {
                Instantiate();
            }

            PoolableObject poolable = Extract();

            EnablePoolableObject(poolable);
            poolable.Reset();
            return poolable;
        }

        private PoolableObject Extract()
        {
            PoolableObject po = null;
            po = unusedObjects.First.Value;
            unusedObjects.RemoveFirst();
            po.node = usedObjects.AddFirst(po);

#if UNITY_EDITOR
            RefreshName();
#endif
            return po;
        }

        public PoolableObject Instantiate()
        {
            var gameObject = InstantiateAsOriginal();
            return SetupPoolableObject(gameObject);
        }

        public GameObject InstantiateAsOriginal()
        {
            Assert.IsTrue(original != null, "Original should never be null here");

            GameObject gameObject = null;

            if (instantiator != null)
                gameObject = instantiator.Instantiate(original);
            else
                gameObject = GameObject.Instantiate(original);

            gameObject.SetActive(true);

            return gameObject;
        }

        private PoolableObject SetupPoolableObject(GameObject gameObject, bool active = false)
        {
            if (PoolManager.i.poolables.ContainsKey(gameObject))
                return PoolManager.i.GetPoolable(gameObject);

            PoolableObject poolable = new PoolableObject();
            poolable.pool = this;
            poolable.gameObject = gameObject;
            PoolManager.i.poolables.Add(gameObject, poolable);
            PoolManager.i.poolableValues.Add(poolable);

            if (!active)
            {
                DisablePoolableObject(poolable);
                poolable.node = unusedObjects.AddFirst(poolable);
            }
            else
            {
                EnablePoolableObject(poolable);
                poolable.node = usedObjects.AddFirst(poolable);
            }

#if UNITY_EDITOR
            RefreshName();
#endif
            return poolable;
        }

        public void Release(PoolableObject poolable)
        {
#if UNITY_EDITOR
            if (isQuitting)
                return;
#endif

            if (poolable == null || !PoolManager.i.HasPoolable(poolable))
                return;

            DisablePoolableObject(poolable);

            poolable.node.List.Remove(poolable.node);
            poolable.node = unusedObjects.AddFirst(poolable);

#if UNITY_EDITOR
            RefreshName();
#endif
        }

        public void ReleaseAll()
        {
            while (usedObjects.Count > 0)
            {
                usedObjects.First.Value.Release();
            }
        }

        /// <summary>
        /// This will add a gameObject that is not on any pool to this pool.
        /// </summary>
        /// <param name="gameObject"></param>
        public void AddToPool(GameObject gameObject, bool addActive = true)
        {
            if (instantiator != null && !instantiator.IsValid(gameObject))
            {
                Debug.LogError($"ERROR: Trying to add invalid gameObject to pool! -- {gameObject.name}", gameObject);
                return;
            }

            PoolableObject obj = PoolManager.i.GetPoolable(gameObject);

            if (obj != null)
            {
                Debug.LogError($"ERROR: gameObject is already being tracked by a pool! -- {gameObject.name}", gameObject);
                return;
            }

            SetupPoolableObject(gameObject, addActive);
        }

        public void RemoveFromPool(PoolableObject poolable)
        {
            poolable.node.List.Remove(poolable);
            poolable.node = null;

            PoolManager.i.poolables.Remove(poolable.gameObject);
            PoolManager.i.poolableValues.Remove(poolable);
#if UNITY_EDITOR
            RefreshName();
#endif
        }

        public void Cleanup()
        {
            ReleaseAll();

            while (unusedObjects.Count > 0)
            {
                PoolManager.i.poolables.Remove(unusedObjects.First.Value.gameObject);
                PoolManager.i.poolableValues.Remove(unusedObjects.First.Value);
                unusedObjects.RemoveFirst();
            }

            while (usedObjects.Count > 0)
            {
                PoolManager.i.poolables.Remove(usedObjects.First.Value.gameObject);
                PoolManager.i.poolableValues.Remove(usedObjects.First.Value);
                usedObjects.RemoveFirst();
            }

            unusedObjects.Clear();
            usedObjects.Clear();

            Object.Destroy(this.original);

            if (PoolManager.USE_POOL_CONTAINERS)
                Object.Destroy(this.container);

            OnCleanup?.Invoke(this);
        }

        public void EnablePoolableObject(PoolableObject poolable)
        {
            GameObject go = poolable.gameObject;

            if (go == null)
                return;

            if (!go.activeSelf)
                go.SetActive(true);

            go.transform.ResetLocalTRS();

            lastGetTime = Time.unscaledTime;
        }

        public void DisablePoolableObject(PoolableObject poolable)
        {
#if UNITY_EDITOR
            if (isQuitting)
                return;
#endif
            GameObject go = poolable.gameObject;

            if (go == null)
                return;

            if (go.activeSelf)
                go.SetActive(false);

            if (PoolManager.USE_POOL_CONTAINERS)
            {
                if (container != null)
                {
                    go.transform.SetParent(container.transform);
                }
            }
            else
            {
                go.transform.SetParent(null);
            }
        }

#if UNITY_EDITOR
        private void RefreshName()
        {
            if (this.container != null)
                this.container.name = $"in: {unusedObjectsCount} out: {usedObjectsCount} id: {id} persistent: {persistent}";
        }
#endif
        public static bool FindPoolInGameObject(GameObject gameObject, out Pool pool)
        {
            pool = null;

            if (PoolManager.i.poolables.TryGetValue(gameObject, out PoolableObject poolable))
            {
                pool = poolable.pool;
                return true;
            }

            return false;
        }

#if UNITY_EDITOR
        // In production it will always be false
        private bool isQuitting = false;

        // We need to check if application is quitting in editor
        // to prevent the pool from releasing objects that are
        // being destroyed 
        void OnIsQuitting()
        {
            Application.quitting -= OnIsQuitting;
            isQuitting = true;
        }
#endif
    }
};