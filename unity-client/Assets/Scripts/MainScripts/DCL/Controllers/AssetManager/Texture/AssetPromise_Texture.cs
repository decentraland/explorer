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
        string id;
        string defaultTextureId;
        TextureWrapMode wrapMode;
        FilterMode filterMode;
        Coroutine loadCoroutine;

        public AssetPromise_Texture(string textureUrl, TextureWrapMode textureWrapMode = DEFAULT_WRAP_MODE, FilterMode textureFilterMode = DEFAULT_FILTER_MODE)
        {
            url = textureUrl;
            wrapMode = textureWrapMode;
            filterMode = textureFilterMode;

            id = ConstructId(url, wrapMode, filterMode);
            defaultTextureId = ConstructId(url, DEFAULT_WRAP_MODE, DEFAULT_FILTER_MODE);
        }

        protected override void OnAfterLoadOrReuse()
        {
        }

        protected override void OnBeforeLoadOrReuse()
        {
        }

        protected override void OnCancelLoading()
        {
            ClearLoadCoroutine();
        }

        protected override void OnLoad(Action OnSuccess, Action OnFail)
        {
            ClearLoadCoroutine();

            // Reuse the already-stored default texture with the needed config
            if (!UsesDefaultWrapAndFilterMode() && library.Contains(defaultTextureId))
            {
                asset.texture = library.Get(defaultTextureId).texture;

                OnSuccess?.Invoke();
            }
            else
            {
                loadCoroutine = CoroutineStarter.Start(DownloadAndStore(OnSuccess, OnFail));
            }
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
                if (!library.Contains(defaultTextureId))
                {
                    // Save default texture asset
                    asset.id = defaultTextureId;
                    library.Add(asset);

                    // Duplicate default texture to be configured as we want
                    Texture2D duplicatedTex = new Texture2D(asset.texture.width, asset.texture.height, asset.texture.format, false);
                    Graphics.CopyTexture(asset.texture, duplicatedTex);

                    // By using library.Get() for the default tex we just stored, we add a reference count to it,
                    // that will come in handy for removing that default tex when there is no one using it
                    asset = library.Get(defaultTextureId).Clone() as Asset_Texture;

                    asset.id = id;
                    asset.texture = duplicatedTex;
                }

                ConfigureTexture(asset.texture, wrapMode, filterMode);
            }

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
            return id;
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
