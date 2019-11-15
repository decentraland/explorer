using DCL.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Networking;
using UnityGLTF;
using UnityGLTF.Cache;

namespace DCL
{
    public static class BundleBuilder
    {
        internal static bool VERBOSE = true;
        internal static bool VERBOSE_DOWNLOAD = false;

        /**
         * Don't use a folder inside the project because Unity will try to get
         * that asset into the asset pack.
         */
        internal static string TEMP_DOWNLOAD_FOLDER = Path.Combine(Application.dataPath, "../../ConverterContentDownloads");

        /**
         * Output folder for asset bundles
         */
        internal static string ASSET_BUNDLE_OUTPUT_FOLDER = Path.Combine(Application.dataPath, "../../AssetBundleOutput");

        /**
         * Folder, inside the working directory, for importing assets into
         */
        internal static string ASSET_BUNDLE_WORKING_DIR = Path.Combine(Application.dataPath, "AssetBundleWorkingDir");

        /**
         * Relative path to the  working folder
         */
        internal static string ASSET_BUNDLE_RELATIVE_WORKING_DIR = Path.Combine("Assets", "AssetBundleWorkingDir");

        internal static string[] BUFFER_EXTENSIONS = { ".bin" };
        internal static string[] TEXTURE_EXTENSIONS = { ".jpg", ".png", ".jpeg", ".tga", ".gif", ".bmp", ".psd", ".tiff", ".iff" };
        internal static string[] GLTF_EXTENSIONS = { ".glb", ".gltf" };

        /**
         * Generates any tagged-for-export asset bundle
         */
        public static string[] GenerateAllAssetBundles()
        {
            var manifest = BuildPipeline.BuildAssetBundles(
                ASSET_BUNDLE_OUTPUT_FOLDER,
                BuildAssetBundleOptions.ChunkBasedCompression
                | BuildAssetBundleOptions.DeterministicAssetBundle
                | BuildAssetBundleOptions.StrictMode,
                BuildTarget.WebGL
            );

            CleanupExtraGeneratedBundles();
            return manifest.GetAllAssetBundles();
        }

        /**
         * Generate a GLTF Asset Bundle
         */
        public static string GenerateGLTFAssetBundle(ContentProvider contentProvider, string file)
        {
            DownloadAll(contentProvider);

            var modelFile = PrepareGLTFAssetBundle(contentProvider, file);
            var hash = contentProvider.GetLowercaseHashForFile(file);
            Thread.Yield();
            var retry = 50;

            while (!GLTFImporter.finishedImporting || retry > 0)
            {
                Thread.Yield();
                retry--;
            }

            var assetImporter = AssetImporter.GetAtPath(modelFile);
            assetImporter.SetAssetBundleNameAndVariant(hash, "");

            return modelFile;
        }

        public static string PrepareGLTFAssetBundle(ContentProvider contentProvider, string file)
        {
            GLTFImporter.OnGLTFRootIsConstructed -= AssetBundleBuilderUtils.FixGltfDependencyPaths;
            GLTFImporter.OnGLTFRootIsConstructed += AssetBundleBuilderUtils.FixGltfDependencyPaths;

            PersistentAssetCache.ImageCacheByUri.Clear();
            PersistentAssetCache.StreamCacheByUri.Clear();

            var textures = FilterOnlyAssetsWithExtension(contentProvider, TEXTURE_EXTENSIONS);
            foreach (var texture in textures)
            {
                RetrieveAndInjectTexture(contentProvider.GetLowercaseHashForFile(texture), texture, file);
            }

            var buffers = FilterOnlyAssetsWithExtension(contentProvider, BUFFER_EXTENSIONS);
            foreach (var buffer in buffers)
            {
                RetrieveAndInjectBuffer(contentProvider.GetLowercaseHashForFile(buffer), buffer, file);
            }

            var modelFile = CopyContentIntoWorkingFolder(contentProvider, file);

            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();

            return modelFile;
        }

        /**
         * Retrieve a texture from the built folder, inject it into the Persistent Asset Cache
         */
        public static void RetrieveAndInjectTexture(string hash, string filename, string gltfFilename)
        {
            var bundlePath = CopyBuiltAssetBundle(hash);
            var workingPath = GetWorkingPathForBundle(hash);
            if (!File.Exists(bundlePath))
            {
                throw new Exception($"Could not find bundle for texture {filename} and hash {hash}");
            }

            AssetDatabase.ImportAsset(workingPath);
            Thread.Yield();
            var retry = 5;

            Texture2D t2d = AssetDatabase.LoadAssetAtPath<Texture2D>(workingPath);
            while (t2d == null && retry > 0)
            {
                AssetDatabase.Refresh();
                Thread.Yield();
                t2d = AssetDatabase.LoadAssetAtPath<Texture2D>(workingPath);
                Thread.Yield();
                retry--;
            }

            Debug.Log($"injecting texture dependency {bundlePath}. guid: {AssetDatabase.AssetPathToGUID(workingPath)}");

            if (t2d != null)
            {
                string relativePath = AssetBundleBuilderUtils.GetRelativePathTo(gltfFilename, filename);
                Debug.Log($"Mapping: {relativePath} -> {bundlePath}");
                //NOTE(Brian): This cache will be used by the GLTF importer when seeking textures. This way the importer will
                //             consume the asset bundle dependencies instead of trying to create new textures.
                PersistentAssetCache.ImageCacheByUri[relativePath] = new RefCountedTextureData(relativePath, t2d);
            }
            else
            {
                throw new Exception($"Impossible to resolve: {gltfFilename} -> {bundlePath}");
            }
        }

        /**
         * Retrieve a buffer from the downloads folder, inject it into the Persistent Asset Cache
         */
        public static void RetrieveAndInjectBuffer(string hash, string filename, string gltfFilename)
        {
            var filePath = CopyContentIntoWorkingFolder(hash);
            Stream stream = File.OpenRead(filePath);
            string relativePath = AssetBundleBuilderUtils.GetRelativePathTo(gltfFilename, filename);

            // NOTE(Brian): This cache will be used by the GLTF importer when seeking streams. This way the importer will
            //              consume the asset bundle dependencies instead of trying to create new streams.
            PersistentAssetCache.StreamCacheByUri[relativePath] = new RefCountedStreamData(relativePath, stream);
        }

        /**
         * Generate asset bundles out of all potential textures in a content provider
         */
        public static string[] GenerateTextureAssetBundles(ContentProvider contentProvider)
        {
            var filesWithTextureExtension = FilterOnlyAssetsWithExtension(contentProvider, TEXTURE_EXTENSIONS);

            foreach (var file in filesWithTextureExtension)
            {
                PrepareTextureAssetBundle(contentProvider, file);
            }
            return GenerateAllAssetBundles();
        }

        /**
         * Only build a single texture
         */
        public static string GenerateTextureAssetBundle(ContentProvider contentProvider, string file)
        {
            PrepareTextureAssetBundle(contentProvider, file);
            var assetBundles = GenerateAllAssetBundles();
            if (assetBundles.Length > 1)
            {
                var arrayAsString = assetBundles.Aggregate("", (current, next) => (current.Length > 0) ? current + ", " + next : next);
                throw new Exception($"Expected a length of 1, found {assetBundles.Length}: {arrayAsString}");
            }
            return assetBundles[0];
        }

        public static void PrepareTextureAssetBundle(ContentProvider contentProvider, string file)
        {
            var metaFilePath = ImportTextureAsset(contentProvider, file);
            var assetImporter = AssetImporter.GetAtPath(metaFilePath.Substring(0, metaFilePath.LastIndexOf('.')));
            assetImporter.SetAssetBundleNameAndVariant(contentProvider.GetLowercaseHashForFile(file), "");
        }

        public static string ImportTextureAsset(ContentProvider contentProvider, string file)
        {
            DownloadIntoWorkingFolder(contentProvider, file);
            var pathToImport = GetRelativeWorkingPathForFile(contentProvider, file);

            AssetDatabase.ImportAsset(pathToImport, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);

            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();

            AssetDatabase.ReleaseCachedFileHandles();

            var metaFilePath = pathToImport + ".meta";
            string metaContent = File.ReadAllText(metaFilePath);
            string guid = AssetBundleBuilderUtils.GetGUID(contentProvider.GetHashForFile(file));
            string result = Regex.Replace(metaContent, @"guid: \w+?\n", $"guid: {guid}\n");
            if (VERBOSE)
            {
                Debug.Log($"Replacing GUID: {file} => {guid}");
            }
            File.WriteAllText(metaFilePath, result);

            AssetDatabase.Refresh();

            return metaFilePath;
        }

        public static void CleanupExtraGeneratedBundles()
        {
            var bundleFile = Path.Combine(ASSET_BUNDLE_OUTPUT_FOLDER, Path.GetFileName(ASSET_BUNDLE_OUTPUT_FOLDER));
            var manifestFile = Path.ChangeExtension(bundleFile, "manifest");

            if (File.Exists(bundleFile))
                File.Delete(bundleFile);
            if (File.Exists(manifestFile))
                File.Delete(manifestFile);
        }

        public static string DownloadIntoWorkingFolder(ContentProvider contentProvider, string file)
        {
            DownloadRawContent(contentProvider, file);
            return CopyContentIntoWorkingFolder(contentProvider, file);
        }

        public static List<string> DownloadAll(ContentProvider contentProvider)
        {
            var result = new List<string>();
            foreach (var mapping in contentProvider.contents)
            {
                result.Add(DownloadRawContent(contentProvider, mapping.file));
            }
            return result;
        }

        /**
         * Downloads a file into TEMP_DOWNLOAD_FOLDER
         */
        public static string DownloadRawContent(ContentProvider contentProvider, string file)
        {
            string hash = contentProvider.GetHashForFile(file);
            string lowercaseHash = hash.ToLowerInvariant();
            string fileExt = Path.GetExtension(file);
            string outputPath = Path.Combine(TEMP_DOWNLOAD_FOLDER, lowercaseHash + fileExt);
            string outputPathDir = Path.GetDirectoryName(outputPath);

            if (File.Exists(outputPath))
            {
                if (VERBOSE_DOWNLOAD)
                    Debug.Log("Skipping already downloaded asset: " + outputPath);

                return outputPath;
            }
            if (VERBOSE_DOWNLOAD)
            {
                Debug.Log("Downloading " + file + "(" + hash + ") to " + outputPath);
            }

            UnityWebRequest req;
            string finalUrl = contentProvider.GetContentsUrl(file);
            int retryCount = 5;
            do
            {
                req = UnityWebRequest.Get(finalUrl);
                req.SendWebRequest();
                while (req.isDone == false) { }

                retryCount--;

                if (retryCount == 0)
                    return null;
            }
            while (!req.WebRequestSucceded());

            if (VERBOSE_DOWNLOAD)
                Debug.Log("Downloaded asset = " + finalUrl + " to " + outputPath);

            if (!Directory.Exists(outputPathDir))
                Directory.CreateDirectory(outputPathDir);

            if (!File.Exists(outputPath))
                File.WriteAllBytes(outputPath, req.downloadHandler.data);

            return outputPath;
        }

        public static string GetRelativeWorkingPathForFile(ContentProvider contentProvider, string file)
        {
            return Path.Combine(
                ASSET_BUNDLE_RELATIVE_WORKING_DIR,
                contentProvider.GetLowercaseHashWithExtension(file)
            );
        }

        public static string GetWorkingPathForBundle(string hash)
        {
            return Path.Combine(ASSET_BUNDLE_RELATIVE_WORKING_DIR, hash);
        }


        public static string CopyContentIntoWorkingFolder(ContentProvider contentProvider, string file)
        {
            var hashedFilename = contentProvider.GetLowercaseHashWithExtension(file);
            var downloadedPath = Path.Combine(TEMP_DOWNLOAD_FOLDER, hashedFilename);
            var target = Path.Combine(ASSET_BUNDLE_WORKING_DIR, hashedFilename);
            if (!File.Exists(target))
                File.Copy(downloadedPath, target);
            return target;
        }
        public static string CopyContentIntoWorkingFolder(string hashedFilename)
        {
            var downloadedPath = Path.Combine(TEMP_DOWNLOAD_FOLDER, hashedFilename);
            var target = Path.Combine(ASSET_BUNDLE_WORKING_DIR, hashedFilename);
            if (!File.Exists(target))
                File.Copy(downloadedPath, target);
            return target;
        }

        public static string CopyBuiltAssetBundle(string hash)
        {
            var bundlePath = Path.Combine(ASSET_BUNDLE_OUTPUT_FOLDER, hash);
            var target = Path.Combine(ASSET_BUNDLE_WORKING_DIR, hash);
            if (!File.Exists(target))
                File.Copy(bundlePath, target);
            return target;
        }

        public static List<string> FilterOnlyAssetsWithExtension(ContentProvider provider, string[] extensions)
        {
            var result = new List<string>();

            foreach (var content in provider.contents)
            {
                bool hasExtension = extensions.Any((x) => content.file.ToLower().EndsWith(x, StringComparison.Ordinal));

                if (hasExtension)
                {
                    result.Add(content.file);
                }
            }
            return result;
        }

        public static void CleanupWorkingDir()
        {
            if (Directory.Exists(ASSET_BUNDLE_WORKING_DIR))
            {
                RecursiveDelete(ASSET_BUNDLE_WORKING_DIR);
                DeleteFile(ASSET_BUNDLE_WORKING_DIR + ".meta");
            }
        }

        public static void CleanupFilesystem()
        {
            if (Directory.Exists(TEMP_DOWNLOAD_FOLDER))
            {
                RecursiveDelete(TEMP_DOWNLOAD_FOLDER);
            }
            if (Directory.Exists(ASSET_BUNDLE_OUTPUT_FOLDER))
            {
                RecursiveDelete(ASSET_BUNDLE_OUTPUT_FOLDER);
            }
            CleanupWorkingDir();
        }

        /**
         * Set up the system
         */
        public static void InitializeFilesystemFolders()
        {
            EnsureFolder(TEMP_DOWNLOAD_FOLDER);
            EnsureFolder(ASSET_BUNDLE_OUTPUT_FOLDER);
            EnsureFolder(ASSET_BUNDLE_WORKING_DIR);
        }

        public static void EnsureFolder(string folder)
        {
            try
            {
                if (Directory.Exists(folder))
                {
                    return;
                }
                Directory.CreateDirectory(folder);
            }
            catch (Exception e)
            {
                Debug.LogError($"Exception trying to ensure folder exists: {e.Message}");
            }
        }

        public static void RecursiveDelete(string fileOrFolder)
        {
            try
            {
                Directory.Delete(fileOrFolder, true);
            }
            catch (Exception e)
            {
                Debug.LogError($"Exception trying to ensure folder exists: {e.Message}");
            }
        }

        public static void DeleteFile(string file)
        {
            File.Delete(file);
        }
    }
}
