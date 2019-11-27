using DCL;
using DCL.Helpers;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace AssetPromiseKeeper_AssetBundleModel_Tests
{
    public class AB_GameObject_TestsBase
    {
        protected readonly static string TEST_AB_FILENAME = "QmYACL8SnbXEonXQeRHdWYbfm8vxvaFAWnsLHUaDG4ABp5";
        protected readonly static string BASE_URL = Utils.GetTestsAssetsPath() + "/AssetBundles/";

        protected AssetLibrary_AB_GameObject library;
        protected AssetPromiseKeeper_AB_GameObject keeper;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            library = new AssetLibrary_AB_GameObject();
            keeper = new AssetPromiseKeeper_AB_GameObject(library);
            yield break;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            PoolManager.i.Cleanup();
            library.Cleanup();
            Caching.ClearCache();
            Resources.UnloadUnusedAssets();
            yield break;
        }
    }
}
