using System;
using UnityEngine;
using System.Collections;
using DCL.Helpers;

namespace DCL
{
    public class AssetPromise_Texture : AssetPromise<Asset_Texture>
    {
        const TextureWrapMode DEFAULT_WRAP_MODE = TextureWrapMode.Clamp;
        const FilterMode DEFAULT_FILTER_MODE = FilterMode.Bilinear;

        string url;
        string idWithTexSettings;
        string idWithDefaultTexSettings;
        TextureWrapMode wrapMode;
        FilterMode filterMode;
        Coroutine loadCoroutine;

        public AssetPromise_Texture(string textureUrl, TextureWrapMode textureWrapMode = DEFAULT_WRAP_MODE, FilterMode textureFilterMode = DEFAULT_FILTER_MODE)
        {
            url = textureUrl;
            wrapMode = textureWrapMode;
            filterMode = textureFilterMode;

            idWithDefaultTexSettings = ConstructId(url, DEFAULT_WRAP_MODE, DEFAULT_FILTER_MODE);
            idWithTexSettings = UsesDefaultWrapAndFilterMode() ? idWithDefaultTexSettings : ConstructId(url, wrapMode, filterMode);
        }

        protected override void OnAfterLoadOrReuse()
        {
        }

        protected override void OnBeforeLoadOrReuse()
        {
        }

        internal override void Load()
        {
            if (state == AssetPromiseState.LOADING || state == AssetPromiseState.FINISHED)
                return;

            state = AssetPromiseState.LOADING;

            // We use the id-with-settings in the library, and the default tex id for the "processing" promise to block efficiently the other promises
            // that may arise at that moment, disregarding their settings until it gets stored in the library
            if (library.Contains(idWithTexSettings))
            {
                asset = GetAsset(idWithTexSettings);

                if (asset != null)
                {
                    OnBeforeLoadOrReuse();
                    OnReuse(OnReuseFinished);
                }
                else
                {
                    CallAndClearEvents(false);
                }

                return;
            }

            asset = new Asset_Texture();
            OnBeforeLoadOrReuse();
            asset.id = GetId();

            OnLoad(OnLoadSuccess, OnLoadFailure);
        }

        protected override void OnCancelLoading()
        {
            ClearLoadCoroutine();
        }

        protected override void OnLoad(Action OnSuccess, Action OnFail)
        {
            ClearLoadCoroutine();

            // Reuse the already-stored default texture, we duplicate it and set the needed config afterwards
            if (library.Contains(idWithDefaultTexSettings) && !UsesDefaultWrapAndFilterMode())
                OnSuccess?.Invoke();
            else
                loadCoroutine = CoroutineStarter.Start(DownloadAndStore(OnSuccess, OnFail));
        }

        IEnumerator DownloadAndStore(Action OnSuccess, Action OnFail)
        {
            if (!string.IsNullOrEmpty(url))
            {
                yield return Utils.FetchTexture(url, (tex) =>
                {
                    asset.texture = tex;
                    OnSuccess?.Invoke();
                }, (errorMessage) => OnFail?.Invoke());
            }
            else
            {
                OnFail?.Invoke();
            }
        }

        protected override bool AddToLibrary()
        {
            if (!UsesDefaultWrapAndFilterMode())
            {
                if (!library.Contains(idWithDefaultTexSettings))
                {
                    // Save default texture asset
                    asset.id = idWithDefaultTexSettings;
                    library.Add(asset);
                }

                var defaultTexAsset = library.Get(idWithDefaultTexSettings);

                // By using library.Get() for the default tex we have stored, we increase its references counter,
                // that will come in handy for removing that default tex when there is no one using it
                asset = defaultTexAsset.Clone() as Asset_Texture;
                asset.dependencyAsset = defaultTexAsset;

                // Duplicate default texture to be configured as we want
                Texture2D duplicatedTex = new Texture2D(asset.texture.width, asset.texture.height, asset.texture.format, false);
                Graphics.CopyTexture(asset.texture, duplicatedTex);

                asset.texture = duplicatedTex;

                ConfigureTexture(asset.texture, wrapMode, filterMode);
            }

            asset.id = idWithTexSettings;
            return library.Add(asset);
        }

        void ConfigureTexture(Texture2D texture, TextureWrapMode textureWrapMode, FilterMode textureFilterMode)
        {
            texture.wrapMode = textureWrapMode;
            texture.filterMode = textureFilterMode;
            texture.Compress(false);
            texture.Apply(textureFilterMode != FilterMode.Point, true);
        }

        string ConstructId(string textureUrl, TextureWrapMode textureWrapMode, FilterMode textureFilterMode)
        {
            return ((int)textureWrapMode) + ((int)textureFilterMode) + textureUrl;
        }

        internal override object GetId()
        {
            return idWithDefaultTexSettings;
        }

        public bool UsesDefaultWrapAndFilterMode()
        {
            return wrapMode == DEFAULT_WRAP_MODE && filterMode == DEFAULT_FILTER_MODE;
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
