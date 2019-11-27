using DCL;
using DCL.Helpers;

namespace AssetPromiseKeeper_Tests
{
    public class APKShouldWorkWhen_AB_GameObject : 
        APKWithPoolableAssetShouldWorkWhen_Base< AssetPromiseKeeper_AB_GameObject, 
                                                AssetPromise_AB_GameObject, 
                                                Asset_AB_GameObject, 
                                                AssetLibrary_AB_GameObject>
    {
        protected override AssetPromise_AB_GameObject CreatePromise()
        {
            string contentUrl = Utils.GetTestsAssetsPath() + "/AssetBundles/";
            string hash = "QmYACL8SnbXEonXQeRHdWYbfm8vxvaFAWnsLHUaDG4ABp5";
            var prom = new AssetPromise_AB_GameObject(contentUrl, hash);
            return prom;
        }
    }
}