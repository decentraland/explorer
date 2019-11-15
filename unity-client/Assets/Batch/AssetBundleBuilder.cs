using DCL.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityGLTF;
using UnityGLTF.Cache;
using MappingPair = DCL.ContentServerUtils.MappingPair;
using MappingsAPIData = DCL.ContentServerUtils.MappingsAPIData;

namespace DCL
{
    [System.Serializable]
    public class AssetDependencyMap
    {
        public string[] dependencies;
    }

    public static class AssetBundleBuilder
    {
        static bool VERBOSE = true;

        const string CLI_ALWAYS_BUILD_SYNTAX = "alwaysBuild";
        const string CLI_KEEP_BUNDLES_SYNTAX = "keepBundles";
        const string CLI_BUILD_SCENE_SYNTAX = "sceneCid";
        const string CLI_BUILD_PARCELS_RANGE_SYNTAX = "parcelsXYWH";

        internal static string ASSET_BUNDLE_FOLDER_NAME = "AssetBundles";
        internal static string DOWNLOADED_FOLDER_NAME = "_Downloaded";

        internal static string DOWNLOADED_ASSET_DB_PATH_ROOT = "Assets/" + DOWNLOADED_FOLDER_NAME;
        internal static string DOWNLOADED_PATH_ROOT = Application.dataPath + "/" + DOWNLOADED_FOLDER_NAME;
        internal static string ASSET_BUNDLES_PATH_ROOT = Application.dataPath + "/../" + ASSET_BUNDLE_FOLDER_NAME;

        internal static bool deleteDownloadPathAfterFinished = true;
        internal static bool skipAlreadyBuiltBundles = true;

        internal static string finalAssetBundlePath = "";
        internal static string finalDownloadedPath = "";
        internal static string finalDownloadedAssetDbPath = "";

        static string[] bufferExtensions = { ".bin" };
        static string[] gltfExtensions = { ".glb", ".gltf" };
        static string[] textureExtensions = { ".jpg", ".png", ".jpeg", ".tga", ".gif", ".bmp", ".psd", ".tiff", ".iff" };

        internal static ContentServerUtils.ApiEnvironment environment = ContentServerUtils.ApiEnvironment.ORG;

        internal static System.Action<int> OnBundleBuildFinish = null;
        static Dictionary<string, string> hashLowercaseToHashProper = new Dictionary<string, string>();

        static float startTime;

        public static void PrepareScene()
        {
            try
            {
                if (AssetBundleBuilderUtils.ParseOption(CLI_BUILD_SCENE_SYNTAX, 1, out string[] sceneCid))
                {
                    if (sceneCid == null || string.IsNullOrEmpty(sceneCid[0]))
                    {
                        throw new ArgumentException("Invalid sceneCid argument! Please use -sceneCid <id> to establish the desired id to process.");
                    }

                    DumpTexturesFromScene(sceneCid[0], AssetBundleBuilderUtils.Exit);
                    return;
                }
                else
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

                    if (w > 10 || h > 10 || w < 0 || h < 0)
                    {
                        throw new ArgumentException("Invalid parcelsXYWH argument! Please don't use negative width/height values, and ensure any given width/height doesn't exceed 10.");
                    }

                    DumpAreaTextures(new Vector2Int(x, y), new Vector2Int(w, h), AssetBundleBuilderUtils.Exit);
                    return;
                }
                else
                {
                    throw new ArgumentException("Invalid arguments! You must pass -parcelsXYWH or -sceneCid for dump to work!");
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                AssetBundleBuilderUtils.Exit(1);
            }
        }

        public static void ExportSceneToAssetBundles()
        {
            try
            {
                skipAlreadyBuiltBundles = true;
                deleteDownloadPathAfterFinished = true;

                if (AssetBundleBuilderUtils.ParseOption(CLI_ALWAYS_BUILD_SYNTAX, 0, out string[] noargs))
                    skipAlreadyBuiltBundles = false;

                if (AssetBundleBuilderUtils.ParseOption(CLI_KEEP_BUNDLES_SYNTAX, 0, out string[] noargs2))
                    deleteDownloadPathAfterFinished = false;

                if (AssetBundleBuilderUtils.ParseOption(CLI_BUILD_SCENE_SYNTAX, 1, out string[] sceneCid))
                {
                    if (sceneCid == null || string.IsNullOrEmpty(sceneCid[0]))
                    {
                        throw new ArgumentException("Invalid sceneCid argument! Please use -sceneCid <id> to establish the desired id to process.");
                    }

                    DumpScene(sceneCid[0], AssetBundleBuilderUtils.Exit);
                    return;
                }
                else
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

                    if (w > 10 || h > 10 || w < 0 || h < 0)
                    {
                        throw new ArgumentException("Invalid parcelsXYWH argument! Please don't use negative width/height values, and ensure any given width/height doesn't exceed 10.");
                    }

                    DumpArea(new Vector2Int(x, y), new Vector2Int(w, h), AssetBundleBuilderUtils.Exit);
                    return;
                }
                else
                {
                    throw new ArgumentException("Invalid arguments! You must pass -parcelsXYWH or -sceneCid for dump to work!");
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                AssetBundleBuilderUtils.Exit(1);
            }
        }

        internal static void DownloadSceneTextures(string sceneCid)
        {
            MappingsAPIData parcelInfoApiData = AssetBundleBuilderUtils.GetSceneMappingsData(sceneCid);

            AssetDatabase.Refresh();

            MappingPair[] rawContents = parcelInfoApiData.data[0].content.contents;

            var contentProvider = new DCL.ContentProvider();
            contentProvider.contents = new List<MappingPair>(rawContents);
            contentProvider.baseUrl = ContentServerUtils.GetContentAPIUrlBase(environment);
            contentProvider.BakeHashes();

            var hashToTexturePair = AssetBundleBuilderUtils.FilterExtensions(rawContents, textureExtensions);

            Dictionary<string, string> pathsToTag = new Dictionary<string, string>();

            //NOTE(Brian): Prepare textures. We should prepare all the dependencies in this phase.
            foreach (var kvp in hashToTexturePair)
            {
                string hash = kvp.Key;

                Debug.Log("dumping hash " + hash);
                //NOTE(Brian): try to get an AB before getting the original texture, so we bind the dependencies correctly
                string fullPathToTag = DownloadAsset(contentProvider, hashToTexturePair, hash, hash + "/");

                string fileExt = Path.GetExtension(hashToTexturePair[hash].file);
                string assetPath = hash + "/" + hash + fileExt;

                AssetDatabase.ImportAsset(finalDownloadedAssetDbPath + assetPath, ImportAssetOptions.ForceUpdate);
                AssetDatabase.SaveAssets();

                string metaPath = finalDownloadedPath + assetPath + ".meta";

                AssetDatabase.ReleaseCachedFileHandles();

                //NOTE(Brian): in asset bundles, all dependencies are resolved by their guid (and not the AB hash nor CRC)
                //             So to ensure dependencies are being kept in subsequent editor runs we normalize the asset guid using
                //             the CID.
                string metaContent = File.ReadAllText(metaPath);
                string guid = AssetBundleBuilderUtils.GetGUID(hash);
                string result = Regex.Replace(metaContent, @"guid: \w+?\n", $"guid: {guid}\n");

                File.WriteAllText(metaPath, result);

                var importer = AssetImporter.GetAtPath(finalDownloadedAssetDbPath + assetPath);
                importer.SaveAndReimport();

                Debug.Log("guid should be " + guid);
                Debug.Log("guid is " + AssetDatabase.AssetPathToGUID(finalDownloadedAssetDbPath + assetPath));

                if (fullPathToTag != null)
                {
                    pathsToTag.Add(fullPathToTag, hash);
                }
                else
                {
                    throw new Exception("Failed to get texture dependencies! failing asset: " + hash);
                }
            }

            foreach (var kvp in pathsToTag)
            {
                AssetBundleBuilderUtils.MarkForAssetBundleBuild(kvp.Key, kvp.Value);
            }
        }


        internal static void ExportSceneToAssetBundles_Internal(string sceneCid)
        {
            MappingsAPIData parcelInfoApiData = AssetBundleBuilderUtils.GetSceneMappingsData(sceneCid);

            AssetDatabase.Refresh();

            MappingPair[] rawContents = parcelInfoApiData.data[0].content.contents;

            var contentProvider = new DCL.ContentProvider();
            contentProvider.contents = new List<MappingPair>(rawContents);
            contentProvider.baseUrl = ContentServerUtils.GetContentAPIUrlBase(environment);
            contentProvider.BakeHashes();

            var contentProviderAB = new DCL.ContentProvider();
            contentProviderAB.contents = new List<MappingPair>(rawContents);
            contentProviderAB.baseUrl = ContentServerUtils.GetBundlesAPIUrlBase(environment);
            contentProviderAB.BakeHashes();

            var stringToAB = new Dictionary<string, AssetBundle>();

            var hashToGltfPair = AssetBundleBuilderUtils.FilterExtensions(rawContents, gltfExtensions);
            var hashToBufferPair = AssetBundleBuilderUtils.FilterExtensions(rawContents, bufferExtensions);

            Dictionary<string, string> pathsToTag = new Dictionary<string, string>();

            //NOTE(Brian): Prepare buffers. We should prepare all the dependencies in this phase.
            foreach (var kvp in hashToBufferPair)
            {
                string hash = kvp.Key;

                var result = DownloadAsset(contentProvider, hashToBufferPair, hash, hash + "/");

                if (result == null)
                {
                    throw new Exception("Failed to get buffer dependencies! failing asset: " + hash);
                }
            }

            GLTFImporter.OnGLTFRootIsConstructed -= AssetBundleBuilderUtils.FixGltfDependencyPaths;
            GLTFImporter.OnGLTFRootIsConstructed += AssetBundleBuilderUtils.FixGltfDependencyPaths;

            List<Stream> streamsToDispose = new List<Stream>();

            //NOTE(Brian): Prepare gltfs gathering its dependencies first and filling the importer's static cache.
            foreach (var kvp in hashToGltfPair)
            {
                string gltfHash = kvp.Key;

                if (skipAlreadyBuiltBundles)
                {
                    if (File.Exists(finalAssetBundlePath + gltfHash))
                    {
                        Debug.Log("Skipping existing gltf in AB folder: " + gltfHash);

                        if (!hashLowercaseToHashProper.ContainsKey(gltfHash.ToLower()))
                            hashLowercaseToHashProper.Add(gltfHash.ToLower(), gltfHash);

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
                        RetrieveAndInjectTexture(hashToGltfPair, gltfHash, mappingPair);
                    }

                    bool endsWithBufferExtensions = bufferExtensions.Any((x) => mappingPair.file.ToLower().EndsWith(x));

                    if (endsWithBufferExtensions)
                    {
                        string fileExt = Path.GetExtension(mappingPair.file);
                        string realOutputPath = finalDownloadedPath + mappingPair.hash + "/" + mappingPair.hash + fileExt;

                        if (File.Exists(realOutputPath))
                        {
                            Stream stream = File.OpenRead(realOutputPath);
                            string relativePath = AssetBundleBuilderUtils.GetRelativePathTo(hashToGltfPair[gltfHash].file, mappingPair.file);

                            // NOTE(Brian): This cache will be used by the GLTF importer when seeking streams. This way the importer will
                            //              consume the asset bundle dependencies instead of trying to create new streams.
                            PersistentAssetCache.StreamCacheByUri[relativePath] = new RefCountedStreamData(relativePath, stream);
                        }
                    }
                }

                //NOTE(Brian): Finally, load the gLTF. The GLTFImporter will use the PersistentAssetCache to resolve the external dependencies.
                string path = DownloadAsset(contentProvider, hashToGltfPair, gltfHash, gltfHash + "/");

                if (path != null)
                {
                    AssetDatabase.Refresh();
                    AssetDatabase.SaveAssets();
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
                AssetBundleBuilderUtils.MarkForAssetBundleBuild(kvp.Key, kvp.Value);
            }

            foreach (var s in streamsToDispose)
            {
                s.Dispose();
            }
        }

        internal static void BuildAssetBundles()
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate | ImportAssetOptions.ImportRecursive);
            AssetDatabase.SaveAssets();

            AssetDatabase.MoveAsset(finalDownloadedAssetDbPath, DOWNLOADED_ASSET_DB_PATH_ROOT);

            var manifest = BuildPipeline.BuildAssetBundles(finalAssetBundlePath, BuildAssetBundleOptions.UncompressedAssetBundle | BuildAssetBundleOptions.ForceRebuildAssetBundle, BuildTarget.WebGL);

            if (manifest == null)
            {
                OnBundleBuildFinish?.Invoke(2);
                throw new Exception("Error generating asset bundle!");
            }

            string[] assetBundles = manifest.GetAllAssetBundles();
            string[] assetBundlePaths = new string[assetBundles.Length];

            string finalLog = "";
            finalLog += $"Total generated asset bundles: {assetBundles.Length}\n";

            AssetBundleBuilderUtils.GenerateDependencyMaps(hashLowercaseToHashProper, manifest);

            for (int i = 0; i < assetBundles.Length; i++)
            {
                if (string.IsNullOrEmpty(assetBundles[i]))
                    continue;

                finalLog += $"#{i} Generated asset bundle name: {assetBundles[i]}\n";

                try
                {
                    //NOTE(Brian): This is done for correctness sake, rename files to preserve the hash upper-case
                    if (hashLowercaseToHashProper.TryGetValue(assetBundles[i], out string hashWithUppercase))
                    {
                        string oldPath = finalAssetBundlePath + assetBundles[i];
                        string path = finalAssetBundlePath + hashWithUppercase;
                        File.Move(oldPath, path);
                        assetBundlePaths[i] = path;
                    }

                    string oldPathMf = finalAssetBundlePath + assetBundles[i] + ".manifest";
                    File.Delete(oldPathMf);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Error! " + e.Message);
                }
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

            Debug.Log(finalLog);
            OnBundleBuildFinish?.Invoke(0);
        }


        static void DumpSceneList(List<string> sceneCidsList, System.Action<int> OnFinish)
        {
            if (sceneCidsList == null || sceneCidsList.Count == 0)
            {
                Debug.LogError("Scene list is null or count == 0!");
                OnFinish?.Invoke(1);
                return;
            }

            Debug.Log($"Building {sceneCidsList.Count} scenes...");
            startTime = Time.realtimeSinceStartup;

            InitializeDirectoryPaths(false);

            OnBundleBuildFinish = (errorCode) => { Debug.Log($"Conversion finished. [Time:{Time.realtimeSinceStartup - startTime}]"); OnFinish?.Invoke(errorCode); };

            float timer = Time.realtimeSinceStartup;

            EditorApplication.update = () =>
            {
                try
                {
                    //NOTE(Brian): We have to check this because the ImportAsset for GLTFs is not synchronous, and must execute some delayed calls
                    //             after the import asset finished. Therefore, we have to make sure those calls finished before continuing.
                    if (!GLTFImporter.finishedImporting || Time.realtimeSinceStartup - timer > 60)
                        return;

                    if (sceneCidsList.Count > 0)
                    {
                        ExportSceneToAssetBundles_Internal(sceneCidsList[0]);
                        sceneCidsList.RemoveAt(0);
                        timer = Time.realtimeSinceStartup;
                        return;
                    }

                    if (!Directory.Exists(finalAssetBundlePath))
                        Directory.CreateDirectory(finalAssetBundlePath);

                    EditorApplication.update = null;

                    BuildAssetBundles();
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                    AssetBundleBuilderUtils.Exit(1);
                    EditorApplication.update = null;
                }
            };
        }

        static void DumpTexturesFromSceneList(List<string> sceneCidsList, System.Action<int> OnFinish)
        {
            if (sceneCidsList == null || sceneCidsList.Count == 0)
            {
                Debug.LogError("Scene list is null or count == 0!");
                OnFinish?.Invoke(1);
                return;
            }

            Debug.Log($"Preparing {sceneCidsList.Count} scenes...");
            startTime = Time.realtimeSinceStartup;

            InitializeDirectoryPaths(true);

            foreach (string sceneCid in sceneCidsList)
            {
                Debug.Log("preparing... " + sceneCid);
                DownloadSceneTextures(sceneCid);
            }

            OnFinish?.Invoke(0);
        }

        private static void RetrieveAndInjectTexture(Dictionary<string, MappingPair> hashToGltfPair, string gltfHash, MappingPair mappingPair)
        {
            string fileExt = Path.GetExtension(mappingPair.file);
            string realOutputPath = finalDownloadedPath + mappingPair.hash + "/" + mappingPair.hash + fileExt;
            Texture2D t2d = null;

            if (File.Exists(realOutputPath))
            {
                string outputPath = finalDownloadedAssetDbPath + mappingPair.hash + "/" + mappingPair.hash + fileExt;
                t2d = AssetDatabase.LoadAssetAtPath<Texture2D>(outputPath);
                Debug.Log($"injecting texture dependency. guid: {AssetDatabase.AssetPathToGUID(outputPath)}");
            }

            if (t2d != null)
            {
                string relativePath = AssetBundleBuilderUtils.GetRelativePathTo(hashToGltfPair[gltfHash].file, mappingPair.file);
                //NOTE(Brian): This cache will be used by the GLTF importer when seeking textures. This way the importer will
                //             consume the asset bundle dependencies instead of trying to create new textures.
                PersistentAssetCache.ImageCacheByUri[relativePath] = new RefCountedTextureData(relativePath, t2d);
            }
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

            UnityWebRequest req;

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

            if (VERBOSE)
            {
                Debug.Log($"Downloaded asset = {finalUrl} to {outputPathDir}");
            }
            if (!Directory.Exists(outputPathDir))
                Directory.CreateDirectory(outputPathDir);

            File.WriteAllBytes(outputPath, req.downloadHandler.data);


            if (!hashLowercaseToHashProper.ContainsKey(hashLowercase))
                hashLowercaseToHashProper.Add(hashLowercase, hash);

            return finalDownloadedPath + additionalPath;
        }


        internal static void DumpArea(Vector2Int coords, Vector2Int size, Action<int> OnFinish = null)
        {
            HashSet<string> sceneCids = AssetBundleBuilderUtils.GetSceneCids(coords, size);

            List<string> sceneCidsList = sceneCids.ToList();
            DumpSceneList(sceneCidsList, OnFinish);
        }

        internal static void DumpArea(List<Vector2Int> coords, Action<int> OnFinish = null)
        {
            HashSet<string> sceneCids = AssetBundleBuilderUtils.GetScenesCids(coords);

            List<string> sceneCidsList = sceneCids.ToList();
            DumpSceneList(sceneCidsList, OnFinish);
        }

        internal static void DumpAreaTextures(Vector2Int coords, Vector2Int size, Action<int> OnFinish = null)
        {
            HashSet<string> sceneCids = AssetBundleBuilderUtils.GetSceneCids(coords, size);

            List<string> sceneCidsList = sceneCids.ToList();
            DumpTexturesFromSceneList(sceneCidsList, OnFinish);
        }

        internal static void DumpScene(string cid, Action<int> OnFinish = null)
        {
            DumpSceneList(new List<string> { cid }, OnFinish);
        }

        internal static void DumpTexturesFromScene(string cid, Action<int> OnFinish = null)
        {
            DumpTexturesFromSceneList(new List<string> { cid }, OnFinish);
        }

        static void InitializeDirectoryPaths(bool deleteIfExists)
        {
            finalAssetBundlePath = ASSET_BUNDLES_PATH_ROOT + "/";
            finalDownloadedPath = DOWNLOADED_PATH_ROOT + "/";
            finalDownloadedAssetDbPath = DOWNLOADED_ASSET_DB_PATH_ROOT + "/";

            AssetBundleBuilderUtils.InitializeDirectory(finalDownloadedPath, deleteIfExists);
        }
    }
}
