using AssetPromiseKeeper_Tests;
using DCL;
using DCL.Helpers;
using System.Collections;
using UnityEngine.Assertions;
using UnityEngine.TestTools;

namespace AssetPromiseKeeper_Gif_Tests
{
    public class APK_Gif_Promise_Should : TestsBase_APK<AssetPromiseKeeper_Gif,
                                                            AssetPromise_Gif,
                                                            Asset_Gif,
                                                            AssetLibrary_RefCounted<Asset_Gif>>
    {
        protected AssetPromise_Gif CreatePromise()
        {
            string url = Utils.GetTestsAssetsPath() + "/Images/gif1.gif";
            var prom = new AssetPromise_Gif(url);
            return prom;
        }

        [UnityTest]
        public IEnumerator ShareGifAmongPromises()
        {
            Asset_Gif loadedAsset = null;
            var prom = CreatePromise();

            prom.OnSuccessEvent += (x) => loadedAsset = x;

            keeper.Keep(prom);
            yield return prom;

            Asset_Gif loadedAsset2 = null;
            var prom2 = CreatePromise();

            prom2.OnSuccessEvent += (x) => loadedAsset2 = x;

            keeper.Keep(prom2);
            yield return prom2;

            Assert.IsNotNull(loadedAsset);
            Assert.IsNotNull(loadedAsset.texture);
            Assert.IsNotNull(loadedAsset2);
            Assert.IsNotNull(loadedAsset2.texture);

            Assert.IsTrue(loadedAsset.texture == loadedAsset2.texture);

            // 2 default textures
            Asset_Gif loadedAsset3 = null;
            var prom3 = CreatePromise();

            prom3.OnSuccessEvent += (x) => loadedAsset3 = x;

            keeper.Keep(prom3);
            yield return prom3;

            Asset_Gif loadedAsset4 = null;
            var prom4 = CreatePromise();

            prom4.OnSuccessEvent += (x) => loadedAsset4 = x;

            keeper.Keep(prom4);
            yield return prom4;

            Assert.IsNotNull(loadedAsset3);
            Assert.IsNotNull(loadedAsset3.texture);
            Assert.IsNotNull(loadedAsset4);
            Assert.IsNotNull(loadedAsset4.texture);

            Assert.IsTrue(loadedAsset3.texture == loadedAsset4.texture);
        }
    }
}