using DCL;
using DCL.Helpers;
using System.Collections;
using UnityEngine.Assertions;
using UnityEngine.TestTools;

namespace AssetPromiseKeeper_AssetBundle_Tests
{
    public class BlockedAndMasterPromisesShould : TestsBase
    {
        [UnityTest]
        public IEnumerator FailCorrectlyWhenGivenWrongURL()
        {
            yield return base.InitScene();

            var library = new AssetLibrary_AssetBundle();
            var keeper = new AssetPromiseKeeper_AssetBundle(library);

            string url = "non_existing_url.glb";
            string baseUrl = Utils.GetTestsAssetsPath() + "AssetBundles/";

            AssetPromise_AssetBundle prom = new AssetPromise_AssetBundle(baseUrl, url);
            Asset_AssetBundle asset = null;
            bool failEventCalled1 = false;
            prom.OnSuccessEvent += (x) => { asset = x; };
            prom.OnFailEvent += (x) => { failEventCalled1 = true; };

            AssetPromise_AssetBundle prom2 = new AssetPromise_AssetBundle(baseUrl, url);
            Asset_AssetBundle asset2 = null;
            bool failEventCalled2 = false;
            prom2.OnSuccessEvent += (x) => { asset2 = x; };
            prom2.OnFailEvent += (x) => { failEventCalled2 = true; };

            AssetPromise_AssetBundle prom3 = new AssetPromise_AssetBundle(baseUrl, url);
            Asset_AssetBundle asset3 = null;
            bool failEventCalled3 = false;
            prom3.OnSuccessEvent += (x) => { asset3 = x; };
            prom3.OnFailEvent += (x) => { failEventCalled3 = true; };

            keeper.Keep(prom);
            keeper.Keep(prom2);
            keeper.Keep(prom3);

            Assert.AreEqual(3, keeper.waitingPromisesCount);

            yield return prom;

            Assert.AreNotEqual(AssetPromiseState.FINISHED, prom.state);
            Assert.AreNotEqual(AssetPromiseState.FINISHED, prom2.state);
            Assert.AreNotEqual(AssetPromiseState.FINISHED, prom3.state);

            Assert.IsTrue(failEventCalled1);
            Assert.IsTrue(failEventCalled2);
            Assert.IsTrue(failEventCalled3);

            Assert.IsFalse(asset != null);
            Assert.IsFalse(asset2 != null);
            Assert.IsFalse(asset3 != null);

            Assert.IsFalse(library.Contains(asset));
            Assert.AreNotEqual(1, library.masterAssets.Count);
        }
    }
}
