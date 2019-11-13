namespace DCL
{
    public class AssetPromiseKeeper_AssetBundle : AssetPromiseKeeper<Asset_AssetBundle, AssetLibrary_AssetBundle, AssetPromise_AssetBundle>
    {
        public AssetPromiseKeeper_AssetBundle(AssetLibrary_AssetBundle library) : base(library)
        {
            library = new AssetLibrary_AssetBundle();
        }
    }

}
