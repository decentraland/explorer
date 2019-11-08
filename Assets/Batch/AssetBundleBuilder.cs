using DCL.Helpers;
using GLTF.Schema;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Networking;
using UnityGLTF;
using UnityGLTF.Cache;
using MappingPair = DCL.ContentServerUtils.MappingPair;
using MappingsAPIData = DCL.ContentServerUtils.MappingsAPIData;
using ScenesAPIData = DCL.ContentServerUtils.ScenesAPIData;

namespace DCL
{
    public static class AssetBundleBuilder
    {
        [System.Serializable]
        public class AssetDependencyMap
        {
            public string[] dependencies;
        }

        static bool VERBOSE = false;

        const string CLI_ALWAYS_BUILD_SYNTAX = "alwaysBuild";
        const string CLI_KEEP_BUNDLES_SYNTAX = "keepBundles";
        const string CLI_BUILD_SCENE_SYNTAX = "sceneCid";
        const string CLI_BUILD_PARCELS_RANGE_SYNTAX = "parcelsXYWH";

        internal static string DOWNLOADED_ASSET_DB_PATH_ROOT = "Assets/_Downloaded/";
        internal static string DOWNLOADED_PATH_ROOT = Application.dataPath + "/_Downloaded/";
        internal static string ASSET_BUNDLE_FOLDER_NAME = "AssetBundles";
        internal static string ASSET_BUNDLES_PATH_ROOT = Application.dataPath + "/../" + ASSET_BUNDLE_FOLDER_NAME + "/";

        internal static bool deleteDownloadPathAfterFinished = true;
        internal static bool skipUploadedGltfs = true;

        internal static string finalAssetBundlePath = "";
        internal static string finalDownloadedPath = "";
        internal static string finalDownloadedAssetDbPath = "";

        internal static ContentServerUtils.ApiEnvironment environment = ContentServerUtils.ApiEnvironment.ORG;

        internal static System.Action<int> OnBundleBuildFinish = null;
        static Dictionary<string, string> hashLowercaseToHashProper = new Dictionary<string, string>();

        static float startTime;

        internal static void DumpArea(List<Vector2Int> coords, Action<int> OnFinish = null)
        {
            HashSet<string> sceneCids = new HashSet<string>();

            foreach (Vector2Int v in coords)
            {
                string url = ContentServerUtils.GetScenesAPIUrl(environment, v.x, v.y, 0, 0);
                UnityWebRequest w = UnityWebRequest.Get(url);
                w.SendWebRequest();

                while (w.isDone == false) { }

                if (!w.WebRequestSucceded())
                    throw new Exception($"Request error! Parcels couldn't be fetched! -- {w.error}");

                ScenesAPIData scenesApiData = JsonUtility.FromJson<ScenesAPIData>(w.downloadHandler.text);

                Assert.IsTrue(scenesApiData != null, "Invalid response from ScenesAPI");
                Assert.IsTrue(scenesApiData.data != null, "Invalid response from ScenesAPI");

                foreach (var data in scenesApiData.data)
                {
                    if (!sceneCids.Contains(data.root_cid))
                    {
                        sceneCids.Add(data.root_cid);
                    }
                }
            }

            List<string> sceneCidsList = sceneCids.ToList();

            DumpSceneList(sceneCidsList, OnFinish);
        }
        internal static void DumpScene(string cid, Action<int> OnFinish = null)
        {
            DumpSceneList(new List<string> { cid }, OnFinish);
        }

        static void DumpSceneList(List<string> sceneCidsList, System.Action<int> OnFinish)
        {
            Debug.Log($"Building {sceneCidsList.Count} scenes...");
            startTime = Time.realtimeSinceStartup;

            finalAssetBundlePath = ASSET_BUNDLES_PATH_ROOT;
            finalDownloadedPath = DOWNLOADED_PATH_ROOT;
            finalDownloadedAssetDbPath = DOWNLOADED_ASSET_DB_PATH_ROOT;

            InitializeDirectory(finalDownloadedPath);
            OnBundleBuildFinish = (errorCode) => { Debug.Log($"Conversion finished. [Time:{Time.realtimeSinceStartup - startTime}]"); OnFinish?.Invoke(errorCode); };

            float timer = Time.realtimeSinceStartup;

            EditorApplication.update = () =>
            {
                //NOTE(Brian): We have to check this because the ImportAsset for GLTFs is not synchronous, and must execute some delayed calls
                //             after the import asset finished. Therefore, we have to make sure those calls finished before continuing.
                if (!GLTFImporter.finishedImporting || Time.realtimeSinceStartup - timer > 60)
                    return;

                if (sceneCidsList.Count > 0)
                {
                    ExportSceneToAssetBundles_Internal(sceneCidsList[0]);
                    sceneCidsList.RemoveAt(0);
                    AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate | ImportAssetOptions.ImportRecursive);
                    timer = Time.realtimeSinceStartup;
                    return;
                }

                if (!Directory.Exists(finalAssetBundlePath))
                    Directory.CreateDirectory(finalAssetBundlePath);

                BuildAssetBundles();

                EditorApplication.update = null;
            };
        }

        public static void ExportSceneToAssetBundles()
        {
            try
            {
                skipUploadedGltfs = true;
                deleteDownloadPathAfterFinished = true;

                if (AssetBundleBuilderUtils.ParseOption(CLI_ALWAYS_BUILD_SYNTAX, 0, out string[] noargs))
                    skipUploadedGltfs = false;

                if (AssetBundleBuilderUtils.ParseOption(CLI_KEEP_BUNDLES_SYNTAX, 0, out string[] noargs2))
                    deleteDownloadPathAfterFinished = false;

                if (AssetBundleBuilderUtils.ParseOption(CLI_BUILD_SCENE_SYNTAX, 1, out string[] sceneCid))
                {
                    if (sceneCid == null || string.IsNullOrEmpty(sceneCid[0]))
                    {
                        throw new ArgumentException("Invalid sceneCid argument! Please use -sceneCid <id> to establish the desired id to process.");
                    }

                    DumpScene(sceneCid[0], Exit);
                    return;
                }

                if (AssetBundleBuilderUtils.ParseOption(CLI_BUILD_PARCELS_RANGE_SYNTAX, 4, out string[] xywh))
                {
                    if (xywh == null)
                    {
                        throw new ArgumentException("Invalid parcelsXYWH argument! Please use -parcelsXYWH x y w h to establish the desired parcels range to process.");
                    }

                    int x, y, w, h;
                    bool parseSuccess = false;

                    parseSuccess |= int.TryParse(xywh[0], out x);
                    parseSuccess |= int.TryParse(xywh[1], out y);
                    parseSuccess |= int.TryParse(xywh[2], out w);
                    parseSuccess |= int.TryParse(xywh[3], out h);

                    if (!parseSuccess)
                    {
                        throw new ArgumentException("Invalid parcelsXYWH argument! Please use -parcelsXYWH x y w h to establish the desired parcels range to process.");
                    }

                    var zoneArray = AssetBundleBuilderUtils.GetBottomLeftZoneArray(new Vector2Int(x, y), new Vector2Int(w, h));

                    DumpArea(zoneArray, Exit);

                    return;
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                Exit(1);
            }
        }

        internal static void ExportSceneToAssetBundles_Internal(string sceneCid)
        {
            Caching.ClearCache();

            if (string.IsNullOrEmpty(sceneCid))
                throw new ArgumentException($"invalid sceneCid! -- cid: {sceneCid}");

            Debug.Log($"Exporting scene... {sceneCid}");

            string url = ContentServerUtils.GetMappingsAPIUrl(environment, sceneCid);
            UnityWebRequest w = UnityWebRequest.Get(url);
            w.SendWebRequest();

            while (w.isDone == false) { }

            if (!w.WebRequestSucceded())
                throw new Exception($"Request error! mappings couldn't be fetched for scene {sceneCid}! -- {w.error}");

            MappingsAPIData parcelInfoApiData = JsonUtility.FromJson<MappingsAPIData>(w.downloadHandler.text);

            if (parcelInfoApiData.data.Length == 0 || parcelInfoApiData.data == null)
            {
                throw new Exception("MappingsAPIData is null?");
            }

            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate | ImportAssetOptions.ImportRecursive);

            MappingPair[] rawContents = parcelInfoApiData.data[0].content.contents;

            var contentProvider = new DCL.ContentProvider();
            contentProvider.contents = new List<MappingPair>(rawContents);
            contentProvider.baseUrl = ContentServerUtils.GetContentAPIUrlBase(environment);
            contentProvider.BakeHashes();

            var contentProviderAB = new DCL.ContentProvider();
            contentProviderAB.contents = new List<MappingPair>(rawContents);
            contentProviderAB.baseUrl = ContentServerUtils.GetBundlesAPIUrlBase(environment);
            contentProviderAB.BakeHashes();

            string[] bufferExtensions = { ".bin" };
            string[] textureExtensions = { ".jpg", ".png", ".jpeg", ".tga", ".gif", ".bmp", ".psd", ".tiff", ".iff" };
            string[] gltfExtensions = { ".glb", ".gltf" };

            var stringToAB = new Dictionary<string, AssetBundle>();

            var hashToTexturePair = FilterExtensions(rawContents, textureExtensions);
            var hashToGltfPair = FilterExtensions(rawContents, gltfExtensions);
            var hashToBufferPair = FilterExtensions(rawContents, bufferExtensions);

            Dictionary<string, string> pathsToTag = new Dictionary<string, string>();

            //NOTE(Brian): Prepare buffers. We should prepare all the dependencies in this phase.
            foreach (var kvp in hashToBufferPair)
            {
                string hash = kvp.Key;
                DownloadAsset(contentProvider, hashToBufferPair, hash, hash + "/");
            }

            //NOTE(Brian): Prepare textures. We should prepare all the dependencies in this phase.
            foreach (var kvp in hashToTexturePair)
            {
                string hash = kvp.Key;

                //NOTE(Brian): try to get an AB before getting the original texture, so we bind the dependencies correctly
                string fullPathToTag = DownloadAsset(contentProvider, hashToTexturePair, hash, hash + "/");

                if (fullPathToTag != null)
                    pathsToTag.Add(fullPathToTag, hash);
            }

            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate | ImportAssetOptions.ImportRecursive);
            AssetDatabase.SaveAssets();

            foreach (var kvp in pathsToTag)
            {
                MarkForAssetBundleBuild(kvp.Key, kvp.Value);
            }

            GLTFImporter.OnGLTFRootIsConstructed -= FixGltfDependencyPaths;
            GLTFImporter.OnGLTFRootIsConstructed += FixGltfDependencyPaths;

            pathsToTag.Clear();
            List<Stream> streamsToDispose = new List<Stream>();

            //NOTE(Brian): Prepare gltfs gathering its dependencies first and filling the importer's static cache.
            foreach (var kvp in hashToGltfPair)
            {
                string gltfHash = kvp.Key;

                if (skipUploadedGltfs)
                {
                    if (CheckContentUrlExists(contentProviderAB, hashToGltfPair, gltfHash))
                    {
                        Debug.Log("Skipping existing gltf: " + gltfHash);
                        continue;
                    }
                }

                PersistentAssetCache.ImageCacheByUri.Clear();
                PersistentAssetCache.StreamCacheByUri.Clear();

                foreach (var mappingPair in rawContents)
                {
                    bool endsWithTextureExtensions = textureExtensions.Any((x) => mappingPair.file.ToLower().EndsWith(x));

                    if (endsWithTextureExtensions)
                    {
                        string relativePath = GetRelativePathTo(hashToGltfPair[gltfHash].file, mappingPair.file);
                        string fileExt = Path.GetExtension(mappingPair.file);
                        string outputPath = finalDownloadedAssetDbPath + mappingPair.hash + "/" + mappingPair.hash + fileExt;
                        string realOutputPath = finalDownloadedPath + mappingPair.hash + "/" + mappingPair.hash + fileExt;

                        if (File.Exists(realOutputPath))
                        {
                            Texture2D t2d = AssetDatabase.LoadAssetAtPath<Texture2D>(outputPath);

                            if (t2d != null)
                            {
                                //NOTE(Brian): This cache will be used by the GLTF importer when seeking textures. This way the importer will
                                //             consume the asset bundle dependencies instead of trying to create new textures.
                                PersistentAssetCache.ImageCacheByUri[relativePath] = new RefCountedTextureData(relativePath, t2d);
                            }
                        }
                    }

                    bool endsWithBufferExtensions = bufferExtensions.Any((x) => mappingPair.file.ToLower().EndsWith(x));

                    if (endsWithBufferExtensions)
                    {
                        string relativePath = GetRelativePathTo(hashToGltfPair[gltfHash].file, mappingPair.file);
                        string fileExt = Path.GetExtension(mappingPair.file);
                        string outputPath = finalDownloadedAssetDbPath + mappingPair.hash + "/" + mappingPair.hash + fileExt;
                        string realOutputPath = finalDownloadedPath + mappingPair.hash + "/" + mappingPair.hash + fileExt;

                        if (File.Exists(realOutputPath))
                        {
                            Stream stream = File.OpenRead(realOutputPath);

                            //NOTE(Brian): This cache will be used by the GLTF importer when seeking streams. This way the importer will
                            //             consume the asset bundle dependencies instead of trying to create new streams.
                            PersistentAssetCache.StreamCacheByUri[relativePath] = new RefCountedStreamData(relativePath, stream);
                        }
                    }
                }

                //NOTE(Brian): Finally, load the gLTF. The GLTFImporter will use the PersistentAssetCache to resolve the external dependencies.
                string path = DownloadAsset(contentProvider, hashToGltfPair, gltfHash, gltfHash + "/");

                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate | ImportAssetOptions.ImportRecursive);
                AssetDatabase.SaveAssets();

                if (path != null)
                {
                    pathsToTag.Add(path, gltfHash);
                }

                foreach (var streamDataKvp in PersistentAssetCache.StreamCacheByUri)
                {
                    if (streamDataKvp.Value.stream != null)
                        streamsToDispose.Add(streamDataKvp.Value.stream);
                }
            }

            foreach (var kvp in pathsToTag)
            {
                MarkForAssetBundleBuild(kvp.Key, kvp.Value);
            }

            foreach (var s in streamsToDispose)
            {
                s.Dispose();
            }
        }

        static void Exit(int errorCode = 0)
        {
            Debug.Log($"Process finished with code {errorCode}");

            if (Application.isBatchMode)
                EditorApplication.Exit(errorCode);
        }

        static void MarkForAssetBundleBuild(string path, string abName)
        {
            string assetPath = path.Substring(path.IndexOf("Assets"));
            assetPath = Path.ChangeExtension(assetPath, null);

            assetPath = assetPath.Substring(0, assetPath.Length - 1);
            AssetImporter a = AssetImporter.GetAtPath(assetPath);
            a.SetAssetBundleNameAndVariant(abName, "");
        }

        internal static void BuildAssetBundles()
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate | ImportAssetOptions.ImportRecursive);
            AssetDatabase.SaveAssets();

            var manifest = BuildPipeline.BuildAssetBundles(finalAssetBundlePath, BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.ForceRebuildAssetBundle, BuildTarget.WebGL);

            if (manifest == null)
            {
                OnBundleBuildFinish?.Invoke(2);
                throw new Exception("Error generating asset bundle!");
            }

            string[] assetBundles = manifest.GetAllAssetBundles();
            string[] assetBundlePaths = new string[assetBundles.Length];

            Debug.Log($"Total generated asset bundles: {assetBundles.Length}");

            GenerateDependencyMaps(manifest);

            for (int i = 0; i < assetBundles.Length; i++)
            {
                if (string.IsNullOrEmpty(assetBundles[i]))
                    continue;

                Debug.Log($"#{i} Generated asset bundle name: {assetBundles[i]}");

                //NOTE(Brian): This is done for correctness sake, rename files to preserve the hash upper-case
                hashLowercaseToHashProper.TryGetValue(assetBundles[i], out string hashWithUppercase);

                string oldPath = finalAssetBundlePath + assetBundles[i];
                string path = finalAssetBundlePath + hashWithUppercase;

                string oldPathMf = finalAssetBundlePath + assetBundles[i] + ".manifest";

                File.Move(oldPath, path);
                File.Delete(oldPathMf);

                assetBundlePaths[i] = path;
            }

            try
            {
                File.Delete(finalAssetBundlePath + ASSET_BUNDLE_FOLDER_NAME);
                File.Delete(finalAssetBundlePath + ASSET_BUNDLE_FOLDER_NAME + ".manifest");
            }
            catch (Exception e)
            {
                Debug.LogError("Error trying to delete AssetBundleManifest root files!\n" + e.Message);
            }

            if (deleteDownloadPathAfterFinished)
            {
                try
                {
                    if (Directory.Exists(finalDownloadedPath))
                        Directory.Delete(finalDownloadedPath, true);

                    AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                }
                catch (Exception e)
                {
                    Debug.LogError("Error trying to delete Assets downloaded path!\n" + e.Message);
                }
            }

            OnBundleBuildFinish?.Invoke(0);
        }

        /// <summary>
        /// This dumps .depmap files
        /// </summary>
        /// <param name="manifest"></param>
        private static void GenerateDependencyMaps(AssetBundleManifest manifest)
        {
            string[] assetBundles = manifest.GetAllAssetBundles();

            for (int i = 0; i < assetBundles.Length; i++)
            {
                if (string.IsNullOrEmpty(assetBundles[i]))
                    continue;

                var depMap = new AssetDependencyMap();
                string[] deps = manifest.GetAllDependencies(assetBundles[i]);

                if (deps.Length == 0)
                    continue;

                depMap.dependencies = deps.Select((x) =>
                {
                    if (hashLowercaseToHashProper.ContainsKey(x))
                        return hashLowercaseToHashProper[x];
                    else
                        return x;

                }).ToArray();

                string json = JsonUtility.ToJson(depMap);
                string finalFilename = assetBundles[i];

                hashLowercaseToHashProper.TryGetValue(assetBundles[i], out finalFilename);
                File.WriteAllText(finalAssetBundlePath + finalFilename + ".depmap", json);
            }
        }

        private static bool CheckContentUrlExists(DCL.ContentProvider contentProvider, Dictionary<string, MappingPair> filteredPairs, string hash)
        {
            string finalUrl = contentProvider.GetContentsUrl(filteredPairs[hash].file);

            UnityWebRequest req = UnityWebRequest.Head(finalUrl);
            req.SendWebRequest();

            while (req.isDone == false) { }

            if (req.WebRequestSucceded())
                return true;

            return false;
        }

        private static string DownloadAsset(DCL.ContentProvider contentProvider, Dictionary<string, MappingPair> filteredPairs, string hash, string additionalPath = "")
        {
            string fileExt = Path.GetExtension(filteredPairs[hash].file);
            string outputPath = finalDownloadedPath + additionalPath + hash + fileExt;
            string outputPathDir = Path.GetDirectoryName(outputPath);
            string finalUrl = contentProvider.GetContentsUrl(filteredPairs[hash].file);
            string hashLowercase = hash.ToLowerInvariant();

            if (VERBOSE)
                Debug.Log("checking against " + outputPath);

            if (File.Exists(outputPath))
            {
                if (VERBOSE)
                    Debug.Log("Skipping already generated asset: " + outputPath);

                if (!hashLowercaseToHashProper.ContainsKey(hashLowercase))
                    hashLowercaseToHashProper.Add(hashLowercase, hash);

                return finalDownloadedPath + additionalPath;
            }

            UnityWebRequest req = UnityWebRequest.Get(finalUrl);
            req.SendWebRequest();
            while (req.isDone == false) { }

            if (VERBOSE)
                Debug.Log("Downloaded asset = " + finalUrl);

            if (!req.WebRequestSucceded())
                return null;

            if (!Directory.Exists(outputPathDir))
                Directory.CreateDirectory(outputPathDir);

            File.WriteAllBytes(outputPath, req.downloadHandler.data);

            if (!hashLowercaseToHashProper.ContainsKey(hashLowercase))
                hashLowercaseToHashProper.Add(hashLowercase, hash);

            return finalDownloadedPath + additionalPath;
        }

        private static void InitializeDirectory(string path)
        {
            try
            {
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }

                Directory.CreateDirectory(path);
            }
            catch (Exception e)
            {
                Debug.LogError($"Exception trying to clean up folder. Continuing anyways.\n{e.Message}");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pairsToSearch"></param>
        /// <param name="extensions"></param>
        /// <returns>A dictionary that maps hashes to mapping pairs</returns>
        public static Dictionary<string, MappingPair> FilterExtensions(MappingPair[] pairsToSearch, string[] extensions)
        {
            var result = new Dictionary<string, MappingPair>();

            for (int i = 0; i < pairsToSearch.Length; i++)
            {
                MappingPair mappingPair = pairsToSearch[i];

                bool hasExtension = extensions.Any((x) => mappingPair.file.ToLower().EndsWith(x));

                if (hasExtension)
                {
                    if (!result.ContainsKey(mappingPair.hash))
                    {
                        result.Add(mappingPair.hash, mappingPair);
                    }
                }
            }

            return result;
        }

        public static void FixGltfDependencyPaths(GLTFRoot gltfRoot)
        {
            GLTFRoot root = gltfRoot;

            if (root != null)
            {
                if (root.Images != null)
                {
                    foreach (GLTFImage image in root.Images)
                    {
                        if (!string.IsNullOrEmpty(image.Uri))
                        {
                            bool isBase64 = URIHelper.IsBase64Uri(image.Uri);

                            if (!isBase64)
                            {
                                image.Uri = image.Uri.Replace('/', Path.DirectorySeparatorChar);
                            }
                        }
                    }
                }

                if (root.Buffers != null)
                {
                    foreach (GLTFBuffer buffer in root.Buffers)
                    {
                        if (!string.IsNullOrEmpty(buffer.Uri))
                        {
                            bool isBase64 = URIHelper.IsBase64Uri(buffer.Uri);

                            if (!isBase64)
                            {
                                buffer.Uri = buffer.Uri.Replace('/', Path.DirectorySeparatorChar);
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Gets the relative path ("..\..\to_file_or_dir") of another file or directory (to) in relation to the current file/dir (from)
        /// </summary>
        /// <param name="to"></param>
        /// <param name="from"></param>
        /// <returns></returns>
        public static string GetRelativePathTo(string from, string to)
        {
            var fromPath = Path.GetFullPath(from);
            var toPath = Path.GetFullPath(to);

            var fromUri = new Uri(fromPath);
            var toUri = new Uri(toPath);

            var relativeUri = fromUri.MakeRelativeUri(toUri);
            var relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            return relativePath.Replace('/', Path.DirectorySeparatorChar);
        }

    }
}
