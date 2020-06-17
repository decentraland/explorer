using System;
using UnityEngine;
using System.Collections;
using DCL.Helpers;

namespace DCL
{
    public class AssetPromise_Texture : AssetPromise<Asset_Texture>
    {
        string url;
        Coroutine loadCoroutine;

        public AssetPromise_Texture(string textureUrl)
        {
            url = textureUrl;
        }

        protected override void OnAfterLoadOrReuse()
        {
            // ClearLoadCoroutine();
        }

        protected override void OnBeforeLoadOrReuse()
        {
            // ClearLoadCoroutine();
        }

        protected override void OnCancelLoading()
        {
            ClearLoadCoroutine();
        }

        protected override void OnLoad(Action OnSuccess, Action OnFail)
        {
            ClearLoadCoroutine();

            loadCoroutine = CoroutineStarter.Start(DownloadAndStore(OnSuccess, OnFail));
        }

        IEnumerator DownloadAndStore(Action OnSuccess, Action OnFail)
        {
            if (!string.IsNullOrEmpty(url))
            {
                yield return Utils.FetchTexture(url, (tex) =>
                {
                    asset.texture = (Texture2D)tex;
                    OnSuccess?.Invoke();
                }, (errorMessage) => OnFail?.Invoke());
            }
            else
            {
                OnFail?.Invoke();
            }
        }

        internal override object GetId()
        {
            return url;
        }

        void ClearLoadCoroutine()
        {
            if (loadCoroutine != null)
            {
                CoroutineStarter.Stop(loadCoroutine);
                loadCoroutine = null;
            }
        }
    }
}
