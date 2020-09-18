using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityGLTF;
using UnityGLTF.Cache;

namespace DCL
{
    public class AssetBundleConverterCore
    {
        public enum ErrorCodes
        {
            SUCCESS = 0,
            UNDEFINED = 1,
            SCENE_LIST_NULL = 2,
            ASSET_BUNDLE_BUILD_FAIL = 3,
        }

        private const string MAIN_SHADER_AB_NAME = "MainShader_Delete_Me";

        public Dictionary<string, string> hashLowercaseToHashProper = new Dictionary<string, string>();

        internal string finalAssetBundlePath;
        internal readonly string finalDownloadedPath;

        public AssetBundleConverter.Settings settings;

        private float startTime;
        private int totalAssets;
        private int skippedAssets;

        private EditorEnvironment env;
        private static Logger log = new Logger(nameof(AssetBundleConverterCore));
        private string logBuffer;

        public AssetBundleConverterCore(EditorEnvironment env, AssetBundleConverter.Settings settings = null)
        {
            this.env = env;

            this.settings = settings ?? new AssetBundleConverter.Settings();

            finalAssetBundlePath = AssetBundleConverterConfig.ASSET_BUNDLES_PATH_ROOT + "/";
            finalDownloadedPath = AssetBundleConverterConfig.DOWNLOADED_PATH_ROOT + "/";
            log.verboseEnabled = settings.verbose;
        }

        public void CleanAndExit(ErrorCodes errorCode)
        {
            float conversionTime = Time.realtimeSinceStartup - startTime;
            logBuffer = $"Conversion finished!. error code = {errorCode}";

            logBuffer += "\n";
            logBuffer += $"Converted {totalAssets - skippedAssets} of {totalAssets}. (Skipped {skippedAssets})\n";
            logBuffer += $"Total time: {conversionTime}";

            if (totalAssets > 0)
            {
                logBuffer += $"... Time per asset: {conversionTime / totalAssets}\n";
            }

            logBuffer += "\n";
            logBuffer += logBuffer;

            log.Info(logBuffer);

            CleanupWorkingFolders();
            AssetBundleBuilderUtils.Exit((int) errorCode);
        }


        private List<AssetPath> DumpSceneTextures(List<AssetPath> textureAssetPaths)
        {
            List<AssetPath> result = new List<AssetPath>(textureAssetPaths);

            foreach (var assetPath in textureAssetPaths)
            {
                //NOTE(Brian): try to get an AB before getting the original texture, so we bind the dependencies correctly
                string fullPathToTag = DownloadAsset(assetPath);

                env.assetDatabase.ImportAsset(assetPath.finalPath, ImportAssetOptions.ForceUpdate);
                env.assetDatabase.SaveAssets();

                string metaPath = env.assetDatabase.GetTextMetaFilePathFromAssetPath(assetPath.finalPath);

                env.assetDatabase.ReleaseCachedFileHandles();

                //NOTE(Brian): in asset bundles, all dependencies are resolved by their guid (and not the AB hash nor CRC)
                //             So to ensure dependencies are being kept in subsequent editor runs we normalize the asset guid using
                //             the CID.
                string metaContent = env.file.ReadAllText(metaPath);
                string guid = AssetBundleBuilderUtils.CidToGuid(assetPath.hash);
                string newMetaContent = Regex.Replace(metaContent, @"guid: \w+?\n", $"guid: {guid}\n");

                //NOTE(Brian): We must do this hack in order to the new guid to be added to the AssetDatabase
                //             in windows, an AssetImporter.SaveAndReimport call makes the trick, but this won't work
                //             in Unix based OSes for some reason.
                env.file.Delete(metaPath);

                env.file.Copy(assetPath.finalPath, finalDownloadedPath + "tmp");
                env.assetDatabase.DeleteAsset(assetPath.finalPath);
                env.file.Delete(assetPath.finalPath);

                env.assetDatabase.Refresh();
                env.assetDatabase.SaveAssets();

                env.file.Copy(finalDownloadedPath + "tmp", assetPath.finalPath);
                env.file.WriteAllText(metaPath, newMetaContent);
                env.file.Delete(finalDownloadedPath + "tmp");

                env.assetDatabase.Refresh();
                env.assetDatabase.SaveAssets();

                log.Verbose($"content = {env.file.ReadAllText(metaPath)}");
                log.Verbose("guid should be " + guid);
                log.Verbose("guid is " + env.assetDatabase.AssetPathToGUID(assetPath.finalPath));

                if (fullPathToTag == null)
                {
                    result.Remove(assetPath);
                    log.Error("Failed to get texture dependencies! failing asset: " + assetPath.hash);
                }
            }

            return result;
        }

        private void MarkShaderAssetBundle()
        {
            //NOTE(Brian): We tag the main shader, so all the asset bundles don't contain repeated shader assets.
            //             This way we save the big Shader.Parse and gpu compiling performance overhead and make
            //             the bundles a bit lighter.

            //             This shader bundle doesn't need to be really used, as we are going to use the 
            //             embedded one, so we are going to delete it after the generation ended.
            var mainShader = Shader.Find("DCL/LWRP/Lit");
            AssetBundleBuilderUtils.MarkForAssetBundleBuild(env.assetDatabase, mainShader, MAIN_SHADER_AB_NAME);
        }


        private bool DumpAssets(ContentServerUtils.MappingPair[] rawContents)
        {
            List<AssetPath> gltfPaths = AssetBundleBuilderUtils.GetPathsFromPairs(finalDownloadedPath, rawContents, AssetBundleConverterConfig.gltfExtensions);
            List<AssetPath> bufferPaths = AssetBundleBuilderUtils.GetPathsFromPairs(finalDownloadedPath, rawContents, AssetBundleConverterConfig.bufferExtensions);
            List<AssetPath> texturePaths = AssetBundleBuilderUtils.GetPathsFromPairs(finalDownloadedPath, rawContents, AssetBundleConverterConfig.textureExtensions);

            List<AssetPath> assetsToMark = new List<AssetPath>();

            if (!PrepareDump(ref gltfPaths))
                return false;

            //NOTE(Brian): Prepare textures and buffers. We should prepare all the dependencies in this phase.
            assetsToMark.AddRange(DumpSceneTextures(texturePaths));
            DumpSceneBuffers(bufferPaths);

            GLTFImporter.OnGLTFRootIsConstructed -= AssetBundleBuilderUtils.FixGltfRootInvalidUriCharacters;
            GLTFImporter.OnGLTFRootIsConstructed += AssetBundleBuilderUtils.FixGltfRootInvalidUriCharacters;

            //NOTE(Brian): Prepare gltfs gathering its dependencies first and filling the importer's static cache.
            foreach (var gltfPath in gltfPaths)
            {
                assetsToMark.Add(DumpGltf(gltfPath, texturePaths, bufferPaths));
            }

            env.assetDatabase.Refresh();
            env.assetDatabase.SaveAssets();

            MarkAllAssetBundles(assetsToMark);
            MarkShaderAssetBundle();
            return true;
        }

        internal bool PrepareDump(ref List<AssetPath> gltfPaths)
        {
            bool shouldAbortBecauseAllBundlesExist = true;

            totalAssets += gltfPaths.Count;

            if (settings.skipAlreadyBuiltBundles)
            {
                int gltfCount = gltfPaths.Count;

                gltfPaths = gltfPaths.Where(
                    assetPath =>
                        !env.file.Exists(finalAssetBundlePath + assetPath.hash)).ToList();

                int skippedCount = gltfCount - gltfPaths.Count;
                skippedAssets += skippedCount;
                shouldAbortBecauseAllBundlesExist = gltfPaths.Count == 0;
            }
            else
            {
                shouldAbortBecauseAllBundlesExist = false;
            }

            if (shouldAbortBecauseAllBundlesExist)
            {
                log.Info("All assets in this scene were already generated!. Skipping.");
                return false;
            }

            return true;
        }

        private AssetPath DumpGltf(AssetPath gltfPath, List<AssetPath> texturePaths, List<AssetPath> bufferPaths)
        {
            List<Stream> streamsToDispose = new List<Stream>();

            PersistentAssetCache.ImageCacheByUri.Clear();
            PersistentAssetCache.StreamCacheByUri.Clear();

            foreach (var texturePath in texturePaths)
            {
                RetrieveAndInjectTexture(gltfPath, texturePath);
            }

            foreach (var bufferPath in bufferPaths)
            {
                RetrieveAndInjectBuffer(gltfPath, bufferPath);
            }

            //NOTE(Brian): Finally, load the gLTF. The GLTFImporter will use the PersistentAssetCache to resolve the external dependencies.
            string path = DownloadAsset(gltfPath);

            if (path != null)
            {
                env.assetDatabase.Refresh();
                env.assetDatabase.SaveAssets();
            }

            foreach (var streamDataKvp in PersistentAssetCache.StreamCacheByUri)
            {
                if (streamDataKvp.Value.stream != null)
                    streamsToDispose.Add(streamDataKvp.Value.stream);
            }

            foreach (var s in streamsToDispose)
            {
                s.Dispose();
            }

            return path != null ? gltfPath : null;
        }

        private void DumpSceneBuffers(List<AssetPath> bufferPaths)
        {
            foreach (var assetPath in bufferPaths)
            {
                var result = DownloadAsset(assetPath);

                if (result == null)
                {
                    throw new Exception("Failed to get buffer dependencies! failing asset: " + assetPath.hash);
                }
            }
        }

        private void MarkAllAssetBundles(List<AssetPath> assetPaths)
        {
            foreach (var assetPath in assetPaths)
            {
                AssetBundleBuilderUtils.MarkForAssetBundleBuild(assetPath.finalPath, assetPath.hash);
            }
        }


        protected virtual bool BuildAssetBundles(out AssetBundleManifest manifest)
        {
            env.assetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate | ImportAssetOptions.ImportRecursive);
            env.assetDatabase.SaveAssets();

            env.assetDatabase.MoveAsset(finalDownloadedPath, AssetBundleConverterConfig.DOWNLOADED_PATH_ROOT);

            manifest = BuildPipeline.BuildAssetBundles(finalAssetBundlePath, BuildAssetBundleOptions.UncompressedAssetBundle | BuildAssetBundleOptions.ForceRebuildAssetBundle, BuildTarget.WebGL);

            if (manifest == null)
            {
                log.Error("Error generating asset bundle!");
                return false;
            }

            DependencyMapBuilder.Generate(env.file, finalAssetBundlePath, hashLowercaseToHashProper, manifest, MAIN_SHADER_AB_NAME);
            logBuffer += $"Generating asset bundles at path: {finalAssetBundlePath}\n";

            string[] assetBundles = manifest.GetAllAssetBundles();

            logBuffer += $"Total generated asset bundles: {assetBundles.Length}\n";

            for (int i = 0; i < assetBundles.Length; i++)
            {
                if (string.IsNullOrEmpty(assetBundles[i]))
                    continue;

                logBuffer += $"#{i} Generated asset bundle name: {assetBundles[i]}\n";
            }

            logBuffer += $"\nFree disk space after conv: {GetFreeSpace()}";

            return true;
        }

        public void Convert(ContentServerUtils.MappingPair[] rawContents, System.Action<ErrorCodes> OnFinish = null)
        {
            if (OnFinish == null)
                OnFinish = CleanAndExit;
            else
                OnFinish += CleanAndExit;

            startTime = Time.realtimeSinceStartup;

            log.Info($"Conversion start... free space in disk: {GetFreeSpace()}");

            InitializeDirectoryPaths(true);
            PopulateLowercaseMappings(rawContents);

            float timer = Time.realtimeSinceStartup;
            bool shouldGenerateAssetBundles = true;
            bool assetsAlreadyDumped = false;

            EditorApplication.CallbackFunction updateLoop = null;

            updateLoop = () =>
            {
                try
                {
                    //NOTE(Brian): We have to check this because the ImportAsset for GLTFs is not synchronous, and must execute some delayed calls
                    //             after the import asset finished. Therefore, we have to make sure those calls finished before continuing.
                    if (!GLTFImporter.finishedImporting && Time.realtimeSinceStartup - timer < 60)
                        return;

                    env.assetDatabase.Refresh();

                    if (!assetsAlreadyDumped)
                    {
                        shouldGenerateAssetBundles |= DumpAssets(rawContents);
                        assetsAlreadyDumped = true;
                        timer = Time.realtimeSinceStartup;

                        //NOTE(Brian): return in order to wait for GLTFImporter.finishedImporting flag, as it will set asynchronously.
                        return;
                    }

                    EditorApplication.update -= updateLoop;

                    if (shouldGenerateAssetBundles)
                    {
                        AssetBundleManifest manifest;

                        if (BuildAssetBundles(out manifest))
                        {
                            CleanAssetBundleFolder(manifest.GetAllAssetBundles());
                            OnFinish?.Invoke(ErrorCodes.SUCCESS);
                        }
                        else
                        {
                            OnFinish?.Invoke(ErrorCodes.ASSET_BUNDLE_BUILD_FAIL);
                        }
                    }
                    else
                    {
                        OnFinish?.Invoke(ErrorCodes.SUCCESS);
                    }
                }
                catch (Exception e)
                {
                    log.Error(e.Message);
                    OnFinish?.Invoke(ErrorCodes.UNDEFINED);
                    EditorApplication.update -= updateLoop;
                }
            };

            EditorApplication.update += updateLoop;
        }


        internal void CleanAssetBundleFolder(string[] assetBundles)
        {
            AssetBundleBuilderUtils.CleanAssetBundleFolder(env.file, finalAssetBundlePath, assetBundles, hashLowercaseToHashProper);
        }


        internal void PopulateLowercaseMappings(ContentServerUtils.MappingPair[] pairs)
        {
            foreach (var content in pairs)
            {
                string hashLower = content.hash.ToLowerInvariant();

                if (!hashLowercaseToHashProper.ContainsKey(hashLower))
                    hashLowercaseToHashProper.Add(hashLower, content.hash);
            }
        }

        internal void RetrieveAndInjectTexture(AssetPath gltfPath, AssetPath texturePath)
        {
            string finalPath = texturePath.finalPath;

            if (!env.file.Exists(finalPath))
                return;

            Texture2D t2d = env.assetDatabase.LoadAssetAtPath<Texture2D>(finalPath);

            if (t2d == null)
                return;

            string relativePath = AssetBundleBuilderUtils.GetRelativePathTo(gltfPath.file, texturePath.file);

            //NOTE(Brian): This cache will be used by the GLTF importer when seeking textures. This way the importer will
            //             consume the asset bundle dependencies instead of trying to create new textures.
            PersistentAssetCache.AddImage(relativePath, gltfPath.finalPath, new RefCountedTextureData(relativePath, t2d));
        }

        internal void RetrieveAndInjectBuffer(AssetPath gltfPath, AssetPath bufferPath)
        {
            string finalPath = bufferPath.finalPath;

            if (!env.file.Exists(finalPath))
                return;

            Stream stream = env.file.OpenRead(finalPath);
            string relativePath = AssetBundleBuilderUtils.GetRelativePathTo(gltfPath.file, bufferPath.file);

            // NOTE(Brian): This cache will be used by the GLTF importer when seeking streams. This way the importer will
            //              consume the asset bundle dependencies instead of trying to create new streams.
            PersistentAssetCache.AddBuffer(relativePath, gltfPath.finalPath, new RefCountedStreamData(relativePath, stream));
        }

        internal string DownloadAsset(AssetPath assetPath)
        {
            string outputPath = assetPath.finalPath;
            string outputPathDir = Path.GetDirectoryName(outputPath);

            string baseUrl = ContentServerUtils.GetContentAPIUrlBase(settings.tld);
            string finalUrl = baseUrl + assetPath.hash;

            log.Verbose("checking against " + outputPath);

            if (env.file.Exists(outputPath))
            {
                log.Verbose("Skipping already generated asset: " + outputPath);
                return outputPath;
            }

            byte[] assetData = env.webRequest.Get(finalUrl);

            log.Verbose($"Downloaded asset = {finalUrl} to {outputPathDir}");

            if (!env.directory.Exists(outputPathDir))
                env.directory.CreateDirectory(outputPathDir);

            env.file.WriteAllBytes(outputPath, assetData);
            env.assetDatabase.ImportAsset(outputPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ImportRecursive);

            return outputPath;
        }

        internal virtual void InitializeDirectoryPaths(bool deleteIfExists)
        {
            env.directory.InitializeDirectory(finalDownloadedPath, deleteIfExists);
            env.directory.InitializeDirectory(finalAssetBundlePath, deleteIfExists);
        }

        internal virtual void CleanupWorkingFolders()
        {
            env.file.Delete(finalAssetBundlePath + AssetBundleConverterConfig.ASSET_BUNDLE_FOLDER_NAME);
            env.file.Delete(finalAssetBundlePath + AssetBundleConverterConfig.ASSET_BUNDLE_FOLDER_NAME + ".manifest");

            if (settings.deleteDownloadPathAfterFinished)
            {
                env.directory.Delete(finalDownloadedPath);
                env.assetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            }
        }

        protected virtual float GetFreeSpace()
        {
            FileInfo file = new FileInfo(finalAssetBundlePath);

            if (file.Directory == null)
                return 0;

            DriveInfo info = new DriveInfo(file.Directory.Root.FullName);
            return info.AvailableFreeSpace;
        }
    }
}