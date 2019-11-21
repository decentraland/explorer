using DCL.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace DCL
{
    public class AssetPromise_AssetBundle<T> : AssetPromise<T>
        where T : Asset_AssetBundle, new()
    {
        public static bool VERBOSE = false;
        static float maxLoadBudgetTime = 0.032f;
        static float currentLoadBudgetTime = 0;


        static Dictionary<string, int> loadOrderByExtension = new Dictionary<string, int>()
        {
            { "png", 0 },
            { "jpg", 1 },
            { "peg", 2 },
            { "bmp", 3 },
            { "psd", 4 },
            { "iff", 5 },
            { "mat", 6 },
            { "ltf", 7 },
            { "glb", 8 }
        };

        readonly protected string url;
        readonly protected string baseUrl;
        readonly protected ContentProvider provider = null;

        protected object id = null;

        public class Settings
        {
            public Shader shaderOverride;

            public Transform parent;
            public Vector3? initialLocalPosition;
            public Quaternion? initialLocalRotation;
            public Vector3? initialLocalScale;
            public bool forceNewInstance;
        }

        public Settings settings = new Settings();



        public AssetPromise_AssetBundle(ContentProvider provider, string baseUrl, string url)
        {
            this.provider = provider;
            this.url = url;
            this.baseUrl = baseUrl;
        }

        protected override void AddToLibrary()
        {
            library.Add(asset);
            asset = library.Get(asset.id);
            ApplySettings_LoadStart();
        }

        internal override object GetId()
        {
            if (id == null)
                id = ComputeId(provider, url);

            return id;
        }

        private string ComputeId(ContentProvider provider, string url)
        {
            if (provider.contents != null)
            {
                if (provider.TryGetContentsUrl_Raw(url, out string finalUrl))
                {
                    return finalUrl;
                }
            }

            return url;
        }

        protected override void OnCancelLoading()
        {
        }

        protected override void ApplySettings_LoadFinished()
        {
        }

        protected override void ApplySettings_LoadStart()
        {
        }

        IEnumerator LoadAssetBundleWithDeps(string baseUrl, string hash, Action OnSuccess, Action OnFail)
        {
            yield return AssetBundleLoadHelper.GetDepMap(baseUrl, hash);

            if (AssetBundleLoadHelper.dependenciesMap.ContainsKey(hash))
            {
                foreach (string dep in AssetBundleLoadHelper.dependenciesMap[hash])
                {
                    string finalUrl = baseUrl + dep;
                    var promise = new AssetPromise_AssetBundle(provider, baseUrl, finalUrl);
                    AssetPromiseKeeper_AssetBundle.i.Keep(promise);
                    yield return promise;
                }
            }

            yield return LoadAssetBundle("", OnSuccess, OnFail);
        }

        IEnumerator LoadAssetBundle(string finalUrl, Action OnSuccess, Action OnFail)
        {
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

                    if (!asset.assetsByName.ContainsKey(assetName))
                        asset.assetsByName.Add(assetName, assetBundle.LoadAsset(assetName));

                    //loadedAssets.Add(asset, assetBundle.LoadAsset(asset));
                    //#if UNITY_EDITOR
                    //                        if (VERBOSE)
                    //                        {
                    //                            if (asset.EndsWith("mat"))
                    //                            {
                    //                                Texture tex = (loadedAssets[asset] as Material).GetTexture("_BaseMap");

                    //                                if (tex != null)
                    //                                    Debug.Log("material has texture " + tex.name);
                    //                                else
                    //                                    Debug.Log("no texture!!!");
                    //                            }
                    //                        }
                    //#endif
                    //if (asset.EndsWith("glb") || asset.EndsWith("gltf"))
                    //{
                    //    if (!bundleToMainAssets.ContainsKey(assetBundle))
                    //        bundleToMainAssets.Add(assetBundle, new List<string>(1));

                    //    bundleToMainAssets[assetBundle].Add(asset);
                    //}

                    if (RenderingController.i.renderingEnabled)
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
            }
        }

        protected override void OnLoad(Action OnSuccess, Action OnFail)
        {
            string lowerCaseUrl = url.ToLower();
            string hash = provider.fileToHash[lowerCaseUrl];
            string finalUrl = baseUrl + hash;

            CoroutineStarter.Start(LoadAssetBundleWithDeps(baseUrl, hash, OnSuccess, OnFail));

            //using (UnityWebRequest assetBundleRequest = UnityWebRequestAssetBundle.GetAssetBundle(url))
            //{
            //    if (downloadingBundle.Contains(url))
            //    {
            //        yield return new WaitUntil(() => !downloadingBundle.Contains(url), 20);
            //        Debug.Log($"Waiting too long for {url}?");
            //        yield return new WaitUntil(() => !downloadingBundle.Contains(url));
            //        yield break;
            //    }

            //    downloadingBundle.Add(url);

            //    var asyncOp = assetBundleRequest.SendWebRequest();
            //    float progress = 0;

            //    while (!asyncOp.isDone)
            //    {
            //        if (VERBOSE)
            //        {
            //            if (asyncOp.progress != progress)
            //            {
            //                Debug.Log("Progress for " + url + " = " + asyncOp.progress);
            //                progress = asyncOp.progress;
            //            }
            //        }
            //        yield return null;
            //    }

            //    if (assetBundleRequest.isHttpError || assetBundleRequest.isNetworkError)
            //    {
            //        failedRequests.Add(url);
            //        downloadingBundle.Remove(url);
            //        Debug.LogWarning("AssetBundle request fail! " + url);
            //        yield break;
            //    }
        }
    }

    public class AssetPromise_AssetBundle : AssetPromise_AssetBundle<Asset_AssetBundle>
    {
        public AssetPromise_AssetBundle(ContentProvider provider, string baseUrl, string url) : base(provider, baseUrl, url)
        {
        }
    }
}
