namespace DCL
{
    public class AssetPromiseKeeper_AssetBundleModel : AssetPromiseKeeper<Asset_AssetBundleModel, AssetLibrary_AssetBundleModel, AssetPromise_AssetBundleModel>
    {
        public AssetPromiseKeeper_AssetBundleModel(AssetLibrary_AssetBundleModel library) : base(library)
        {
            library = new AssetLibrary_AssetBundleModel();
        }
    }

}
