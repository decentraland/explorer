using DCL.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace DCL
{
    public class AssetPromise_AB : AssetPromise_WithUrl<Asset_AB>
    {
        public static bool VERBOSE = false;
        static float maxLoadBudgetTime = 0.032f;
        static float currentLoadBudgetTime = 0;
        public static bool limitTimeBudget = false;

        Coroutine loadCoroutine;

        static Dictionary<string, int> loadOrderByExtension = new Dictionary<string, int>()
        {
            { "png", 0 },
            { "jpg", 1 },
            { "peg", 2 },
            { "bmp", 3 },
            { "psd", 4 },
            { "iff", 5 },
            { "mat", 6 },
            { "nim", 7 },
            { "ltf", 8 },
            { "glb", 9 }
        };

        public AssetPromise_AB(string contentUrl, string hash) : base(contentUrl, hash)
        {
        }

        public AssetPromise_AB() 
        {}

        protected override bool AddToLibrary()
        {
            if (!library.Add(asset))
                return false;

            asset = library.Get(asset.id);
            return true;
        }

        internal override object GetId()
        {
            return hash;
        }

        protected override void OnCancelLoading()
        {
            if (loadCoroutine != null)
            {
                CoroutineStarter.Stop(loadCoroutine);
                loadCoroutine = null;
            }

            if (asset != null)
            {
                asset.CancelShow();
            }
        }

        protected override void OnAfterLoadOrReuse()
        {
        }

        protected override void OnBeforeLoadOrReuse()
        {
        }

        protected IEnumerator LoadAssetBundleWithDeps(string baseUrl, string hash, Action OnSuccess, Action OnFail)
        {
            yield return AssetBundleLoadHelper.GetDepMap(baseUrl, hash);

            if (AssetBundleLoadHelper.dependenciesMap.ContainsKey(hash))
            {
                foreach (string dep in AssetBundleLoadHelper.dependenciesMap[hash])
                {
                    var promise = new AssetPromise_AB(baseUrl, hash);
                    AssetPromiseKeeper_AB.i.Keep(promise);
                    yield return promise;
                }
            }

            yield return LoadAssetBundle(baseUrl + hash, OnSuccess, OnFail);
        }

        IEnumerator LoadAssetBundle(string finalUrl, Action OnSuccess, Action OnFail)
        {
            Debug.Log("req = " + finalUrl);
            using (UnityWebRequest assetBundleRequest = UnityWebRequestAssetBundle.GetAssetBundle(finalUrl))
            {
                yield return assetBundleRequest.SendWebRequest();

                if (!assetBundleRequest.WebRequestSucceded())
                {
                    OnFail?.Invoke();
                    yield break;
                }

                AssetBundle assetBundle = DownloadHandlerAssetBundle.GetContent(assetBundleRequest);

                if (assetBundle == null)
                {
                    OnFail?.Invoke();
                    yield break;
                }

                string[] assets = assetBundle.GetAllAssetNames();
                List<string> assetsToLoad = new List<string>();
                assetsToLoad = assets.OrderBy(
                    (x) =>
                    {
                        string ext = x.Substring(x.Length - 3);

                        if (loadOrderByExtension.ContainsKey(ext))
                            return loadOrderByExtension[ext];
                        else
                            return 99;
                    }).ToList();


                foreach (string assetName in assetsToLoad)
                {
                    float time = Time.realtimeSinceStartup;

#if UNITY_EDITOR
                    if (VERBOSE)
                        Debug.Log("loading asset = " + assetName);
#endif
                    string ext = assetName.Substring(assetName.Length - 3);

                    UnityEngine.Object loadedAsset = assetBundle.LoadAsset(assetName);

                    if (!asset.assetsByName.ContainsKey(assetName))
                        asset.assetsByName.Add(assetName, loadedAsset);

                    if (!asset.assetsByExtension.ContainsKey(ext))
                        asset.assetsByExtension.Add(ext, new List<UnityEngine.Object>());

                    asset.assetsByExtension[ext].Add(loadedAsset);

                    if (limitTimeBudget)
                    {
                        currentLoadBudgetTime += Time.realtimeSinceStartup - time;

                        if (currentLoadBudgetTime > maxLoadBudgetTime)
                        {
                            currentLoadBudgetTime = 0;
                            yield return null;
                        }
                    }
                }

                asset.ownerAssetBundle = assetBundle;
                asset.assetBundleAssetName = assetBundle.name;
                Debug.Log("Loading bundle... " + assetBundle.name);
            }

            OnSuccess?.Invoke();
        }

        protected override void OnLoad(Action OnSuccess, Action OnFail)
        {
            loadCoroutine = CoroutineStarter.Start(LoadAssetBundleWithDeps(contentUrl, hash, OnSuccess, OnFail));
        }
    }
}
