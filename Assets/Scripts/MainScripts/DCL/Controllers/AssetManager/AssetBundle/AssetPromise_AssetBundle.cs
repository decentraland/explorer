using DCL.Helpers;
using System;
using System.Collections;
using UnityEngine;

namespace DCL
{
    public class AssetPromise_AssetBundle : AssetPromise<Asset_AssetBundle>
    {
        public string url { get; private set; }
        ContentProvider provider = null;
        object id = null;
        string sceneId = "";
        UnityEngine.Object[] loadedAssets;

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

        protected override void ApplySettings_LoadStart()
        {
            Transform assetTransform = asset.container.transform;

            asset.container.name = "AB: " + url;

            if (settings.parent != null)
            {
                assetTransform.SetParent(settings.parent, false);
                assetTransform.ResetLocalTRS();
            }

            if (settings.initialLocalPosition.HasValue)
            {
                assetTransform.localPosition = settings.initialLocalPosition.Value;
            }

            if (settings.initialLocalRotation.HasValue)
            {
                assetTransform.localRotation = settings.initialLocalRotation.Value;
            }

            if (settings.initialLocalScale.HasValue)
            {
                assetTransform.localScale = settings.initialLocalScale.Value;
            }
        }

        public AssetPromise_AssetBundle(ContentProvider provider, string url, string sceneId)
        {
            this.provider = provider;
            this.url = url;
            this.sceneId = sceneId; //TODO(Brian): Remove this field
        }

        protected override void OnLoad(Action OnSuccess, Action OnFail)
        {
            CoroutineStarter.Start(OnLoadCoroutine(OnSuccess, OnFail));
        }

        IEnumerator OnLoadCoroutine(Action OnSuccess, Action OnFail)
        {
            string lowerCaseUrl = url.ToLower();

            if (!provider.fileToHash.ContainsKey(lowerCaseUrl))
            {
                Debug.Log("targetUrl not found?... " + url);
                OnFail?.Invoke();
                yield break;
            }

            string hash = provider.fileToHash[lowerCaseUrl];

            yield return AssetBundleLoadHelper.FetchManifest(provider, hash, sceneId);

            GameObject container = null;

            if (!AssetBundleLoadHelper.HasManifest(sceneId))
            {
                OnFail?.Invoke();
                yield break;
            }

            yield return AssetBundleLoadHelper.FetchAssetBundleWithDependencies(hash, (go) => { container = go; });

            if (container == null)
                OnFail?.Invoke();
            else
            {
                asset.assetBundleAssetName = container.name;

                container.transform.parent = asset.container.transform;
                container.transform.ResetLocalTRS();

                OnSuccess?.Invoke();
            }
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
    }
}
