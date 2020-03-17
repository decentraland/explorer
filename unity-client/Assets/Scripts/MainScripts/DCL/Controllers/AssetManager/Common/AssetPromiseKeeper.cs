using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL
{
    public class AssetPromiseKeeper
    {
        public static float PROCESS_PROMISES_TIME_BUDGET = 0.006f;
    }
    /// <summary>
    /// The AssetPromiseKeeper is the user entry point interface.
    /// It manages stuff like requesting something that's already being loaded, etc.
    ///
    /// It also handles the special case of a promise that depends on another
    /// to be completed (blocked promise)
    /// </summary>
    /// <typeparam name="AssetType">Asset type to be handled</typeparam>
    /// <typeparam name="AssetLibraryType">Asset library type. It must handle the same type as AssetType.</typeparam>
    /// <typeparam name="AssetPromiseType">Asset promise type. It must handle the same type as AssetType.</typeparam>
    public class AssetPromiseKeeper<AssetType, AssetLibraryType, AssetPromiseType>
        where AssetType : Asset, new()
        where AssetLibraryType : AssetLibrary<AssetType>, new()
        where AssetPromiseType : AssetPromise<AssetType>
    {
        private static AssetPromiseKeeper<AssetType, AssetLibraryType, AssetPromiseType> instance;
        public static AssetPromiseKeeper<AssetType, AssetLibraryType, AssetPromiseType> i
        {
            get
            {
                if (instance == null)
                {
                    instance = new AssetPromiseKeeper<AssetType, AssetLibraryType, AssetPromiseType>(new AssetLibraryType());
                }

                return instance;
            }
        }

        public AssetLibraryType library;

        //NOTE(Brian): All waiting promises. Only used for cleanup and to keep count.
        List<AssetPromiseType> waitingPromises = new List<AssetPromiseType>(100);
        public int waitingPromisesCount => waitingPromises.Count;


        //NOTE(Brian): List of promises waiting for assets not in library.
        Dictionary<object, AssetPromiseType> masterPromiseById = new Dictionary<object, AssetPromiseType>(100);

        //NOTE(Brian): List of promises waiting for assets that are currently being loaded by another promise.
        List<AssetPromiseType> blockedPromises = new List<AssetPromiseType>(100);

        //NOTE(Brian): Master promise id -> blocked promises HashSet
        Dictionary<object, HashSet<AssetPromiseType>> masterToBlockedPromises = new Dictionary<object, HashSet<AssetPromiseType>>(100);

        public bool useTimeBudget { get { return CommonScriptableObjects.rendererState.Get(); } }
        float startTime;

        public bool IsBlocked(AssetPromiseType promise)
        {
            return blockedPromises.Contains(promise);
        }

        public string GetMasterState(AssetPromiseType promise)
        {
            object promiseId = promise.GetId();

            if (!masterToBlockedPromises.ContainsKey(promiseId))
                return "Master not found";

            if (!masterToBlockedPromises[promiseId].Contains(promise))
                return "Promise is not blocked???";

            if (!masterPromiseById.ContainsKey(promiseId))
                return "not registered as master?";

            return $"master state = {masterPromiseById[promiseId].state}";
        }

        public AssetPromiseKeeper(AssetLibraryType library)
        {
            this.library = library;
            CoroutineStarter.Start(ProcessBlockedPromisesQueue());
        }

        public AssetPromiseType Keep(AssetPromiseType promise)
        {
            if (promise == null || promise.state != AssetPromiseState.IDLE_AND_EMPTY || waitingPromises.Contains(promise))
                return promise;

            object id = promise.GetId();

            if (id == null)
            {
                Debug.LogError("ERROR: ID == null. Promise is not set up correctly.");
                return promise;
            }

            //NOTE(Brian): We already have a master promise for this id, add to blocked list.
            if (masterPromiseById.ContainsKey(id))
            {
                waitingPromises.Add(promise);

                if (!masterToBlockedPromises.ContainsKey(id))
                    masterToBlockedPromises.Add(id, new HashSet<AssetPromiseType>());

                masterToBlockedPromises[id].Add(promise);

                blockedPromises.Add(promise);
                promise.SetWaitingState();
                return promise;
            }

            // NOTE(Brian): Not in library, add to corresponding lists...
            if (!library.Contains(promise))
            {
                waitingPromises.Add(promise);
                masterPromiseById.Add(id, promise);
            }

            promise.library = library;
            promise.OnPreFinishEvent += OnRequestCompleted;
            promise.Load();

            return promise;
        }

        public AssetPromiseType Forget(AssetPromiseType promise)
        {
            if (promise == null)
                return null;

            if (promise.state == AssetPromiseState.IDLE_AND_EMPTY || promise.state == AssetPromiseState.WAITING)
            {
                CleanPromise(promise);
                return promise;
            }

            object id = promise.GetId();

            if (promise.state == AssetPromiseState.LOADING)
            {
                bool isMasterPromise = masterPromiseById.ContainsKey(id) && masterPromiseById[id] == promise;
                bool hasBlockedPromises = masterToBlockedPromises.ContainsKey(id) && masterToBlockedPromises[id].Count > 0;

                if (isMasterPromise && hasBlockedPromises)
                {
                    //NOTE(Brian): Pending promises are waiting for this one.
                    //             We clear the events because we shouldn't call them, as this promise is forgotten.
                    promise.ClearEvents();
                    OnSilentForget(promise);
                    return promise;
                }
            }

            promise.Unload();
            CleanPromise(promise);

            return promise;
        }

        Queue<AssetPromiseType> toResolveBlockedPromisesQueue = new Queue<AssetPromiseType>();

        private void OnRequestCompleted(AssetPromise<AssetType> loadedPromise)
        {
            if (!masterToBlockedPromises.ContainsKey(loadedPromise.GetId()))
            {
                CleanPromise(loadedPromise);
                return;
            }

            toResolveBlockedPromisesQueue.Enqueue(loadedPromise as AssetPromiseType);
        }

        IEnumerator ProcessBlockedPromisesQueue()
        {
            startTime = Time.unscaledTime;

            while (true)
            {
                if (toResolveBlockedPromisesQueue.Count <= 0)
                {
                    yield return null;
                    continue;
                }

                AssetPromiseType promise = toResolveBlockedPromisesQueue.Dequeue();
                yield return ProcessBlockedPromisesDeferred(promise);
                CleanPromise(promise);

                var enumerator = SkipFrameIfOverBudget();

                if (enumerator != null)
                    yield return enumerator;
            }
        }
        private IEnumerator ProcessBlockedPromisesDeferred(AssetPromiseType loadedPromise)
        {
            object loadedPromiseId = loadedPromise.GetId();

            if (!masterToBlockedPromises.ContainsKey(loadedPromiseId)
                || !masterPromiseById.ContainsKey(loadedPromiseId)
                || masterPromiseById[loadedPromiseId] != loadedPromise)
            {
                Debug.LogWarning($"Early exit for some reason for id {loadedPromiseId}");
                yield break;
            }


            if (loadedPromise.state != AssetPromiseState.FINISHED)
                yield return ForgetBlockedPromises(loadedPromiseId);
            else
                yield return LoadBlockedPromises(loadedPromiseId);

            if (masterToBlockedPromises.ContainsKey(loadedPromiseId))
                masterToBlockedPromises.Remove(loadedPromiseId);
        }


        private IEnumerator SkipFrameIfOverBudget()
        {
            if (useTimeBudget && Time.realtimeSinceStartup - startTime >= AssetPromiseKeeper.PROCESS_PROMISES_TIME_BUDGET)
            {
                yield return null;
                startTime = Time.unscaledTime;
            }
        }

        private IEnumerator ForgetBlockedPromises(object loadedPromiseId)
        {
            List<AssetPromiseType> blockedPromisesToForget = new List<AssetPromiseType>();

            using (var iterator = masterToBlockedPromises[loadedPromiseId].GetEnumerator())
            {
                while (iterator.MoveNext())
                {
                    var blockedPromise = iterator.Current;
                    blockedPromisesToForget.Add(blockedPromise);
                }
            }

            int blockedPromisesToForgetCount = blockedPromisesToForget.Count;

            for (int i = 0; i < blockedPromisesToForgetCount; i++)
            {
                var promise = blockedPromisesToForget[i];
                promise.ForceFail();
                Forget(promise);

                var enumerator = SkipFrameIfOverBudget();

                if (enumerator != null)
                    yield return enumerator;
            }
        }

        private List<AssetPromiseType> GetBlockedPromisesToLoadForId(object masterPromiseId)
        {
            var blockedPromisesToLoadAux = new List<AssetPromiseType>();

            using (var iterator = masterToBlockedPromises[masterPromiseId].GetEnumerator())
            {
                while (iterator.MoveNext())
                {
                    var blockedPromise = iterator.Current;

                    blockedPromisesToLoadAux.Add(blockedPromise);
                    blockedPromises.Remove(blockedPromise);
                }
            }

            return blockedPromisesToLoadAux;
        }

        private IEnumerator LoadBlockedPromises(object loadedPromiseId)
        {
            List<AssetPromiseType> blockedPromisesToLoad = GetBlockedPromisesToLoadForId(loadedPromiseId);

            var enumerator = SkipFrameIfOverBudget();

            if (enumerator != null)
                yield return enumerator;

            int blockedPromisesToLoadCount = blockedPromisesToLoad.Count;

            for (int i = 0; i < blockedPromisesToLoadCount; i++)
            {
                AssetPromiseType promise = blockedPromisesToLoad[i];
                promise.library = library;
                promise.OnPreFinishEvent += CleanPromise;
                promise.Load();

                enumerator = SkipFrameIfOverBudget();

                if (enumerator != null)
                    yield return enumerator;
            }
        }

        void CleanPromise(AssetPromise<AssetType> promise)
        {
            AssetPromiseType finalPromise = promise as AssetPromiseType;

            object id = promise.GetId();

            if (masterToBlockedPromises.ContainsKey(id))
            {
                if (masterToBlockedPromises[id].Contains(finalPromise))
                {
                    masterToBlockedPromises[id].Remove(finalPromise);
                }
            }

            if (masterPromiseById.ContainsKey(id) && masterPromiseById[id] == promise)
                masterPromiseById.Remove(id);

            if (blockedPromises.Contains(finalPromise))
                blockedPromises.Remove(finalPromise);

            if (waitingPromises.Contains(finalPromise))
                waitingPromises.Remove(finalPromise);
        }

        public void Cleanup()
        {
            blockedPromises = new List<AssetPromiseType>();
            masterToBlockedPromises = new Dictionary<object, HashSet<AssetPromiseType>>();

            int waitingPromisesCount = waitingPromises.Count;

            for (int i = 0; i < waitingPromisesCount; i++)
            {
                waitingPromises[i].Cleanup();
            }

            foreach (var kvp in masterPromiseById)
            {
                kvp.Value.Cleanup();
            }

            masterPromiseById = new Dictionary<object, AssetPromiseType>();
            waitingPromises = new List<AssetPromiseType>();
            library.Cleanup();
        }

        protected virtual void OnSilentForget(AssetPromiseType promise)
        {
        }
    }
}
