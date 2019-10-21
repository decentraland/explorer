using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Networking;

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

        IEnumerator TryToFetchAssetBundle(string targetUrl, Action<LoadWrapper> OnSuccess, Action<LoadWrapper> OnFail)
        {
            if (contentProvider.fileToHash.ContainsKey(targetUrl))
            {
                string hash = contentProvider.fileToHash[targetUrl];

                UnityWebRequest manifestRequest = UnityWebRequest.Get($"http://localhost:8000/{entity.scene.sceneData.id}/{hash}.manifest");
                yield return manifestRequest.SendWebRequest();

                if (manifestRequest.isNetworkError || manifestRequest.isHttpError)
                {
                    LoadGltf(targetUrl, OnSuccess, OnFail);
                    yield break;
                }

                AssetBundle abmf = DownloadHandlerAssetBundle.GetContent(manifestRequest);
                AssetBundleManifest manifest = abmf.LoadAsset<AssetBundleManifest>("AssetBundleManifest");

                UnityWebRequest assetBundleRequest = UnityWebRequest.Get($"http://localhost:8000/{entity.scene.sceneData.id}/{hash}");
                yield return assetBundleRequest.SendWebRequest();

                AssetBundle ab = DownloadHandlerAssetBundle.GetContent(assetBundleRequest);

                string[] assetNames = ab.GetAllAssetNames();
                string gltfAsset = null;

                for (int i = 0; i < assetNames.Length; i++)
                {
                    if (assetNames[i].Contains("gltf") || assetNames[i].Contains("glb"))
                    {
                        gltfAsset = assetNames[i];
                        break;
                    }
                }

                if (gltfAsset != null)
                {
                    string[] deps = manifest.GetAllDependencies(gltfAsset);
                    //TODO(Brian): Load dependencies and load the desired bundle
                }
            }
            yield break;
        }

        void LoadGltf(string targetUrl, Action<LoadWrapper> OnSuccess, Action<LoadWrapper> OnFail)
        {
            if (gltfPromise != null)
            {
                AssetPromiseKeeper_GLTF.i.Forget(gltfPromise);

                if (VERBOSE)
                    Debug.Log("Forgetting not null promise...");
            }

            alreadyLoaded = false;

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
