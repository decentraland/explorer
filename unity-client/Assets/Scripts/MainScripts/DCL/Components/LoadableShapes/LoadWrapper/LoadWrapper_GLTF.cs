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
        static Dictionary<string, AssetBundleManifest> cachedManifests = new Dictionary<string, AssetBundleManifest>();
        static HashSet<string> failedRequests = new HashSet<string>();
        static List<string> downloadingBundle = new List<string>();

        IEnumerator GetAssetBundle(string url)
        {
            if (failedRequests.Contains(url) || cachedBundles.ContainsKey(url))
                yield break;

            UnityWebRequest assetBundleRequest = UnityWebRequestAssetBundle.GetAssetBundle(url);

            if (downloadingBundle.Contains(url))
            {
                yield return new WaitUntil(() => cachedBundles.ContainsKey(url));
                yield break;
            }

            downloadingBundle.Add(url);
            yield return assetBundleRequest.SendWebRequest();

            if (assetBundleRequest.isHttpError || assetBundleRequest.isNetworkError)
            {
                failedRequests.Add(url);
                yield break;
            }

            if (!cachedBundles.ContainsKey(url))
            {
                AssetBundle assetBundle = DownloadHandlerAssetBundle.GetContent(assetBundleRequest);
                cachedBundles[url] = assetBundle;
                downloadingBundle.Remove(url);
            }
        }

        IEnumerator FetchManifest(string sceneCid)
        {
            if (cachedManifests.ContainsKey(sceneCid))
                yield break;

            string url = $"http://localhost:1338/manifests/{sceneCid}";

            yield return GetAssetBundle(url);

            AssetBundle mainAssetBundle;

            if (cachedBundles.TryGetValue(url, out mainAssetBundle))
            {
                AssetBundleManifest manifest = mainAssetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
                cachedManifests[sceneCid] = manifest;
            }
        }


        IEnumerator FetchAssetBundleWithDependencies(string hash, string[] extensionFilter = null, bool loadDependencies = true)
        {
            if (!cachedManifests.ContainsKey(entity.scene.sceneData.id))
                yield break;

            string url = $"http://localhost:1338/{hash}";

            yield return GetAssetBundle(url);

            AssetBundle mainAssetBundle;

            if (!cachedBundles.TryGetValue(url, out mainAssetBundle))
                yield break;

            AssetBundleManifest manifest = cachedManifests[entity.scene.sceneData.id];

            string[] assetNames = mainAssetBundle.GetAllAssetNames();

            if (extensionFilter != null)
            {
                for (int i = 0; i < assetNames.Length; i++)
                {
                    string asset = assetNames[i];

                    bool containsExtension = extensionFilter.Any(x => asset.Contains(x));

                    if (containsExtension)
                    {
                        if (loadDependencies)
                        {
                            string assetBundleName = Regex.Match(asset, @"(\w*)\.\w*$").Groups[1].Value;
                            string[] deps = manifest.GetAllDependencies(assetBundleName);

                            foreach (string dep in deps)
                            {
                                if (dep == assetBundleName)
                                    continue;

                                string depHash = contentProvider.contents.FirstOrDefault((pair) => pair.hash.ToLowerInvariant() == dep.ToLowerInvariant()).hash;

                                if (depHash != null)
                                {
                                    yield return FetchAssetBundleWithDependencies(depHash, null, false);
                                }
                            }
                        }

                        if (asset.Contains("glb") || asset.Contains("gltf"))
                        {
                            gltfContainer = Instantiate(mainAssetBundle.LoadAsset<GameObject>(asset));
#if UNITY_EDITOR
                            gltfContainer.GetComponentsInChildren<Renderer>().ToList().ForEach(ResetShader);
#endif
                            yield break;
                        }
                    }
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

            yield return FetchManifest(entity.scene.sceneData.id);
            yield return FetchAssetBundleWithDependencies(hash, new string[] { "gltf", "glb" });

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
            }
        }
    }
}
