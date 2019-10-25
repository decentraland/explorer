using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Networking;
using System.Linq;
using DCL.Helpers;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DCL.Components
{
    public class LoadWrapper_GLTF : LoadWrapper
    {
        static bool VERBOSE = false;

        public GameObject gltfContainer;

        AssetPromise_GLTF gltfPromise;

        string assetDirectoryPath;


#if UNITY_EDITOR
        [ContextMenu("Debug Load Count")]
        public void DebugLoadCount()
        {
            Debug.Log($"promise state = {gltfPromise.state} ... waiting promises = {AssetPromiseKeeper_GLTF.i.waitingPromisesCount}");
        }
#endif

        public override void Load(string targetUrl, Action<LoadWrapper> OnSuccess, Action<LoadWrapper> OnFail)
        {
            Assert.IsFalse(string.IsNullOrEmpty(targetUrl), "url is null!!");

            StartCoroutine(TryToFetchAssetBundle(targetUrl, OnSuccess, OnFail));
        }

        static Dictionary<string, AssetBundle> cachedBundles = new Dictionary<string, AssetBundle>();

        static Dictionary<string, List<string>> dependenciesMap = new Dictionary<string, List<string>>();
        static HashSet<string> processedManifests = new HashSet<string>();
        static HashSet<string> failedRequests = new HashSet<string>();
        static List<string> downloadingBundle = new List<string>();

        UnityWebRequest currentRequest;

        static List<UnityEngine.Object> allLoadedAssets = new List<UnityEngine.Object>();

        IEnumerator GetAssetBundle(string url)
        {
            if (failedRequests.Contains(url) || cachedBundles.ContainsKey(url))
                yield break;

            UnityWebRequest assetBundleRequest = UnityWebRequestAssetBundle.GetAssetBundle(url);
            currentRequest = assetBundleRequest;

            if (downloadingBundle.Contains(url))
            {
                yield return new WaitUntil(() => !downloadingBundle.Contains(url));
                yield break;
            }

            downloadingBundle.Add(url);

            yield return assetBundleRequest.SendWebRequest();

            if (assetBundleRequest.isHttpError || assetBundleRequest.isNetworkError)
            {
                failedRequests.Add(url);
                downloadingBundle.Remove(url);
                Debug.Log("fail! " + url);
                yield break;
            }

            if (!cachedBundles.ContainsKey(url))
            {
                AssetBundle assetBundle = DownloadHandlerAssetBundle.GetContent(assetBundleRequest);

                if (assetBundle != null)
                {
                    cachedBundles[url] = assetBundle;
                    UnityEngine.Object[] loadedAssets = assetBundle.LoadAllAssets();
                    allLoadedAssets.AddRange(loadedAssets); //NOTE(Brian): Done to prevent Resources.UnloadUnusedAssets to strip them before they are used.
                }
                else
                {
                    failedRequests.Add(url);
                }
            }

            downloadingBundle.Remove(url);
        }

        IEnumerator FetchManifest(string hash, string sceneCid)
        {
            if (processedManifests.Contains(sceneCid) || dependenciesMap.ContainsKey(hash))
                yield break;

            string url = $"http://localhost:1338/manifests/{sceneCid}";

            yield return GetAssetBundle(url);

            AssetBundle mainAssetBundle;

            if (cachedBundles.TryGetValue(url, out mainAssetBundle))
            {
                AssetBundleManifest manifest = mainAssetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
                processedManifests.Add(sceneCid);

                List<string> dependencies = new List<string>();

                foreach (string asset in manifest.GetAllAssetBundles())
                {
                    string[] deps = manifest.GetAllDependencies(asset);
                    dependencies = new List<string>();

                    foreach (string dep in deps)
                    {
                        if (dep == asset)
                            continue;

                        var matchingPair = contentProvider.contents.FirstOrDefault((pair) => pair.hash.ToLowerInvariant() == dep.ToLowerInvariant());

                        if (matchingPair == null)
                        {
                            Debug.Log($"matchingPair not found? {dep}\n{contentProvider.ToString()}");
                            continue;
                        }

                        string depHash = matchingPair.hash;

                        if (depHash != null)
                        {
                            dependencies.Add(depHash);
                        }
                    }

                    var matchingPair2 = contentProvider.contents.FirstOrDefault((pair) => pair.hash.ToLowerInvariant() == asset.ToLowerInvariant());

                    if (matchingPair2 == null)
                    {
                        Debug.Log($"matchingPair2 not found? {asset}\n{contentProvider.ToString()}");
                        continue;
                    }

                    if (!dependenciesMap.ContainsKey(matchingPair2.hash))
                    {
                        dependenciesMap.Add(matchingPair2.hash, dependencies);
                        Debug.Log($"Adding dep map. Hash = {matchingPair2.hash}. Count = {dependencies.Count}.");
                    }
                }


                mainAssetBundle.Unload(true);

                cachedBundles.Remove(url);
            }
        }


        IEnumerator FetchAssetBundleWithDependencies(string hash)
        {
            string url = $"http://localhost:1338/{hash}";

            yield return GetAssetBundle(url);

            AssetBundle mainAssetBundle;

            if (!cachedBundles.TryGetValue(url, out mainAssetBundle))
                yield break;

            if (dependenciesMap.ContainsKey(hash))
            {
                foreach (string dep in dependenciesMap[hash])
                {
                    yield return FetchAssetBundleWithDependencies(dep);
                }
            }

            string[] assetNames = mainAssetBundle.GetAllAssetNames();

            for (int i = 0; i < assetNames.Length; i++)
            {
                string asset = assetNames[i];

                if (asset.Contains("glb") || asset.Contains("gltf"))
                {
                    GameObject model = mainAssetBundle.LoadAsset<GameObject>(asset);
                    gltfContainer = Instantiate(model);
#if UNITY_EDITOR
                    gltfContainer.GetComponentsInChildren<Renderer>().ToList().ForEach(ResetShader);
#endif
                    yield break;
                }

            }
        }

#if UNITY_EDITOR
        private static void ResetShader(Renderer renderer)
        {
            if (renderer.material == null) return;

            for (int i = 0; i < renderer.materials.Length; i++)
            {
                renderer.materials[i].shader = Shader.Find(renderer.materials[i].shader.name);
            }
        }
#endif


        IEnumerator TryToFetchAssetBundle(string targetUrl, Action<LoadWrapper> OnSuccess, Action<LoadWrapper> OnFail)
        {
            string lowerCaseUrl = targetUrl.ToLower();
            if (!contentProvider.fileToHash.ContainsKey(lowerCaseUrl))
            {
                Debug.Log("targetUrl not found?... " + targetUrl);
                OnFail.Invoke(this);
                yield break;
            }

            alreadyLoaded = false;
            string hash = contentProvider.fileToHash[lowerCaseUrl];

            yield return FetchManifest(hash, entity.scene.sceneData.id);

            if (processedManifests.Contains(entity.scene.sceneData.id))
            {
                yield return FetchAssetBundleWithDependencies(hash);
            }

            if (gltfContainer == null)
            {
                LoadGltf(targetUrl, OnSuccess, OnFail);
            }
            else
            {
                alreadyLoaded = true;
                gltfContainer.transform.SetParent(transform);
                gltfContainer.transform.ResetLocalTRS();

                if (initialVisibility == false)
                {
                    foreach (Renderer r in gltfContainer.GetComponentsInChildren<Renderer>())
                    {
                        r.enabled = false;
                    }
                }

                this.entity.OnCleanupEvent -= OnEntityCleanup;
                this.entity.OnCleanupEvent += OnEntityCleanup;
                OnSuccess.Invoke(this);
            }
        }

        void LoadGltf(string targetUrl, Action<LoadWrapper> OnSuccess, Action<LoadWrapper> OnFail)
        {
            if (gltfPromise != null)
            {
                AssetPromiseKeeper_GLTF.i.Forget(gltfPromise);

                if (VERBOSE)
                    Debug.Log("Forgetting not null promise...");
            }

            gltfPromise = new AssetPromise_GLTF(contentProvider, targetUrl);

            if (VERBOSE)
                Debug.Log($"Load(): target URL -> {targetUrl},  url -> {gltfPromise.url}, directory path -> {assetDirectoryPath}");

            gltfPromise.settings.parent = transform;

            if (initialVisibility == false)
            {
                gltfPromise.settings.visibleFlags = AssetPromise_GLTF.VisibleFlags.INVISIBLE;
            }
            else
            {
                if (useVisualFeedback)
                    gltfPromise.settings.visibleFlags = AssetPromise_GLTF.VisibleFlags.VISIBLE_WITH_TRANSITION;
                else
                    gltfPromise.settings.visibleFlags = AssetPromise_GLTF.VisibleFlags.VISIBLE_WITHOUT_TRANSITION;
            }

            gltfPromise.OnSuccessEvent += (x) => OnSuccessWrapper(x, OnSuccess);
            gltfPromise.OnFailEvent += (x) => OnFailWrapper(x, OnFail);

            AssetPromiseKeeper_GLTF.i.Keep(gltfPromise);
        }

        private void OnFailWrapper(Asset_GLTF loadedAsset, Action<LoadWrapper> OnFail)
        {
            if (VERBOSE)
            {
                Debug.Log($"Load(): target URL -> {gltfPromise.url}. Failure!");
            }

            OnFail?.Invoke(this);
        }

        private void OnSuccessWrapper(Asset_GLTF loadedAsset, Action<LoadWrapper> OnSuccess)
        {
            if (VERBOSE)
            {
                Debug.Log($"Load(): target URL -> {gltfPromise.url}. Success!");
            }

            alreadyLoaded = true;

            this.entity.OnCleanupEvent -= OnEntityCleanup;
            this.entity.OnCleanupEvent += OnEntityCleanup;

            OnSuccess?.Invoke(this);
        }

        public void OnEntityCleanup(ICleanableEventDispatcher source)
        {
            Unload();
        }

        public override void Unload()
        {
            this.entity.OnCleanupEvent -= OnEntityCleanup;
            AssetPromiseKeeper_GLTF.i.Forget(gltfPromise);
        }

        public void OnDestroy()
        {
            if (Application.isPlaying)
            {
                Unload();

                //NOTE(Brian): Fix for https://forum.unity.com/threads/5-5-1f-855646-not-fixed-unitywebreqest-high-cpu-use.453139/
                if (currentRequest != null)
                    currentRequest.Abort();
            }
        }
    }
}
