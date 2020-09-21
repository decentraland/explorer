using DCL;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;
using AssetDatabase = UnityEditor.AssetDatabase;
using Directory = System.IO.Directory;
using File = System.IO.File;

namespace ABConverterTests
{
    public class ABConverterShould
    {
        private ABConverter.Core core;
        private EditorEnvironment env;

        [SetUp]
        public void SetUp()
        {
            ResetCacheAndWorkingFolders();

            var settings = new ABConverter.Client.Settings();
            settings.baseUrl = ContentServerUtils.GetBaseUrl(ContentServerUtils.ApiTLD.ZONE) + "/contents/";
            settings.verbose = true;
            settings.deleteDownloadPathAfterFinished = false;

            env = EditorEnvironment.CreateWithMockImplementations();
            core = new ABConverter.Core(env, settings);
        }

        [TearDown]
        public void TearDown()
        {
            //ResetCacheAndWorkingFolders();
        }

        [Test]
        public void PopulateLowercaseMappingsCorrectly()
        {
            var pairs = new List<ContentServerUtils.MappingPair>();

            pairs.Add(new ContentServerUtils.MappingPair() {file = "foo", hash = "tEsT1"});
            pairs.Add(new ContentServerUtils.MappingPair() {file = "foo", hash = "Test2"});
            pairs.Add(new ContentServerUtils.MappingPair() {file = "foo", hash = "tesT3"});
            pairs.Add(new ContentServerUtils.MappingPair() {file = "foo", hash = "teSt4"});

            core.PopulateLowercaseMappings(pairs.ToArray());

            Assert.IsTrue(core.hashLowercaseToHashProper.ContainsKey("test1"));
            Assert.IsTrue(core.hashLowercaseToHashProper.ContainsKey("test2"));
            Assert.IsTrue(core.hashLowercaseToHashProper.ContainsKey("test3"));
            Assert.IsTrue(core.hashLowercaseToHashProper.ContainsKey("test4"));

            Assert.AreEqual("tEsT1", core.hashLowercaseToHashProper["test1"]);
            Assert.AreEqual("Test2", core.hashLowercaseToHashProper["test2"]);
            Assert.AreEqual("tesT3", core.hashLowercaseToHashProper["test3"]);
            Assert.AreEqual("teSt4", core.hashLowercaseToHashProper["test4"]);
        }

        [Test]
        public void InitializeDirectoryPathsCorrectly()
        {
            env = EditorEnvironment.CreateWithDefaultImplementations();

            core.InitializeDirectoryPaths(false);

            Assert.IsFalse(string.IsNullOrEmpty(core.settings.finalAssetBundlePath));
            Assert.IsFalse(string.IsNullOrEmpty(core.finalDownloadedPath));

            Assert.IsTrue(env.directory.Exists(core.settings.finalAssetBundlePath));
            Assert.IsTrue(env.directory.Exists(core.finalDownloadedPath));

            string file1 = core.settings.finalAssetBundlePath + "test.txt";
            string file2 = core.finalDownloadedPath + "test.txt";

            env.file.WriteAllText(file1, "test");
            env.file.WriteAllText(file2, "test");

            core.InitializeDirectoryPaths(true);

            Assert.IsFalse(env.file.Exists(file1));
            Assert.IsFalse(env.file.Exists(file2));
        }

        [UnityTest]
        public IEnumerator InjectTexturesCorrectly()
        {
            AssetPath gltfPath = new AssetPath("", new ContentServerUtils.MappingPair());
            AssetPath texturePath = new AssetPath("", new ContentServerUtils.MappingPair());

            core.RetrieveAndInjectTexture(gltfPath, texturePath);

            //TODO: Assert
            yield break;
        }

        [UnityTest]
        public IEnumerator InjectBufferCorrectly()
        {
            AssetPath gltfPath = new AssetPath("", new ContentServerUtils.MappingPair());
            AssetPath bufferPath = new AssetPath("", new ContentServerUtils.MappingPair());

            core.RetrieveAndInjectBuffer(gltfPath, bufferPath);

            //TODO: Assert

            yield break;
        }

        [UnityTest]
        public IEnumerator CheckAlreadyGeneratedAssetsCorrectly()
        {
            List<AssetPath> gltfPaths = new List<AssetPath>();
            core.PrepareDump(ref gltfPaths);

            //TODO: Assert
            yield break;
        }

        [UnityTest]
        public IEnumerator AssetBundleFolderIsBeingCleanedProperly()
        {
            //TODO: Arrange
            //TODO: Act
            //TODO: Assert
            yield break;
        }

        [UnityTest]
        public IEnumerator DumpGLTFCorrectly()
        {
            List<AssetPath> texturePaths = new List<AssetPath>();
            List<AssetPath> bufferPaths = new List<AssetPath>();
            AssetPath gltfPath = new AssetPath("", new ContentServerUtils.MappingPair());

            core.DumpGltf(gltfPath, texturePaths, bufferPaths);

            //TODO: Assert
            yield break;
        }

        [UnityTest]
        public IEnumerator DumpTexturesCorrectly()
        {
            List<AssetPath> paths = new List<AssetPath>();

            core.DumpSceneTextures(paths);

            //TODO: Assert
            yield break;
        }

        [UnityTest]
        public IEnumerator DumpBuffersCorrectly()
        {
            List<AssetPath> paths = new List<AssetPath>();

            core.DumpSceneBuffers(paths);

            //TODO: Assert
            yield break;
        }


        [UnityTest]
        public IEnumerator DownloadAssetCorrectly()
        {
            AssetPath path = new AssetPath(
                basePath: core.finalDownloadedPath,
                hash: "texture.png",
                file: "QmPLdeHkHbT1SEdouMJLFRAubYr4jiQEut9HhKyuQ8oK8V"
            );

            string output = core.DownloadAsset(path);

            UnityEngine.Assertions.Assert.IsTrue(env.file.Exists(output));

            //TODO: Assert
            yield break;
        }

        [UnityTest]
        public IEnumerator ConvertAssetsWithExternalDependenciesCorrectly()
        {
            env = EditorEnvironment.CreateWithDefaultImplementations();

            var state = ABConverter.Client.DumpArea(
                new Vector2Int(-110, -110),
                new Vector2Int(1, 1),
                ContentServerUtils.ApiTLD.ZONE,
                core.settings);

            yield return new WaitUntil(() => state.step == ABConverter.Core.State.Step.FINISHED);

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

        [UnityTest]
        public IEnumerator DumpAreaCorrectly()
        {
            //TODO: Arrange

            var state = ABConverter.Client.DumpArea(
                new Vector2Int(-110, -110),
                new Vector2Int(1, 1),
                ContentServerUtils.ApiTLD.ZONE,
                core.settings);

            yield return new WaitUntil(() => state.step == ABConverter.Core.State.Step.FINISHED);

            //TODO: Assert
        }

        [UnityTest]
        public IEnumerator DumpSceneCorrectly()
        {
            //TODO: Arrange

            var state = ABConverter.Client.DumpScene("asd", ContentServerUtils.ApiTLD.ORG, core.settings);

            yield return new WaitUntil(() => state.step == ABConverter.Core.State.Step.FINISHED);

            //TODO: Assert
        }

        void ResetCacheAndWorkingFolders()
        {
            Caching.ClearCache();

            if (Directory.Exists(AssetBundleConverterConfig.ASSET_BUNDLES_PATH_ROOT))
                Directory.Delete(AssetBundleConverterConfig.ASSET_BUNDLES_PATH_ROOT, true);

            if (Directory.Exists(AssetBundleConverterConfig.DOWNLOADED_PATH_ROOT))
                Directory.Delete(AssetBundleConverterConfig.DOWNLOADED_PATH_ROOT, true);

            if (File.Exists(AssetBundleConverterConfig.DOWNLOADED_PATH_ROOT + ".meta"))
                File.Delete(AssetBundleConverterConfig.DOWNLOADED_PATH_ROOT + ".meta");

            AssetDatabase.Refresh();
        }
    }
}