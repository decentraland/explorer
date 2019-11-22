using DCL;
using DCL.Helpers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.TestTools;

namespace AssetPromiseKeeper_AssetBundleModel_Tests
{
    public class AnyAssetPromiseShould : TestsBase
    {
        const string TEST_AB_FILENAME = "QmYACL8SnbXEonXQeRHdWYbfm8vxvaFAWnsLHUaDG4ABp5";

        [UnityTest]
        public IEnumerator BeSetupCorrectlyAfterLoad()
        {
            yield return base.InitScene();

            var library = new AssetLibrary_AssetBundleModel();
            var keeper = new AssetPromiseKeeper_AssetBundleModel(library);

            string baseUrl = Utils.GetTestsAssetsPath() + "AssetBundles/";
            string url = TEST_AB_FILENAME;

            var prom = new AssetPromise_AssetBundleModel(baseUrl, url);
            Asset_AssetBundleModel loadedAsset = null;


            prom.OnSuccessEvent +=
                (x) =>
                {
                    loadedAsset = x;
                }
            ;

            Vector3 initialPos = Vector3.one;
            Quaternion initialRot = Quaternion.LookRotation(Vector3.right, Vector3.up);
            Vector3 initialScale = Vector3.one * 2;

            prom.settings.initialLocalPosition = initialPos;
            prom.settings.initialLocalRotation = initialRot;
            prom.settings.initialLocalScale = initialScale;

            keeper.Keep(prom);

            yield return prom;

            Assert.IsTrue(PoolManager.i.ContainsPool(loadedAsset.id), "Not in pool after loaded!");

            Pool pool = PoolManager.i.GetPool(loadedAsset.id);

            Assert.AreEqual(0, pool.inactiveCount, "incorrect inactive objects in pool");
            Assert.AreEqual(1, pool.activeCount, "incorrect active objects in pool");
            Assert.IsTrue(pool.original != loadedAsset.container, "In pool, the original gameObject must NOT be the loaded asset!");

            //NOTE(Brian): If the following asserts fail, check that ApplySettings_LoadStart() is called from AssetPromise_GLTF.AddToLibrary() when the clone is made.
            Assert.AreEqual(initialPos.ToString(), loadedAsset.container.transform.localPosition.ToString(), "initial position not set correctly!");
            Assert.AreEqual(initialRot.ToString(), loadedAsset.container.transform.localRotation.ToString(), "initial rotation not set correctly!");
            Assert.AreEqual(initialScale.ToString(), loadedAsset.container.transform.localScale.ToString(), "initial scale not set correctly!");

            Assert.IsTrue(loadedAsset != null);
            Assert.IsTrue(library.Contains(loadedAsset));
            Assert.AreEqual(1, library.masterAssets.Count);
        }

        [UnityTest]
        public IEnumerator ForceNewInstanceIsOff()
        {
            yield return InitScene();
            var library = new AssetLibrary_AssetBundleModel();
            var keeper = new AssetPromiseKeeper_AssetBundleModel(library);

            string baseUrl = Utils.GetTestsAssetsPath() + "AssetBundles/";
            string url = TEST_AB_FILENAME;
            var prom = new AssetPromise_AssetBundleModel(baseUrl, url);
            prom.settings.forceNewInstance = false;
            keeper.Keep(prom);
            yield return prom;

            //var poolableObjectComponent = prom.asset.container.GetComponentInChildren<PoolableObject>();
            //Assert.IsNotNull(poolableObjectComponent);
        }

        [UnityTest]
        public IEnumerator ForceNewInstanceIsOffMultipleTimes()
        {
            yield return InitScene();
            var library = new AssetLibrary_AssetBundleModel();
            var keeper = new AssetPromiseKeeper_AssetBundleModel(library);

            var poolableComponents = new List<PoolableObject>();

            string baseUrl = Utils.GetTestsAssetsPath() + "AssetBundles/";
            string url = TEST_AB_FILENAME;

            for (int i = 0; i < 10; i++)
            {
                var prom = new AssetPromise_AssetBundleModel(baseUrl, url);
                prom.settings.forceNewInstance = false;
                keeper.Keep(prom);
                yield return prom;
                //poolableComponents.Add(prom.asset.container.GetComponentInChildren<PoolableObject>());
                keeper.Forget(prom);
            }

            Assert.IsTrue(poolableComponents.TrueForAll(x => x != null));
        }

        [UnityTest]
        public IEnumerator ForceNewInstanceIsOn()
        {
            yield return InitScene();
            var library = new AssetLibrary_AssetBundleModel();
            var keeper = new AssetPromiseKeeper_AssetBundleModel(library);

            string baseUrl = Utils.GetTestsAssetsPath() + "AssetBundles/";
            string url = TEST_AB_FILENAME;

            var prom = new AssetPromise_AssetBundleModel(baseUrl, url);
            prom.settings.forceNewInstance = true;
            keeper.Keep(prom);
            yield return prom;

            //var poolableObjectComponent = prom.asset.container.GetComponentInChildren<PoolableObject>();
            //Assert.IsNull(poolableObjectComponent);
        }

        [UnityTest]
        public IEnumerator ForceNewInstanceIsOnMultipleTimes()
        {
            yield return InitScene();
            var library = new AssetLibrary_AssetBundleModel();
            var keeper = new AssetPromiseKeeper_AssetBundleModel(library);

            var poolableComponents = new List<PoolableObject>();

            string baseUrl = Utils.GetTestsAssetsPath() + "AssetBundles/";
            string url = TEST_AB_FILENAME;

            for (int i = 0; i < 10; i++)
            {
                var prom = new AssetPromise_AssetBundleModel(baseUrl, url);
                prom.settings.forceNewInstance = true;
                keeper.Keep(prom);
                yield return prom;
                //poolableComponents.Add(prom.asset.container.GetComponentInChildren<PoolableObject>());
                keeper.Forget(prom);
            }

            Assert.IsTrue(poolableComponents.TrueForAll(x => x == null));
        }
    }
}
