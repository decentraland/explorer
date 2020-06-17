namespace DCL
{
    public class AssetPromiseKeeper_Texture : AssetPromiseKeeper<Asset_Texture, AssetLibrary_Texture, AssetPromise_Texture>
    {
        // public AssetPromiseKeeper_Texture(AssetLibrary_Texture library) : base(library)
        public AssetPromiseKeeper_Texture() : base(new AssetLibrary_Texture())
        {
        }
    }
}
