using DCL;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.TestTools;
using UnityGLTF.Cache;

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
            AssetPath gltfPath = new AssetPath(core.finalDownloadedPath, "MyHash", "myModel.gltf");
            AssetPath texturePath = new AssetPath(core.finalDownloadedPath, "MyHash2", "texture.png");

            PersistentAssetCache.ImageCacheByUri.Clear();
            PersistentAssetCache.StreamCacheByUri.Clear();

            core.RetrieveAndInjectTexture(gltfPath, texturePath);

            //TODO: Assert
            //texturePath.finalPath.Contains("texture.png");

            yield break;
        }

        [UnityTest]
        public IEnumerator InjectBufferCorrectly()
        {
            AssetPath gltfPath = new AssetPath(core.finalDownloadedPath, "MyHash", "myModel.gltf");
            AssetPath bufferPath = new AssetPath(core.finalDownloadedPath, "MyHash2", "texture.bin");

            PersistentAssetCache.ImageCacheByUri.Clear();
            PersistentAssetCache.StreamCacheByUri.Clear();

            core.RetrieveAndInjectBuffer(gltfPath, bufferPath);

            //TODO: Assert
            //texturePath.finalPath.Contains("texture.bin");

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

            string basePath = @"C:\test-path\";
            string path1 = $"{basePath}file1.png", path2 = $"{basePath}file2.png";
            string path3 = $"{basePath}file3.png", path4 = $"{basePath}file4.png";

            string hash1 = "QmHash1", hash2 = "QmHash2", hash3 = "QmHash3", hash4 = "QmHash4";

            paths.Add(new AssetPath(basePath, hash1, Path.GetFileName(path1)));
            paths.Add(new AssetPath(basePath, hash2, Path.GetFileName(path2)));
            paths.Add(new AssetPath(basePath, hash3, Path.GetFileName(path3)));
            paths.Add(new AssetPath(basePath, hash4, Path.GetFileName(path4)));

            if (env.webRequest is Mocked.WebRequest mockedReq)
            {
                string baseUrl = @"https://peer.decentraland.org/lambdas/contentv2/contents/";
                mockedReq.mockedContent.Add($"{baseUrl}{hash1}", "TestContent1 - guid: 14e357df3f3b75940b5d59e1035255b1\n");
                mockedReq.mockedContent.Add($"{baseUrl}{hash2}", "TestContent2 - guid: 14e357df3f3b75940b5d59e1035255b2\n");
                mockedReq.mockedContent.Add($"{baseUrl}{hash3}", "TestContent3 - guid: 14e357df3f3b75940b5d59e1035255b3\n");
            }

            string dumpPath1 = $"{basePath}{hash1}\\{hash1}.png";
            string dumpPath2 = $"{basePath}{hash2}\\{hash2}.png";
            string dumpPath3 = $"{basePath}{hash3}\\{hash3}.png";
            string dumpPath4 = $"{basePath}{hash4}\\{hash4}.png";

            string targetGuid1 = ABConverter.Utils.CidToGuid(hash1);
            string targetGuid2 = ABConverter.Utils.CidToGuid(hash2);
            string targetGuid3 = ABConverter.Utils.CidToGuid(hash3);

            LogAssert.ignoreFailingMessages = true;
            var textures = core.DumpSceneTextures(paths);
            LogAssert.ignoreFailingMessages = false;

            Assert.AreEqual(3, textures.Count);

            //NOTE(Brian): textures exist?
            Assert.IsTrue(env.file.Exists(dumpPath1));
            Assert.IsTrue(env.file.Exists(dumpPath2));
            Assert.IsTrue(env.file.Exists(dumpPath3));
            Assert.IsFalse(env.file.Exists(dumpPath4));

            //NOTE(Brian): textures .meta exist?
            Assert.IsTrue(env.file.Exists(Path.ChangeExtension(dumpPath1, "meta")));
            Assert.IsTrue(env.file.Exists(Path.ChangeExtension(dumpPath2, "meta")));
            Assert.IsTrue(env.file.Exists(Path.ChangeExtension(dumpPath3, "meta")));
            Assert.IsFalse(env.file.Exists(Path.ChangeExtension(dumpPath4, "meta")));

            //NOTE(Brian): textures .meta guid is changed?
            Assert.IsTrue(env.file.ReadAllText(Path.ChangeExtension(dumpPath1, "meta")).Contains(targetGuid1));
            Assert.IsTrue(env.file.ReadAllText(Path.ChangeExtension(dumpPath2, "meta")).Contains(targetGuid2));
            Assert.IsTrue(env.file.ReadAllText(Path.ChangeExtension(dumpPath3, "meta")).Contains(targetGuid3));
            yield break;
        }

        [UnityTest]
        public IEnumerator DumpBuffersCorrectly()
        {
            List<AssetPath> paths = new List<AssetPath>();

            string basePath = @"C:\test-path\";
            string path1 = $"{basePath}file1.bin", path2 = $"{basePath}file2.bin";
            string path3 = $"{basePath}file3.bin", path4 = $"{basePath}file4.bin";

            string hash1 = "QmHash1", hash2 = "QmHash2", hash3 = "QmHash3", hash4 = "QmHash4";

            paths.Add(new AssetPath(basePath, hash1, Path.GetFileName(path1)));
            paths.Add(new AssetPath(basePath, hash2, Path.GetFileName(path2)));
            paths.Add(new AssetPath(basePath, hash3, Path.GetFileName(path3)));
            paths.Add(new AssetPath(basePath, hash4, Path.GetFileName(path4)));

            if (env.webRequest is Mocked.WebRequest mockedReq)
            {
                string baseUrl = @"https://peer.decentraland.org/lambdas/contentv2/contents/";
                mockedReq.mockedContent.Add($"{baseUrl}{hash1}", "Whatever1");
                mockedReq.mockedContent.Add($"{baseUrl}{hash2}", "Whatever2");
                mockedReq.mockedContent.Add($"{baseUrl}{hash3}", "Whatever3");
            }

            string dumpPath1 = $"{basePath}{hash1}\\{hash1}.bin";
            string dumpPath2 = $"{basePath}{hash2}\\{hash2}.bin";
            string dumpPath3 = $"{basePath}{hash3}\\{hash3}.bin";
            string dumpPath4 = $"{basePath}{hash4}\\{hash4}.bin";

            LogAssert.ignoreFailingMessages = true;
            var buffers = core.DumpSceneBuffers(paths);
            LogAssert.ignoreFailingMessages = false;

            Assert.AreEqual(3, buffers.Count);

            //NOTE(Brian): textures exist?
            Assert.IsTrue(env.file.Exists(dumpPath1));
            Assert.IsTrue(env.file.Exists(dumpPath2));
            Assert.IsTrue(env.file.Exists(dumpPath3));
            Assert.IsFalse(env.file.Exists(dumpPath4));

            //NOTE(Brian): textures .meta exist?
            Assert.IsTrue(env.file.Exists(Path.ChangeExtension(dumpPath1, "meta")));
            Assert.IsTrue(env.file.Exists(Path.ChangeExtension(dumpPath2, "meta")));
            Assert.IsTrue(env.file.Exists(Path.ChangeExtension(dumpPath3, "meta")));
            Assert.IsFalse(env.file.Exists(Path.ChangeExtension(dumpPath4, "meta")));

            yield break;
        }


        [UnityTest]
        public IEnumerator DownloadAssetCorrectly()
        {
            AssetPath path = new AssetPath(
                basePath: @"C:\Base-Path",
                hash: "QmTestHash",
                file: "texture.png"
            );

            if (env.webRequest is Mocked.WebRequest mockedReq)
            {
                string baseUrl = @"https://peer.decentraland.org/lambdas/contentv2/contents/";
                mockedReq.mockedContent.Add($"{baseUrl}QmTestHash", "Whatever1");
            }

            string output = core.DownloadAsset(path);

            UnityEngine.Assertions.Assert.IsTrue(env.file.Exists(output));
            UnityEngine.Assertions.Assert.IsTrue(env.file.Exists(Path.ChangeExtension(output, "meta")));
            UnityEngine.Assertions.Assert.AreEqual("Whatever1", env.file.ReadAllText(output));

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
            //TODO(Brian): Implement later 
            yield break;
        }

        [UnityTest]
        public IEnumerator DumpSceneCorrectly()
        {
            //TODO(Brian): Implement later 
            yield break;
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

            UnityEditor.AssetDatabase.Refresh();
        }
    }
}