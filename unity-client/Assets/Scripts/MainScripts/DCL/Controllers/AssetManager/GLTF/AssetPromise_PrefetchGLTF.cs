using UnityEngine;

namespace DCL
{
    public class AssetPromise_PrefetchGLTF : AssetPromise_GLTF
    {
        public AssetPromise_PrefetchGLTF(ContentProvider provider, string url) : base(provider, url)
        {
            settings.visibleFlags = VisibleFlags.INVISIBLE;
        }

        protected override void ApplySettings_LoadStart()
        {
            Transform assetTransform = asset.container.transform;

            asset.container.name = "GLTF: " + url;
        }

        protected override void ApplySettings_LoadFinished()
        {
            Renderer[] renderers = asset.container.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                renderer.enabled = false;
            }
        }

        internal override object GetId()
        {
            return base.GetId();
        }

        protected override void OnLoad(System.Action OnSuccess, System.Action OnFail)
        {
            base.OnLoad(OnSuccess, OnFail);
        }

        void ParseGLTFWebRequestedFile(ref string requestedFileName)
        {
            provider.TryGetContentsUrl(assetDirectoryPath + requestedFileName, out requestedFileName);
        }

        protected override void OnReuse(System.Action OnSuccess)
        {
            asset.Show(false, OnSuccess);
        }

        protected override void AddToLibrary()
        {
            library.Add(asset);
        }

        internal override void Load()
        {
            if (!library.Contains(GetId()))
            {
                base.Load();
            }
            else
            {
                CallAndClearEvents(false);
            }
        }

        protected override void OnCancelLoading()
        {
            base.OnCancelLoading();
        }

        protected override Asset_GLTF GetAsset(object id)
        {
            return library.Get(id);
        }
    }
}