using System;

namespace DCL
{
    public class AssetPromise_WithUrl<T> : AssetPromise<T> where T : Asset, new()
    {
        public string contentUrl;
        public string hash;
        public AssetPromise_WithUrl(string contentUrl, string hash)
        {
            this.contentUrl = contentUrl;
            this.hash = hash;
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

        protected override void OnLoad(Action OnSuccess, Action OnFail)
        {
        }

        internal override object GetId()
        {
            return hash;
        }
    }
}
