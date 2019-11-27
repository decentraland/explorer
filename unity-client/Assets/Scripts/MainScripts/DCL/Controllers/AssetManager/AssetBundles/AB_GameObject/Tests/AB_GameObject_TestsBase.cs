using DCL;
using DCL.Helpers;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace AssetPromiseKeeper_AssetBundleModel_Tests
{
    public class AB_GameObject_TestsBase
    {
        protected readonly static string TEST_AB_FILENAME = "QmNS4K7GaH63T9rhAfkrra7ADLXSEeco8FTGknkPnAVmKM";
        protected readonly static string BASE_URL = Utils.GetTestsAssetsPath() + "/AssetBundles/";

        protected AssetPromiseKeeper_AB_GameObject keeper;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            keeper = new AssetPromiseKeeper_AB_GameObject();
            yield break;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            PoolManager.i.Cleanup();
            keeper.Cleanup();
            Caching.ClearCache();
            Resources.UnloadUnusedAssets();
            yield break;
        }
    }
}
