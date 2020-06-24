using AssetPromiseKeeper_Tests;
using DCL;
using DCL.Helpers;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.TestTools;

namespace AssetPromiseKeeper_Texture_Tests
{
    public class BlockedAndMasterPromisesShould : APKWithRefCountedAssetShouldWorkWhen_Base<AssetPromiseKeeper_Texture,
                                                            AssetPromise_Texture,
                                                            Asset_Texture,
                                                            AssetLibrary_Texture>
    {
        protected override AssetPromise_Texture CreatePromise()
        {
            return CreatePromise(Utils.GetTestsAssetsPath() + "/Images/atlas.png");
        }

        protected AssetPromise_Texture CreatePromise(string promiseUrl)
        {
            string url = promiseUrl;
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
        public IEnumerator FailCorrectlyWhenGivenWrongURL()
        {
            var prom = CreatePromise("123325");
            Asset_Texture asset = null;
            bool failEventCalled1 = false;
            prom.OnSuccessEvent += (x) => { asset = x; };
            prom.OnFailEvent += (x) => { failEventCalled1 = true; };

            var prom2 = CreatePromise("43254378");
            Asset_Texture asset2 = null;
            bool failEventCalled2 = false;
            prom2.OnSuccessEvent += (x) => { asset2 = x; };
            prom2.OnFailEvent += (x) => { failEventCalled2 = true; };

            var prom3 = CreatePromise("09898765");
            Asset_Texture asset3 = null;
            bool failEventCalled3 = false;
            prom3.OnSuccessEvent += (x) => { asset3 = x; };
            prom3.OnFailEvent += (x) => { failEventCalled3 = true; };

            keeper.Keep(prom);
            keeper.Keep(prom2);
            keeper.Keep(prom3);

            Assert.AreEqual(3, keeper.waitingPromisesCount);

            yield return prom;
            yield return prom2;
            yield return prom3;

            Assert.AreNotEqual(AssetPromiseState.FINISHED, prom.state);
            Assert.AreNotEqual(AssetPromiseState.FINISHED, prom2.state);
            Assert.AreNotEqual(AssetPromiseState.FINISHED, prom3.state);

            Assert.IsTrue(failEventCalled1);
            Assert.IsTrue(failEventCalled2);
            Assert.IsTrue(failEventCalled3);

            Assert.IsFalse(asset != null);
            Assert.IsFalse(asset2 != null);
            Assert.IsFalse(asset3 != null);

            Assert.IsFalse(keeper.library.Contains(asset));
            Assert.AreNotEqual(1, keeper.library.masterAssets.Count);
        }

        public IEnumerator WaitForPromisesOfSameTextureWithDifferentSettings()
        {
            // default texture (no settings)
            var prom = CreatePromise();
            Asset_Texture asset = null;
            prom.OnSuccessEvent += (x) => { asset = x; };

            keeper.Keep(prom);

            // same texture but with settings
            var prom2 = CreatePromise((int)TextureWrapMode.Repeat, (int)FilterMode.Trilinear);
            Asset_Texture asset2 = null;
            prom.OnSuccessEvent += (x) => { asset2 = x; };

            keeper.Keep(prom);

            Assert.AreEqual(1, keeper.waitingPromisesCount);

            Assert.AreNotEqual(AssetPromiseState.LOADING, prom.state);
            Assert.AreNotEqual(AssetPromiseState.WAITING, prom2.state);

            yield return prom;
            yield return prom2;
        }
    }
}
