using DCL.Helpers;
using System;
using System.Collections;
using UnityEngine;

namespace DCL
{
    public class AssetPromise_AssetBundleModel : AssetPromise_AssetBundle<Asset_AssetBundleModel>
    {
        public AssetPromise_AssetBundleModel(ContentProvider provider, string baseUrl, string url) : base(provider, baseUrl, url)
        {
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

            GameObject container = null;

            yield return AssetBundleLoadHelper.FetchAssetBundleWithDependencies(baseUrl, hash, (go) => { container = go; });

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
    }
}
