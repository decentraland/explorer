﻿using System;
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
        static readonly bool USE_LOCAL_HOST = false;
        static readonly bool USE_GLTF_FALLBACK = false;
        static readonly bool VERBOSE = false;

        public GameObject container;

        AssetPromise_GLTF gltfPromise;
        AssetPromise_AssetBundle abPromise;

        string assetDirectoryPath;


#if UNITY_EDITOR
        [ContextMenu("Debug Load Count")]
        public void DebugLoadCount()
        {
            float loadTime = Mathf.Min(loadFinishTime, Time.realtimeSinceStartup) - loadStartTime;

            if (gltfPromise != null)
                Debug.Log($"promise state = {gltfPromise.state} ({loadTime} load time)... waiting promises = {AssetPromiseKeeper_GLTF.i.waitingPromisesCount}");

            if (abPromise != null)
                Debug.Log($"promise state = {abPromise.state} ({loadTime} load time)... waiting promises = {AssetPromiseKeeper_AssetBundle.i.waitingPromisesCount}");
        }

        float loadStartTime = 0;
        float loadFinishTime = float.MaxValue;
#endif

        public override void Load(string targetUrl, Action<LoadWrapper> OnSuccess, Action<LoadWrapper> OnFail)
        {
#if UNITY_EDITOR
            loadStartTime = Time.realtimeSinceStartup;
#endif
            Assert.IsFalse(string.IsNullOrEmpty(targetUrl), "url is null!!");

            if (USE_GLTF_FALLBACK)
                LoadAssetBundle(targetUrl, OnSuccess, (x) => LoadGltf(targetUrl, OnSuccess, OnFail));
            else
                LoadAssetBundle(targetUrl, OnSuccess, OnFail);
        }

        void LoadAssetBundle(string targetUrl, Action<LoadWrapper> OnSuccess, Action<LoadWrapper> OnFail)
        {
            if (abPromise != null)
            {
                AssetPromiseKeeper_AssetBundle.i.Forget(abPromise);

                if (VERBOSE)
                    Debug.Log("Forgetting not null promise...");
            }

            string bundlesBaseUrl = USE_LOCAL_HOST ? "http://localhost:1338/" : entity.scene.sceneData.baseUrlBundles;

            abPromise = new AssetPromise_AssetBundle(entity.scene.contentProvider, bundlesBaseUrl, targetUrl);
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

            gltfPromise = new AssetPromise_GLTF(entity.scene.contentProvider, targetUrl);

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
#if UNITY_EDITOR
            loadFinishTime = Time.realtimeSinceStartup;
#endif

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
#if UNITY_EDITOR
            loadFinishTime = Time.realtimeSinceStartup;
#endif

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
