using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL
{
    public class MemoryManager : Singleton<MemoryManager>
    {
        private const float TIME_TO_POOL_CLEANUP = 60.0f;
        private const float MIN_TIME_BETWEEN_UNLOAD_ASSETS = 10.0f;
        private float lastTimeUnloadUnusedAssets = 0;
        public bool shouldCleanupPoolManager = false;

        public void Initialize()
        {
            CoroutineStarter.Start(AutoCleanup(), 10);
        }

        // TODO: here we'll define cleanup criteria
        bool NeedsMemoryCleanup()
        {
            return true;
        }

        IEnumerator AutoCleanup()
        {
            float timer = 0;

            while (true)
            {
                if (NeedsMemoryCleanup())
                {
                    yield return CleanupPoolsIfNeeded();
                }

                while (timer < 1.0f)
                {
                    yield return null;
                    timer += Time.unscaledDeltaTime;
                }

                timer = 0;
            }
        }

        private bool NeedsCleanup(Pool pool, bool forceCleanup = false)
        {
            if (forceCleanup)
                return true;

            bool timeout = DCLTime.realtimeSinceStartup - pool.lastGetTime >= TIME_TO_POOL_CLEANUP;
            return timeout && pool.usedObjectsCount == 0;
        }

        public IEnumerator CleanupPoolsIfNeeded(bool forceCleanup = false)
        {
            using (var iterator = PoolManager.i.pools.GetEnumerator())
            {
                List<object> idsToCleanup = new List<object>();

                while (iterator.MoveNext())
                {
                    Pool pool = iterator.Current.Value;

                    if (NeedsCleanup(pool, forceCleanup))
                    {
                        idsToCleanup.Add(pool.id);
                    }
                }

                int count = idsToCleanup.Count;

                if (count > 0)
                {
                    for (int i = 0; i < count; i++)
                    {
                        PoolManager.i.RemovePool(idsToCleanup[i]);
                        yield return null;
                    }

                    if (DCLTime.realtimeSinceStartup - lastTimeUnloadUnusedAssets >= MIN_TIME_BETWEEN_UNLOAD_ASSETS)
                    {
                        lastTimeUnloadUnusedAssets = DCLTime.realtimeSinceStartup;
                        Resources.UnloadUnusedAssets();

                        //if (shouldCleanupPoolManager)
                        //    PoolManager.i.CleanPoolableReferences();
                    }
                }
            }
        }
    }
}
