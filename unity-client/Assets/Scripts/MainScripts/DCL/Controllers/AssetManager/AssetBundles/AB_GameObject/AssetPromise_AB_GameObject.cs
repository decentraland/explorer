using System;

namespace DCL
{
    public class AssetPromise_AB_GameObject : AssetPromise_WithUrl<Asset_AB_GameObject>
    {
        public AssetPromiseSettings_Rendering settings = new AssetPromiseSettings_Rendering();

        public AssetPromise_AB_GameObject(string contentUrl, string hash) : base(contentUrl, hash)
        {
        }

        protected override void OnLoad(Action OnSuccess, Action OnFail)
        {
            var promise = new AssetPromise_AB(contentUrl, hash);
            promise.OnSuccessEvent += (x) => OnSuccessInternal(x, OnSuccess);
            //promise.OnFailEvent += (x) => OnFailInternal(x, OnFail);
            AssetPromiseKeeper_AssetBundle.i.Keep(promise);
        }

        protected override void OnReuse(Action OnSuccess)
        {
            OnSuccessInternal(null, OnSuccess);
        }

        void OnSuccessInternal(Asset_AB asset, Action OnSuccess)
        {
            //if (asset != null)
            //    this.asset = asset;

            asset.Show(true, OnSuccess);
        }

        protected override void OnAfterLoadOrReuse()
        {
        }

        protected override void OnBeforeLoadOrReuse()
        {

        }

        protected override void OnCancelLoading()
        {
        }
    }
}
