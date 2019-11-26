using DCL;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.TestTools;

namespace AssetPromiseKeeper_AssetBundleModel_Tests
{
    public class BlockedAndMasterPromisesShould : AssetBundleModel_TestsBase
    {
        [UnityTest]
        public IEnumerator SucceedWhenMastersParentIsDestroyed()
        {
            GameObject parent = new GameObject("parent");

            AssetPromise_AssetBundleModel prom = new AssetPromise_AssetBundleModel(BASE_URL, TEST_AB_FILENAME);
            prom.settings.parent = parent.transform;

            AssetPromise_AssetBundleModel prom2 = new AssetPromise_AssetBundleModel(BASE_URL, TEST_AB_FILENAME);
            bool failEventCalled2 = false;
            prom2.OnFailEvent += (x) => { failEventCalled2 = true; };

            AssetPromise_AssetBundleModel prom3 = new AssetPromise_AssetBundleModel(BASE_URL, TEST_AB_FILENAME);
            bool failEventCalled3 = false;
            prom3.OnFailEvent += (x) => { failEventCalled3 = true; };

            keeper.Keep(prom);
            keeper.Keep(prom2);
            keeper.Keep(prom3);

            keeper.Forget(prom);

            Assert.AreEqual(3, keeper.waitingPromisesCount);

            Object.Destroy(parent);

            yield return prom;
            yield return prom2;
            yield return prom3;

            Assert.AreEqual(AssetPromiseState.FINISHED, prom.state);
            Assert.AreEqual(AssetPromiseState.FINISHED, prom2.state);
            Assert.AreEqual(AssetPromiseState.FINISHED, prom3.state);

            Assert.IsFalse(failEventCalled2);
            Assert.IsFalse(failEventCalled3);

            Assert.IsTrue(prom.asset != null);
            Assert.IsTrue(prom2.asset != null);
            Assert.IsTrue(prom3.asset != null);

            Assert.IsTrue(library.Contains(prom.asset));
            Assert.AreEqual(1, library.masterAssets.Count);
        }

        [UnityTest]
        public IEnumerator FailCorrectlyWhenGivenWrongURL()
        {
            var library = new AssetLibrary_AssetBundleModel();
            var keeper = new AssetPromiseKeeper_AssetBundleModel(library);

            //NOTE(Brian): Expect the 404 error
            //LogAssert.Expect(LogType.Error, new Regex("^*.?404"));

            string url = "non_existing_url.glb";

            AssetPromise_AssetBundleModel prom = new AssetPromise_AssetBundleModel(BASE_URL, url);
            Asset_AssetBundleModel asset = null;
            bool failEventCalled1 = false;
            prom.OnSuccessEvent += (x) => { asset = x; };
            prom.OnFailEvent += (x) => { failEventCalled1 = true; };

            AssetPromise_AssetBundleModel prom2 = new AssetPromise_AssetBundleModel(BASE_URL, url);
            Asset_AssetBundleModel asset2 = null;
            bool failEventCalled2 = false;
            prom2.OnSuccessEvent += (x) => { asset2 = x; };
            prom2.OnFailEvent += (x) => { failEventCalled2 = true; };

            AssetPromise_AssetBundleModel prom3 = new AssetPromise_AssetBundleModel(BASE_URL, url);
            Asset_AssetBundleModel asset3 = null;
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
