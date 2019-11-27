namespace DCL
{
    public class AssetPromiseKeeper_AB : AssetPromiseKeeper<Asset_AB, AssetLibrary_AB, AssetPromise_AB>
    {
        public AssetPromiseKeeper_AB(AssetLibrary_AB library) : base(library)
        {
            library = new AssetLibrary_AB();
        }
    }

}
