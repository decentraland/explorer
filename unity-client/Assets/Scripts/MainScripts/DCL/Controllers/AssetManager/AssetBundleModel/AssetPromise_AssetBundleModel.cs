using DCL.Helpers;
using System;
using UnityEngine;

namespace DCL
{
    public class AssetPromise_AssetBundleModel : AssetPromise_AssetBundle<Asset_AssetBundleModel>
    {
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
        public AssetPromise_AssetBundleModel(string contentUrl, string hash) : base(contentUrl, hash)
        {
        }

        protected override void OnLoad(Action OnSuccess, Action OnFail)
        {
            CoroutineStarter.Start(base.LoadAssetBundleWithDeps(contentUrl, hash, () => OnSuccessInternal(OnSuccess), OnFail));
        }

        protected override void OnReuse(Action OnSuccess)
        {
            OnSuccessInternal(OnSuccess);
        }

        void OnSuccessInternal(Action OnSuccess)
        {
            asset.Show(true, OnSuccess);
        }

        protected override void OnAfterLoadOrReuse()
        {
            base.OnAfterLoadOrReuse();
        }

        //IEnumerator OnLoadCoroutine(Action OnSuccess, Action OnFail)
        //{
        //    string lowerCaseUrl = contentUrl.ToLower();

        //    GameObject container = null;

        //    yield return AssetBundleLoadHelper.FetchAssetBundleWithDependencies(contentUrl, hash, (go) => { container = go; });

        //    if (container == null)
        //        OnFail?.Invoke();
        //    else
        //    {
        //        asset.assetBundleAssetName = container.name;

        //        container.transform.parent = asset.container.transform;
        //        container.transform.ResetLocalTRS();

        //        OnSuccess?.Invoke();
        //    }
        //}

        protected override void OnBeforeLoadOrReuse()
        {
            Transform assetTransform = asset.container.transform;

            asset.container.name = "AB: " + contentUrl;

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
