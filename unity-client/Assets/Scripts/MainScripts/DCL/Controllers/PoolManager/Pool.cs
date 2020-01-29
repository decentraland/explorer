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

        private readonly LinkedList<PoolableObject> unusedObjects = new LinkedList<PoolableObject>();
        private readonly LinkedList<PoolableObject> usedObjects = new LinkedList<PoolableObject>();
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
                return unusedObjects.Count;
            }
        }

        public int activeCount
        {
            get { return usedObjects.Count; }
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

        void OnRenderingStateChanged(bool renderingEnabled)
        {
            initializing = !renderingEnabled;
        }

        public void ForcePrewarm()
        {
            for (int i = 0; i < maxPrewarmCount; i++)
                Instantiate();
        }

        public PoolableObject Get()
        {
            if (initializing)
            {
                int count = activeCount;

                for (int i = inactiveCount; i < Mathf.Min(count * PREWARM_ACTIVE_MULTIPLIER, maxPrewarmCount); i++)
                {
                    Instantiate();
                }
            }
            else if (unusedObjects.Count == 0)
            {
                Instantiate();
            }

            PoolableObject poolable = Extract();

            EnablePoolableObject(poolable);

            return poolable;
        }

        private PoolableObject Extract()
        {
            PoolableObject po = unusedObjects.First.Value;
            unusedObjects.RemoveFirst();
            po.node = usedObjects.AddFirst(po);

#if UNITY_EDITOR
            RefreshName();
#endif
            return po;
        }

        private void Return(PoolableObject po)
        {
            unusedObjects.AddFirst(po);
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
                unusedObjects.AddFirst(poolable);
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

            if (poolable == null || poolable.isInsidePool)
                return;

            DisablePoolableObject(poolable);
            Return(poolable);
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
            if (unusedObjects.Contains(poolable))
                unusedObjects.Remove(poolable);

            if (usedObjects.Contains(poolable))
                usedObjects.Remove(poolable);

            PoolManager.i.poolables.Remove(poolable.gameObject);
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
                unusedObjects.RemoveFirst();
            }

            while (usedObjects.Count > 0)
            {
                PoolManager.i.poolables.Remove(usedObjects.First.Value.gameObject);
                usedObjects.RemoveFirst();
            }

            unusedObjects.Clear();
            usedObjects.Clear();

            Object.Destroy(this.original);
            Object.Destroy(this.container);

            OnCleanup?.Invoke(this);

            if (RenderingController.i != null)
                RenderingController.i.OnRenderingStateChanged -= OnRenderingStateChanged;
        }

        public void EnablePoolableObject(PoolableObject poolable)
        {
            GameObject go = poolable.gameObject;

            if (go == null)
                return;

            if (!go.activeSelf)
                go.SetActive(true);

            if (go.transform.parent != null)
                go.transform.SetParent(null);

            lastGetTime = Time.unscaledTime;
        }

        public void DisablePoolableObject(PoolableObject poolable)
        {
#if UNITY_EDITOR
            if (isQuitting)
                return;
#endif
            if (poolable.gameObject == null)
                return;

            if (poolable.gameObject.activeSelf)
                poolable.gameObject.SetActive(false);
#if UNITY_EDITOR
            if (container != null)
            {
                poolable.gameObject.transform.SetParent(container.transform);
                poolable.gameObject.transform.ResetLocalTRS();
            }
#endif
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
