namespace DCL
{
    public class AssetPromiseKeeper_AB_GameObject : AssetPromiseKeeper<Asset_AB_GameObject, AssetLibrary_AB_GameObject, AssetPromise_AB_GameObject>
    {
        public AssetPromiseKeeper_AB_GameObject(AssetLibrary_AB_GameObject library) : base(library)
        {
            library = new AssetLibrary_AB_GameObject();
        }
    }

}
