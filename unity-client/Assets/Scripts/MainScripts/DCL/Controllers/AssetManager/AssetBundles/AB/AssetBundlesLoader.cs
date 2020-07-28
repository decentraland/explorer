using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DCL
{
    public class AssetBundlesLoader
    {
        public static event Action<KeyValuePair<int, int>> OnQueuesChanged;

        private const float MAX_LOAD_BUDGET_TIME = 0.004f;
        private const int SKIPPED_FRAMES_AFTER_BUDGET_TIME_IS_REACHED_FOR_NEARBY_ASSETS = 1;
        private const int SKIPPED_FRAMES_AFTER_BUDGET_TIME_IS_REACHED_FOR_DISTANT_ASSETS = 5;
        private const float MAX_SQR_DISTANCE_FOR_QUICK_LOADING = 6000f;
        private const float TIME_BETWEEN_REPRIORITIZATIONS = 1f;

        private struct AssetBundleInfo
        {
            public Asset_AB asset;
            public AssetBundle assetBundle;
            public Transform containerTransform;
            public Action onSuccess;

            public AssetBundleInfo(Asset_AB asset, AssetBundle assetBundle, Transform containerTransform, Action onSuccess)
            {
                this.asset = asset;
                this.assetBundle = assetBundle;
                this.containerTransform = containerTransform;
                this.onSuccess = onSuccess;
            }
        }

        private Coroutine assetBundlesLoadingCoroutine;
        private IOrderedEnumerable<string> assetsToLoad;
        private Queue<AssetBundleInfo> assetBundlesReadyToBeLoaded = new Queue<AssetBundleInfo>();
        private Queue<AssetBundleInfo> assetBundlesWaitingForLoad = new Queue<AssetBundleInfo>();
        private Dictionary<string, int> loadOrderByExtension = new Dictionary<string, int>()
        {
            {"png", 0},
            {"jpg", 1},
            {"peg", 2},
            {"bmp", 3},
            {"psd", 4},
            {"iff", 5},
            {"mat", 6},
            {"nim", 7},
            {"ltf", 8},
            {"glb", 9}
        };
        private List<UnityEngine.Object> loadedAssetsByName = new List<UnityEngine.Object>();
        private float currentLoadBudgetTime = 0;
        private AssetBundleInfo assetBundleInfoToLoad;
        private float lastQueuesReprioritizationTime = 0;

        private bool limitTimeBudget => CommonScriptableObjects.rendererState.Get();

        public void Start()
        {
            if (assetBundlesLoadingCoroutine != null)
                return;

            assetBundlesLoadingCoroutine = CoroutineStarter.Start(LoadAssetBundlesCoroutine());
        }

        public void Stop()
        {
            if (assetBundlesLoadingCoroutine == null)
                return;

            CoroutineStarter.Stop(assetBundlesLoadingCoroutine);
            assetBundlesReadyToBeLoaded.Clear();
            assetBundlesWaitingForLoad.Clear();
            assetsToLoad.ToList().Clear();
        }

        public void MarkAssetBundleForLoad(Asset_AB asset, AssetBundle assetBundle, Transform containerTransform, Action onSuccess)
        {
            CheckForReprioritizeAwaitingAssets();

            AssetBundleInfo assetBundleToLoad = new AssetBundleInfo(asset, assetBundle, containerTransform, onSuccess);

            float distanceFromPlayer = GetDistanceFromPlayer(containerTransform);
            if (distanceFromPlayer <= MAX_SQR_DISTANCE_FOR_QUICK_LOADING)
                assetBundlesReadyToBeLoaded.Enqueue(assetBundleToLoad);
            else
                assetBundlesWaitingForLoad.Enqueue(assetBundleToLoad);

            QueuesChanged();
        }

        private IEnumerator LoadAssetBundlesCoroutine()
        {
            while (true)
            {
                while (assetBundlesReadyToBeLoaded.Count > 0)
                {
                    assetBundleInfoToLoad = assetBundlesReadyToBeLoaded.Dequeue();
                    yield return LoadAssetsInOrder(assetBundleInfoToLoad, SKIPPED_FRAMES_AFTER_BUDGET_TIME_IS_REACHED_FOR_NEARBY_ASSETS);
                    QueuesChanged();
                }

                while (assetBundlesWaitingForLoad.Count > 0 && assetBundlesReadyToBeLoaded.Count == 0)
                {
                    assetBundleInfoToLoad = assetBundlesWaitingForLoad.Dequeue();
                    yield return LoadAssetsInOrder(assetBundleInfoToLoad, SKIPPED_FRAMES_AFTER_BUDGET_TIME_IS_REACHED_FOR_DISTANT_ASSETS);
                    QueuesChanged();
                }

                yield return null;
            }
        }

        private IEnumerator LoadAssetsInOrder(AssetBundleInfo assetBundleInfo, int skippedFramesBetweenLoadings)
        {
            float time = Time.realtimeSinceStartup;

            string[] assets = assetBundleInfo.assetBundle.GetAllAssetNames();

            assetsToLoad = assets.OrderBy(
                (x) =>
                {
                    string ext = x.Substring(x.Length - 3);

                    if (loadOrderByExtension.ContainsKey(ext))
                        return loadOrderByExtension[ext];
                    else
                        return 99;
                });

            foreach (string assetName in assetsToLoad)
            {
                if (assetBundleInfo.asset == null)
                    break;

                UnityEngine.Object loadedAsset = assetBundleInfo.assetBundle.LoadAsset(assetName);

                if (loadedAsset is Material loadedMaterial)
                    loadedMaterial.shader = null;

                loadedAssetsByName.Add(loadedAsset);

                if (limitTimeBudget)
                {
                    currentLoadBudgetTime += Time.realtimeSinceStartup - time;
                    if (currentLoadBudgetTime > MAX_LOAD_BUDGET_TIME)
                    {
                        yield return WaitForSkippedFrames(skippedFramesBetweenLoadings);

                        time = Time.realtimeSinceStartup;
                        currentLoadBudgetTime = 0f;
                    }
                }
            }

            foreach (var loadedAsset in loadedAssetsByName)
            {
                string ext = "any";

                if (loadedAsset is Texture)
                {
                    ext = "png";
                }
                else if (loadedAsset is Material)
                {
                    ext = "mat";
                }
                else if (loadedAsset is Animation || loadedAsset is AnimationClip)
                {
                    ext = "nim";
                }
                else if (loadedAsset is GameObject)
                {
                    ext = "glb";
                }

                if (!assetBundleInfo.asset.assetsByExtension.ContainsKey(ext))
                    assetBundleInfo.asset.assetsByExtension.Add(ext, new List<UnityEngine.Object>());

                assetBundleInfo.asset.assetsByExtension[ext].Add(loadedAsset);
            }

            assetsToLoad.ToList().Clear();
            loadedAssetsByName.Clear();
            assetBundleInfo.onSuccess?.Invoke();
        }

        private IEnumerator WaitForSkippedFrames(int skippedFramesBetweenLoadings)
        {
            for (int i = 0; i < skippedFramesBetweenLoadings; i++)
            {
                yield return null;
            }
        }

        private void CheckForReprioritizeAwaitingAssets()
        {
            if (assetBundlesWaitingForLoad.Count == 0 ||
                (Time.realtimeSinceStartup - lastQueuesReprioritizationTime) < TIME_BETWEEN_REPRIORITIZATIONS)
                return;

            while (assetBundlesWaitingForLoad.Count > 0 && GetDistanceFromPlayer(assetBundlesWaitingForLoad.Peek().containerTransform) <= MAX_SQR_DISTANCE_FOR_QUICK_LOADING)
            {
                assetBundlesReadyToBeLoaded.Enqueue(assetBundlesWaitingForLoad.Dequeue());
                lastQueuesReprioritizationTime = Time.realtimeSinceStartup;
            }
        }

        private float GetDistanceFromPlayer(Transform containerTransform)
        {
            return limitTimeBudget ? Vector3.SqrMagnitude(containerTransform.position - CommonScriptableObjects.playerUnityPosition.Get()) : 0f;
        }

        private void QueuesChanged()
        {
            OnQueuesChanged?.Invoke(new KeyValuePair<int, int>(assetBundlesReadyToBeLoaded.Count, assetBundlesWaitingForLoad.Count));
        }
    }
}