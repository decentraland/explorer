using AssetPromiseKeeper_Tests;
using DCL;
using DCL.Helpers;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.TestTools;

namespace AssetPromiseKeeper_Texture_Tests
{
    public class APK_Texture_Promise_Should : APKWithRefCountedAssetShouldWorkWhen_Base<AssetPromiseKeeper_Texture,
                                                            AssetPromise_Texture,
                                                            Asset_Texture,
                                                            AssetLibrary_Texture>
    {
        protected override AssetPromise_Texture CreatePromise()
        {
            string url = Utils.GetTestsAssetsPath() + "/Images/atlas.png";
            var prom = new AssetPromise_Texture(url);
            return prom;
        }

        protected AssetPromise_Texture CreatePromise(int wrapmode = -1, int filterMode = -1)
        {
            if (filterMode <= -1 && wrapmode <= -1) return CreatePromise();

            string url = Utils.GetTestsAssetsPath() + "/Images/atlas.png";
            AssetPromise_Texture prom = new AssetPromise_Texture(url, (TextureWrapMode)wrapmode, (FilterMode)filterMode);

            return prom;
        }

        [UnityTest]
        public IEnumerator BeSetupCorrectlyAfterLoad()
        {
            // Check non-default-settings texture
            Asset_Texture loadedAsset = null;
            var prom = CreatePromise((int)TextureWrapMode.Repeat, (int)FilterMode.Trilinear);

            prom.OnSuccessEvent += (x) => loadedAsset = x;

            keeper.Keep(prom);

            yield return prom;

            Assert.IsNotNull(loadedAsset);
            Assert.IsNotNull(loadedAsset.texture);
            Assert.AreEqual(loadedAsset.texture.wrapMode, TextureWrapMode.Repeat);
            Assert.AreEqual(loadedAsset.texture.filterMode, FilterMode.Trilinear);

            // Check default texture
            loadedAsset = null;
            prom = CreatePromise();

            prom.OnSuccessEvent += (x) => loadedAsset = x;

            keeper.Keep(prom);

            yield return prom;

            Assert.IsNotNull(loadedAsset);
            Assert.IsNotNull(loadedAsset.texture);

            TextureWrapMode defaultWrapMode = TextureWrapMode.Clamp;
            FilterMode defaultFilterMode = FilterMode.Bilinear;

            Assert.AreEqual(loadedAsset.texture.wrapMode, defaultWrapMode);
            Assert.AreEqual(loadedAsset.texture.filterMode, defaultFilterMode);
        }
    }
}