using AssetPromiseKeeper_Tests;
using DCL;
using DCL.Helpers;
using UnityEngine;

namespace AssetPromiseKeeper_Texture_Tests
{
    public class APK_Texture_ShouldWorkWhen : APKWithRefCountedAssetShouldWorkWhen_Base<AssetPromiseKeeper_Texture,
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
    }
}
