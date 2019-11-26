using DCL;
using DCL.Helpers;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.TestTools;

namespace AssetPromiseKeeper_AssetBundle_Tests
{
    public class PromiseKeeperShouldBehaveCorrectlyWhen
    {
        readonly static string TEST_AB_FILENAME = "QmYACL8SnbXEonXQeRHdWYbfm8vxvaFAWnsLHUaDG4ABp5";
        readonly static string BASE_URL = Utils.GetTestsAssetsPath() + "/AssetBundles/";

        AssetLibrary_AssetBundle library;
        AssetPromiseKeeper_AssetBundle keeper;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            library = new AssetLibrary_AssetBundle();
            keeper = new AssetPromiseKeeper_AssetBundle(library);
            yield break;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            library.Cleanup();
            Caching.ClearCache();
            Resources.UnloadUnusedAssets();
            yield break;
        }

        [UnityTest]
        public IEnumerator KeepAndForgetIsCalledInSingleFrameWhenLoadingAsset()
        {
            var prom = new AssetPromise_AssetBundle(BASE_URL, TEST_AB_FILENAME);
            bool calledSuccess = false;
            bool calledFail = false;

            prom.OnSuccessEvent +=
                (x) =>
                {
                    calledSuccess = true;
                };

            prom.OnFailEvent +=
                (x) =>
                {
                    calledFail = true;
                };

            keeper.Keep(prom);
            keeper.Forget(prom);
            keeper.Keep(prom);
            keeper.Forget(prom);

            Assert.IsTrue(prom != null);
            Assert.IsTrue(prom.asset == null);
            Assert.IsFalse(calledSuccess);
            Assert.IsTrue(calledFail);

            library.Cleanup();
            yield break;
        }

        [UnityTest]
        public IEnumerator KeepAndForgetIsCalledInSingleFrameWhenReusingAsset()
        {
            var prom = new AssetPromise_AssetBundle(BASE_URL, TEST_AB_FILENAME);
            Asset_AssetBundle loadedAsset = null;

            prom.OnSuccessEvent +=
                (x) =>
                {
                    loadedAsset = x;
                };

            keeper.Keep(prom);
            yield return prom;

            keeper.Forget(prom);
            keeper.Keep(prom);
            keeper.Forget(prom);

            Assert.IsTrue(prom.asset == null);
            library.Cleanup();

        }


        [UnityTest]
        public IEnumerator AnyAssetIsLoadedAndThenUnloaded()
        {
            var prom = new AssetPromise_AssetBundle(BASE_URL, TEST_AB_FILENAME);
            Asset_AssetBundle loadedAsset = null;


            prom.OnSuccessEvent +=
                (x) =>
                {
                    Debug.Log("success!");
                    loadedAsset = x;
                };

            keeper.Keep(prom);

            Assert.IsTrue(prom.state == AssetPromiseState.LOADING);

            yield return prom;

            Assert.IsTrue(loadedAsset != null);
            //Assert.IsTrue(loadedAsset.isLoaded);
            Assert.IsTrue(library.Contains(loadedAsset));
            Assert.AreEqual(1, library.masterAssets.Count);

            keeper.Forget(prom);

            yield return prom;

            Assert.IsTrue(prom.state == AssetPromiseState.IDLE_AND_EMPTY);

            Assert.IsTrue(!library.Contains(loadedAsset.id));
            Assert.AreEqual(0, library.masterAssets.Count);
            library.Cleanup();

        }

        [UnityTest]
        public IEnumerator ForgetIsCalledWhileAssetIsBeingLoaded()
        {
            var prom = new AssetPromise_AssetBundle(BASE_URL, TEST_AB_FILENAME);
            Asset_AssetBundle asset = null;
            prom.OnSuccessEvent += (x) => { asset = x; };

            keeper.Keep(prom);

            yield return new WaitForSeconds(0.5f);

            keeper.Forget(prom);

            Assert.AreEqual(AssetPromiseState.IDLE_AND_EMPTY, prom.state);

            var prom2 = new AssetPromise_AssetBundle(BASE_URL, TEST_AB_FILENAME);

            keeper.Keep(prom2);

            yield return prom2;

            Assert.AreEqual(AssetPromiseState.FINISHED, prom2.state);

            keeper.Forget(prom2);

            Assert.IsTrue(asset.ownerAssetBundle == null);
            Assert.IsTrue(!library.Contains(asset));
            Assert.AreEqual(0, library.masterAssets.Count);
        }

        [UnityTest]
        public IEnumerator ManyPromisesWithTheSameURLAreLoaded()
        {
            string id = "1";
            var prom = new AssetPromise_AssetBundle(BASE_URL, TEST_AB_FILENAME);
            Asset_AssetBundle asset = null;
            prom.OnSuccessEvent += (x) => { asset = x; };

            var prom2 = new AssetPromise_AssetBundle(BASE_URL, TEST_AB_FILENAME);
            Asset_AssetBundle asset2 = null;
            prom2.OnSuccessEvent += (x) => { asset2 = x; };

            var prom3 = new AssetPromise_AssetBundle(BASE_URL, TEST_AB_FILENAME);
            Asset_AssetBundle asset3 = null;
            prom3.OnSuccessEvent += (x) => { asset3 = x; };

            keeper.Keep(prom);
            keeper.Keep(prom2);
            keeper.Keep(prom3);

            Assert.AreEqual(3, keeper.waitingPromisesCount);

            yield return prom;
            yield return new WaitForSeconds(2.0f);

            Assert.IsTrue(asset != null);
            Assert.IsTrue(asset2 != null);
            Assert.IsTrue(asset3 != null);

            Assert.AreEqual(AssetPromiseState.FINISHED, prom.state);
            Assert.AreEqual(AssetPromiseState.FINISHED, prom2.state);
            Assert.AreEqual(AssetPromiseState.FINISHED, prom3.state);

            Assert.IsTrue(asset2.id == asset.id);
            Assert.IsTrue(asset3.id == asset.id);
            Assert.IsTrue(asset2.id == asset3.id);

            //NOTE(Brian): We expect them to be the same asset because AssetBundle non-gameObject assets are shared, as opposed to instanced.
            Assert.IsTrue(asset == asset2);
            Assert.IsTrue(asset == asset3);
            Assert.IsTrue(asset2 == asset3);

            Assert.IsTrue(library.Contains(asset));
            Assert.AreEqual(1, library.masterAssets.Count);
        }
    }
}
