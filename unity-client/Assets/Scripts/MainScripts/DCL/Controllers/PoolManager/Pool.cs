using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DCL.Helpers;

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

        public System.Action<Pool> OnCleanup;

        public IPooledObjectInstantiator instantiator;

        private readonly LinkedList<PoolableObject> inactiveObjects = new LinkedList<PoolableObject>();
        private readonly LinkedList<PoolableObject> activeObjects = new LinkedList<PoolableObject>();
        private int maxPrewarmCount = 0;
        private bool initializing = true;

        public float lastGetTime
        {
            get;
            private set;
        }

        public int objectsCount => inactiveCount + activeCount;

        public int inactiveCount
        {
            get
            {
                return inactiveObjects.Count;
            }
        }

        public int activeCount
        {
            get { return activeObjects.Count; }
        }

        public Pool(string name, int maxPrewarmCount)
        {
            container = new GameObject("Pool - " + name);
            this.maxPrewarmCount = maxPrewarmCount;
            initializing = true;

#if UNITY_EDITOR
            Application.quitting += OnIsQuitting;
#endif

            if (RenderingController.i != null)
                RenderingController.i.OnRenderingStateChanged += OnRenderingStateChanged;

        }

        void OnRenderingStateChanged(bool renderingState)
        {
            if (renderingState)
            {
                if (RenderingController.i != null)
                    RenderingController.i.OnRenderingStateChanged -= OnRenderingStateChanged;

                initializing = false;
            }
        }

        public void ForcePrewarm()
        {
            for (int i = 0; i < maxPrewarmCount; i++)
                Instantiate();
        }

        public PoolableObject Get()
        {
            PoolableObject poolable = null;

            if (initializing || inactiveObjects.Count == 0)
            {
                if (RenderingController.i != null)
                {
                    if (!RenderingController.i.renderingEnabled)
                    {
                        int count = activeCount;

                        for (int i = inactiveCount; i < Mathf.Min(count * PREWARM_ACTIVE_MULTIPLIER, maxPrewarmCount); i++)
                        {
                            Instantiate();
                        }
                    }
                }
                Instantiate();
            }
            poolable = Extract();

            EnablePoolableObject(poolable);

            return poolable;
        }

        private PoolableObject Extract()
        {
            PoolableObject po = inactiveObjects.First.Value;
            inactiveObjects.RemoveFirst();
            po.node = activeObjects.AddFirst(po);

#if UNITY_EDITOR
            RefreshName();
#endif
            return po;
        }

        private void Return(PoolableObject po)
        {
            inactiveObjects.AddFirst(po);
            po.node.List.Remove(po.node);
            po.node = null;

#if UNITY_EDITOR
            RefreshName();
#endif
        }

        public PoolableObject Instantiate()
        {
            var gameObject = InstantiateAsOriginal();
            return SetupPoolableObject(gameObject);
        }

        public GameObject InstantiateAsOriginal()
        {
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
                return null;

            PoolableObject poolable = new PoolableObject();
            poolable.pool = this;
            poolable.gameObject = gameObject;
            PoolManager.i.poolables.Add(gameObject, poolable);

            if (!active)
            {
                DisablePoolableObject(poolable);
                inactiveObjects.AddFirst(poolable);
            }
            else
            {
                EnablePoolableObject(poolable);
                poolable.node = activeObjects.AddFirst(poolable);
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

            if (poolable == null || poolable.isInsidePool)
                return;

            DisablePoolableObject(poolable);
            Return(poolable);
        }

        public void ReleaseAll()
        {
            while (activeObjects.Count > 0)
            {
                activeObjects.First.Value.Release();
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
            if (inactiveObjects.Contains(poolable))
                inactiveObjects.Remove(poolable);

            if (activeObjects.Contains(poolable))
                activeObjects.Remove(poolable);

            PoolManager.i.poolables.Remove(poolable.gameObject);
#if UNITY_EDITOR
            RefreshName();
#endif
        }

        public void Cleanup()
        {
            ReleaseAll();

            while (inactiveObjects.Count > 0)
            {
                PoolManager.i.poolables.Remove(inactiveObjects.First.Value.gameObject);
                inactiveObjects.RemoveFirst();
            }

            while (activeObjects.Count > 0)
            {
                PoolManager.i.poolables.Remove(activeObjects.First.Value.gameObject);
                activeObjects.RemoveFirst();
            }

            inactiveObjects.Clear();
            activeObjects.Clear();

            Object.Destroy(this.original);
            Object.Destroy(this.container);

            OnCleanup?.Invoke(this);

            if (RenderingController.i != null)
                RenderingController.i.OnRenderingStateChanged -= OnRenderingStateChanged;
        }

        public void EnablePoolableObject(PoolableObject poolable)
        {
            if (poolable.gameObject != null)
            {
                poolable.gameObject.SetActive(true);
                poolable.gameObject.transform.SetParent(null);
            }

            lastGetTime = Time.unscaledTime;
        }

        public void DisablePoolableObject(PoolableObject poolable)
        {
#if UNITY_EDITOR
            if (isQuitting)
                return;
#endif
            if (poolable.gameObject != null)
            {
                poolable.gameObject.SetActive(false);

                if (container != null && container.transform != null)
                {
                    poolable.gameObject.transform.SetParent(container.transform);
                    poolable.gameObject.transform.ResetLocalTRS();
                }
            }
        }

        private void RefreshName()
        {
            if (this.container != null)
                this.container.name = $"in: {inactiveCount} out: {activeCount} id: {id}";
        }

        public static bool FindPoolInGameObject(GameObject gameObject, out Pool pool)
        {
            pool = null;

            if (PoolManager.i.poolables.TryGetValue(gameObject, out PoolableObject poolable))
            {
                if (poolable.pool.activeObjects.Contains(poolable))
                {
                    pool = poolable.pool;
                    return true;
                }

                if (poolable.pool.inactiveObjects.Contains(poolable))
                {
                    pool = poolable.pool;
                    return true;
                }
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
