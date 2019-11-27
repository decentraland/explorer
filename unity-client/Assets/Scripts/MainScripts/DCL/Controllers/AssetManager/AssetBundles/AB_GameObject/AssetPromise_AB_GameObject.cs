using System;

namespace DCL
{
    public class AssetPromise_AB_GameObject : AssetPromise_WithUrl<Asset_AB_GameObject>
    {
        public AssetPromiseSettings_Rendering settings = new AssetPromiseSettings_Rendering();
        AssetPromise_AB subPromise;

        public AssetPromise_AB_GameObject(string contentUrl, string hash) : base(contentUrl, hash)
        {
        }

        protected override void OnLoad(Action OnSuccess, Action OnFail)
        {
            subPromise = new AssetPromise_AB(contentUrl, hash);
            subPromise.OnSuccessEvent += (x) => asset.Show(() => OnLoadFinished(OnSuccess, OnFail));
            subPromise.OnFailEvent += (x) => OnFail();
            AssetPromiseKeeper_AB.i.Keep(subPromise);
        }

        void OnLoadFinished(Action OnSuccess, Action OnFail)
        {
            if (asset != null && subPromise != null && subPromise.state == AssetPromiseState.FINISHED)
                OnSuccess?.Invoke();
            else
                OnFail?.Invoke();
        }

        protected override void OnReuse(Action OnSuccess)
        {
            asset.Show(OnSuccess);
        }

        protected override void OnAfterLoadOrReuse()
        {
            asset.ownerPromise = subPromise;
            settings.ApplyAfterLoad(asset.container.transform);
        }

        protected override void OnBeforeLoadOrReuse()
        {
            settings.ApplyBeforeLoad(asset.container.transform);
        }

        protected override void OnCancelLoading()
        {
            AssetPromiseKeeper_AB.i.Forget(subPromise);
        }
    }
}
