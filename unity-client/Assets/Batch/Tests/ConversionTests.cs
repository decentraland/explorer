using DCL;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;
using AssetDatabase = UnityEditor.AssetDatabase;
using Directory = System.IO.Directory;
using File = System.IO.File;

namespace AssetBundleConversionTests
{
    public class ABConverterShould
    {
        void ResetCacheAndWorkingFolders()
        {
            Caching.ClearCache();

            if (Directory.Exists(AssetBundleConverterConfig.ASSET_BUNDLES_PATH_ROOT))
                Directory.Delete(AssetBundleConverterConfig.ASSET_BUNDLES_PATH_ROOT, true);

            if (Directory.Exists(AssetBundleConverterConfig.DOWNLOADED_PATH_ROOT))
                Directory.Delete(AssetBundleConverterConfig.DOWNLOADED_PATH_ROOT, true);

            AssetDatabase.Refresh();
        }

        [SetUp]
        public void SetUp()
        {
            ResetCacheAndWorkingFolders();
        }

        [TearDown]
        public void TearDown()
        {
            ResetCacheAndWorkingFolders();
        }

        [Test]
        public void PopulateLowercaseMappingsCorrectly()
        {
            var builder = new AssetBundleConverterCore(EditorEnvironment.CreateWithDefaultImplementations());
            var pairs = new List<ContentServerUtils.MappingPair>();

            pairs.Add(new ContentServerUtils.MappingPair() {file = "foo", hash = "tEsT1"});
            pairs.Add(new ContentServerUtils.MappingPair() {file = "foo", hash = "Test2"});
            pairs.Add(new ContentServerUtils.MappingPair() {file = "foo", hash = "tesT3"});
            pairs.Add(new ContentServerUtils.MappingPair() {file = "foo", hash = "teSt4"});

            builder.PopulateLowercaseMappings(pairs.ToArray());

            Assert.IsTrue(builder.hashLowercaseToHashProper.ContainsKey("test1"));
            Assert.IsTrue(builder.hashLowercaseToHashProper.ContainsKey("test2"));
            Assert.IsTrue(builder.hashLowercaseToHashProper.ContainsKey("test3"));
            Assert.IsTrue(builder.hashLowercaseToHashProper.ContainsKey("test4"));

            Assert.AreEqual("tEsT1", builder.hashLowercaseToHashProper["test1"]);
            Assert.AreEqual("Test2", builder.hashLowercaseToHashProper["test2"]);
            Assert.AreEqual("tesT3", builder.hashLowercaseToHashProper["test3"]);
            Assert.AreEqual("teSt4", builder.hashLowercaseToHashProper["test4"]);
        }

        [Test]
        public void InitializeDirectoryPathsCorrectly()
        {
            var core = new AssetBundleConverterCore(EditorEnvironment.CreateWithDefaultImplementations());
            core.InitializeDirectoryPaths(false);

            Assert.IsFalse(string.IsNullOrEmpty(core.settings.finalAssetBundlePath));
            Assert.IsFalse(string.IsNullOrEmpty(core.finalDownloadedPath));

            Assert.IsTrue(Directory.Exists(core.settings.finalAssetBundlePath));
            Assert.IsTrue(Directory.Exists(core.finalDownloadedPath));

            string file1 = core.settings.finalAssetBundlePath + "test.txt";
            string file2 = core.finalDownloadedPath + "test.txt";

            File.WriteAllText(file1, "test");
            File.WriteAllText(file2, "test");

            core.InitializeDirectoryPaths(true);

            Assert.IsFalse(File.Exists(file1));
            Assert.IsFalse(File.Exists(file2));
        }

        [UnityTest]
        public IEnumerator ConvertAssetsWithExternalDependenciesCorrectly()
        {
            var settings = new AssetBundleConverter.Settings();
            settings.baseUrl = ContentServerUtils.GetBaseUrl(ContentServerUtils.ApiTLD.ZONE);

            AssetBundleConverter.EnsureEnvironment();

            var state = AssetBundleConverter.DumpArea(
                new Vector2Int(-110, -110),
                new Vector2Int(1, 1),
                ContentServerUtils.ApiTLD.ZONE,
                settings);

            yield return new WaitUntil(() => state.step == AssetBundleConverterCore.State.Step.FINISHED);

            AssetBundle abDependency = AssetBundle.LoadFromFile(AssetBundleConverterConfig.ASSET_BUNDLES_PATH_ROOT + "/QmWZaHM9CaVpCnsWh78LiNFuiXwjCzTQBTaJ6vZL7c9cbp");
            abDependency.LoadAllAssets();

            AssetBundle abMain = AssetBundle.LoadFromFile(AssetBundleConverterConfig.ASSET_BUNDLES_PATH_ROOT + "/QmS9eDwvcEpyYXChz6pFpyWyfyajiXbt6KA4CxQa3JKPGC");
            Material[] mats = abMain.LoadAllAssets<Material>();

            bool hasMap = false;

            foreach (var mat in mats)
            {
                if (mat.name.ToLowerInvariant().Contains("base grass"))
                    hasMap = mat.GetTexture("_BaseMap") != null;
            }

            abMain.Unload(true);
            abDependency.Unload(true);

            Assert.IsTrue(hasMap, "Dependency has NOT been generated correctly!");
        }
    }
}