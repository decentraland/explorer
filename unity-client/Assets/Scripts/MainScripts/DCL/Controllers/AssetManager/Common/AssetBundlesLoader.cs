using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DCL
{
    public class AssetBundlesLoader
    {
        private struct AssetBundleInfo
        {
            public Asset_AB asset;
            public AssetBundle assetBundle;
            public Action onSuccess;

            public AssetBundleInfo(Asset_AB asset, AssetBundle assetBundle, Action onSuccess)
            {
                this.asset = asset;
                this.assetBundle = assetBundle;
                this.onSuccess = onSuccess;
            }
        }

        private Coroutine assetBundlesLoadingCoroutine;
        private IOrderedEnumerable<string> assetsToLoad;
        private Queue<AssetBundleInfo> assetBundlesMarkedForLoad = new Queue<AssetBundleInfo>();
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

        private bool limitTimeBudget => CommonScriptableObjects.rendererState.Get();

        private const int SKIPPED_FRAMES_AFTER_BUDGET_TIME = 1;
        private float maxLoadBudgetTime = 0.004f;
        private float currentLoadBudgetTime = 0;

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
            assetBundlesMarkedForLoad.Clear();
            loadOrderByExtension.Clear();
            assetsToLoad.ToList().Clear();
        }

        public void MarkAssetBundleForLoad(Asset_AB asset, AssetBundle assetBundle, Action onSuccess)
        {
            assetBundlesMarkedForLoad.Enqueue(new AssetBundleInfo(asset, assetBundle, onSuccess));
        }

        private IEnumerator LoadAssetBundlesCoroutine()
        {
            while (true)
            {
                while (assetBundlesMarkedForLoad.Count > 0)
                {
                    AssetBundleInfo assetBundleInfo = assetBundlesMarkedForLoad.Dequeue();
                    yield return LoadAssetsInOrder(assetBundleInfo);
                }

                yield return null;
            }
        }

        private IEnumerator LoadAssetsInOrder(AssetBundleInfo assetBundleInfo)
        {
            float time = Time.realtimeSinceStartup;

            string[] assets = assetBundleInfo.assetBundle.GetAllAssetNames();

            float timeStart = Time.realtimeSinceStartup;
            float timeEnd = Time.realtimeSinceStartup;

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

                timeStart = Time.realtimeSinceStartup;
                //Debug.Log(string.Format("[SANTI LOG] loading asset = {0}", assetName));
                UnityEngine.Object loadedAsset = assetBundleInfo.assetBundle.LoadAsset(assetName);
                timeEnd = Time.realtimeSinceStartup - timeStart;
                //Debug.Log(string.Format("[SANTI LOG] asses loaded = {0} | TIME: {1}", assetName, timeEnd));

                if (loadedAsset is Material loadedMaterial)
                    loadedMaterial.shader = null;

                loadedAssetsByName.Add(loadedAsset);

                if (limitTimeBudget)
                {
                    currentLoadBudgetTime += Time.realtimeSinceStartup - time;
                    if (currentLoadBudgetTime > maxLoadBudgetTime)
                    {
                        for (int i = 0; i < SKIPPED_FRAMES_AFTER_BUDGET_TIME; i++)
                        {
                            yield return null;
                        }

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
    }
}