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
        AssetPromise_AssetBundle abPromise;

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

            //StartCoroutine(TryToFetchAssetBundle(targetUrl, OnSuccess, OnFail));

            if (abPromise != null)
            {
                AssetPromiseKeeper_AssetBundle.i.Forget(abPromise);

                if (VERBOSE)
                    Debug.Log("Forgetting not null promise...");
            }


            abPromise = new AssetPromise_AssetBundle(contentProvider, targetUrl, entity.scene.sceneData.id);
            abPromise.settings.parent = transform;


            abPromise.OnSuccessEvent += (x) => OnSuccessWrapper(x, OnSuccess);
            abPromise.OnFailEvent += (x) => OnFailWrapper(x, OnFail);

            AssetPromiseKeeper_AssetBundle.i.Keep(abPromise);
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

        private void OnFailWrapper(Asset loadedAsset, Action<LoadWrapper> OnFail)
        {
            if (VERBOSE)
            {
                Debug.Log($"Load(): target URL -> {gltfPromise.url}. Failure!");
            }

            OnFail?.Invoke(this);
        }

        private void OnSuccessWrapper(Asset loadedAsset, Action<LoadWrapper> OnSuccess)
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
            AssetPromiseKeeper_AssetBundle.i.Forget(abPromise);
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
