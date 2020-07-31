using DCL.Controllers;
using System;
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
        private const float TIME_TO_CHECK_MEMORY_USE = 0.1f;
        private const float FREE_MEMORY_PERCENTAGE_LIMIT = 10.0f;

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
                    MemoryManager.i.CleanupPoolsIfNeeded();
                    ParcelScene.parcelScenesCleaner.ForceCleanup();
                    Resources.UnloadUnusedAssets();
                }
                else
                {
                    GC.Collect();
                }
            };
        }

        bool NeedsMemoryCleanup()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            float freeMemory = WebGLMemoryStats.GetFreeMemorySize() * 100f / WebGLMemoryStats.GetTotalMemorySize();
#else
            float freeMemory = 0f;
#endif

            return freeMemory <= FREE_MEMORY_PERCENTAGE_LIMIT;
        }

        IEnumerator AutoCleanup()
        {
            while (true)
            {
                if (NeedsMemoryCleanup())
                {
                    yield return CleanupPoolsIfNeeded();
                }

                yield return new WaitForSecondsRealtime(TIME_TO_CHECK_MEMORY_USE);
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