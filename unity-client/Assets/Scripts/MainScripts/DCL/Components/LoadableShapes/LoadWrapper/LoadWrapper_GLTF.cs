using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Networking;
using System.Linq;
using DCL.Helpers;

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

        IEnumerator FetchAssetBundleWithDependencies(string hash, string[] extensionFilter = null)
        {
            UnityWebRequest manifestRequest = UnityWebRequest.Get($"http://localhost:8000/{entity.scene.sceneData.id}/{hash}.manifest");
            yield return manifestRequest.SendWebRequest();

            AssetBundle abmf = DownloadHandlerAssetBundle.GetContent(manifestRequest);
            AssetBundleManifest manifest = abmf.LoadAsset<AssetBundleManifest>("AssetBundleManifest");

            UnityWebRequest assetBundleRequest = UnityWebRequest.Get($"http://localhost:8000/{entity.scene.sceneData.id}/{hash}");
            yield return assetBundleRequest.SendWebRequest();

            AssetBundle mainAssetBundle = DownloadHandlerAssetBundle.GetContent(assetBundleRequest);

            string[] assetNames = mainAssetBundle.GetAllAssetNames();

            if (extensionFilter != null)
            {
                for (int i = 0; i < assetNames.Length; i++)
                {
                    string asset = assetNames[i];

                    bool containsExtension = extensionFilter.Any(x => asset.Contains(x));

                    if (containsExtension)
                    {
                        string[] deps = manifest.GetAllDependencies(asset);

                        foreach (string dep in deps)
                        {
                            string[] depPath = dep.Split('/');
                            string depHash = contentProvider.contents.FirstOrDefault((pair) => pair.hash.ToLowerInvariant() == depPath[2].ToLowerInvariant()).hash;

                            if (depHash != null)
                            {
                                yield return FetchAssetBundleWithDependencies(depHash);
                            }
                        }

                        if (asset.Contains("glb") || asset.Contains("gltf"))
                        {
                            Debug.Log("Instantiating asset bundle! " + asset);
                            gltfContainer = Instantiate(mainAssetBundle.LoadAsset<GameObject>(asset));
                            yield break;
                        }
                    }
                }
            }
        }

        IEnumerator TryToFetchAssetBundle(string targetUrl, Action<LoadWrapper> OnSuccess, Action<LoadWrapper> OnFail)
        {
            if (contentProvider.fileToHash.ContainsKey(targetUrl))
            {
                alreadyLoaded = false;
                string hash = contentProvider.fileToHash[targetUrl];
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
