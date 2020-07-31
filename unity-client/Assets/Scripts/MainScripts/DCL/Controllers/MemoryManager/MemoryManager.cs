using DCL.Controllers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_WEBGL && !UNITY_EDITOR
using Kongregate;
#endif

namespace DCL
{
    public class MemoryManager : Singleton<MemoryManager>
    {
        private const uint MAX_USED_MEMORY_MB = 1500 * 1024 * 1024;
        private const float TIME_BETWEEN_USED_MEMORY_CHECK = 0.1f;

        public void Initialize()
        {
            CoroutineStarter.Start(AutoCleanup());
        }

        public MemoryManager()
        {
            CommonScriptableObjects.rendererState.OnChange += (isEnable, prevState) =>
            {
                if (isEnable)
                {
                    ParcelScene.parcelScenesCleaner.ForceCleanup();
                    Resources.UnloadUnusedAssets();
                }
            };
        }

        bool NeedsMemoryCleanup()
        {
            var usedMemory = 0;

#if UNITY_WEBGL && !UNITY_EDITOR
            usedMemory = WebGLMemoryStats.GetUsedMemorySize();
#endif

            return usedMemory >= MAX_USED_MEMORY_MB;
        }

        IEnumerator AutoCleanup()
        {
            while (true)
            {
                if (NeedsMemoryCleanup())
                {
                    yield return CleanupPoolsIfNeeded();
                }

                yield return new WaitForSecondsRealtime(TIME_BETWEEN_USED_MEMORY_CHECK);
            }
        }

        private bool NeedsCleanup(Pool pool, bool forceCleanup = false)
        {
            if (forceCleanup)
                return true;

            if (pool.persistent)
                return false;

            return pool.usedObjectsCount == 0;
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
                }
            }
        }
    }
}