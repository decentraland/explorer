using DCL;
using DCL.Helpers;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace AssetPromiseKeeper_AssetBundle_Tests
{
    public class AssetBundle_TestsBase
    {
        protected readonly static string TEST_AB_FILENAME = "QmYACL8SnbXEonXQeRHdWYbfm8vxvaFAWnsLHUaDG4ABp5";
        protected readonly static string BASE_URL = Utils.GetTestsAssetsPath() + "/AssetBundles/";

        protected AssetLibrary_AssetBundle library;
        protected AssetPromiseKeeper_AssetBundle keeper;

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
    }
}
