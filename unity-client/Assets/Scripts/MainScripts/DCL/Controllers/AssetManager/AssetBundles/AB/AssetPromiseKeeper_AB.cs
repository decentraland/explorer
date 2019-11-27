namespace DCL
{
    public class AssetPromiseKeeper_AssetBundle : AssetPromiseKeeper<Asset_AB, AssetLibrary_AB, AssetPromise_AB>
    {
        public AssetPromiseKeeper_AssetBundle(AssetLibrary_AB library) : base(library)
        {
            library = new AssetLibrary_AB();
        }
    }

}
