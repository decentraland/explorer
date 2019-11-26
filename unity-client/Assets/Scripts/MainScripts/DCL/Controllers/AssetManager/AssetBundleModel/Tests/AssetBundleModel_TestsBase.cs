using DCL;
using DCL.Helpers;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace AssetPromiseKeeper_AssetBundleModel_Tests
{
    public class AssetBundleModel_TestsBase
    {
        protected readonly static string TEST_AB_FILENAME = "QmYACL8SnbXEonXQeRHdWYbfm8vxvaFAWnsLHUaDG4ABp5";
        protected readonly static string BASE_URL = Utils.GetTestsAssetsPath() + "/AssetBundles/";

        protected AssetLibrary_AssetBundleModel library;
        protected AssetPromiseKeeper_AssetBundleModel keeper;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            library = new AssetLibrary_AssetBundleModel();
            keeper = new AssetPromiseKeeper_AssetBundleModel(library);
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
    }
}
