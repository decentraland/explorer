using DCL;
using DCL.Helpers;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace AssetPromiseKeeper_AssetBundle_Tests
{
    public class AB_TestsBase
    {
        protected readonly static string TEST_AB_FILENAME = "QmYACL8SnbXEonXQeRHdWYbfm8vxvaFAWnsLHUaDG4ABp5";
        protected readonly static string BASE_URL = Utils.GetTestsAssetsPath() + "/AssetBundles/";

        protected AssetPromiseKeeper_AB keeper;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            keeper = new AssetPromiseKeeper_AB();
            yield break;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            keeper.Cleanup();
            Caching.ClearCache();
            Resources.UnloadUnusedAssets();
            yield break;
        }
    }
}
