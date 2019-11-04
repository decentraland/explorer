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

namespace DCL
{
    public static class AssetBundleBuilder
    {
        [System.Serializable]
        public class AssetDependencyMap
        {
            public string[] dependencies;
        }

        private static string DOWNLOADED_ASSET_DB_PATH_ROOT = "Assets/_Downloaded/";
        private static string DOWNLOADED_PATH_ROOT = Application.dataPath + "/_Downloaded/";
        private static string ASSET_BUNDLE_FOLDER_NAME = "_AssetBundles2";
        private static string ASSET_BUNDLES_PATH_ROOT = "/" + ASSET_BUNDLE_FOLDER_NAME + "/";

        private static string finalAssetBundlePath = "";
        private static string finalDownloadedPath = "";
        private static string finalDownloadedAssetDbPath = "";

        public static ContentServerUtils.ApiEnvironment environment = ContentServerUtils.ApiEnvironment.ORG;


        [MenuItem("AssetBundleBuilder/Dump Test Scene")]
        public static void DumpMisc()
        {
            ExportSceneToAssetBundles_Internal("QmbKgHPENpzGGEfagGP5BbEd7CvqXeXuuXLeMEkuswGvrK");
        }


        [MenuItem("AssetBundleBuilder/Dump Museum District")]
        public static void DumpMuseum()
        {
            environment = ContentServerUtils.ApiEnvironment.ORG;
            DumpArea(new List<Vector2Int>() { new Vector2Int(13, 75) });
        }

        [MenuItem("AssetBundleBuilder/Dump Zone 64,-64")]
        public static void DumpZoneArea()
        {
            environment = ContentServerUtils.ApiEnvironment.ZONE;

            List<Vector2Int> coords = new List<Vector2Int>();

            int width = 2;
            int height = 2;

            for (int x = 64 - width; x < 64 + width; x++)
            {
                for (int y = -64 - height; y < -64 + height; y++)
                {
                    coords.Add(new Vector2Int(x, y));
                }
            }

            DumpArea(coords);
        }

        [MenuItem("AssetBundleBuilder/Only Build Bundles")]
        public static void OnlyBuildBundles()
        {
            finalAssetBundlePath = ASSET_BUNDLES_PATH_ROOT;
            BuildPipeline.BuildAssetBundles(finalAssetBundlePath, BuildAssetBundleOptions.UncompressedAssetBundle | BuildAssetBundleOptions.ForceRebuildAssetBundle, BuildTarget.WebGL);
        }

        static void DumpArea(List<Vector2Int> coords)
        {
            HashSet<string> sceneCids = new HashSet<string>();

            foreach (Vector2Int v in coords)
            {
                string url = ContentServerUtils.GetScenesAPIUrl(environment, v.x, v.y, 0, 0);
                UnityWebRequest w = UnityWebRequest.Get(url);
                w.SendWebRequest();

                while (w.isDone == false) { }

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

            sceneCidsList = sceneCids.ToList();
            Debug.Log($"Building {sceneCidsList.Count} scenes...");

            startTime = Time.realtimeSinceStartup;
            foreach (var v in sceneCidsList)
            {
                ExportSceneToAssetBundles_Internal(v);
            }

            BuildAssetBundles();
            OnBundleBuildFinish = () => { Debug.Log($"Conversion finished. [Time:{Time.realtimeSinceStartup - startTime}]"); };
        }

        static float startTime;
        static List<string> sceneCidsList;
        static System.Action OnBundleBuildFinish = null;
        static Dictionary<string, string> hashLowercaseToHashProper = new Dictionary<string, string>();

        [System.Serializable]
        public class ScenesAPIData
        {
            [System.Serializable]
            public class Data
            {
                public string parcel_id;
                public string root_cid;
                public string scene_cid;
            }

            public Data[] data;
        }

        [System.Serializable]
        public class MappingsAPIData
        {
            [System.Serializable]
            public class Data
            {
                [System.Serializable]
                public class Content
                {
                    public DCL.ContentProvider.MappingPair[] contents;
                }

                public Content content;
            }

            public Data[] data;
        }



        public static void ExportSceneToAssetBundles()
        {
            try
            {
                string[] args = Environment.GetCommandLineArgs();

                string sceneCid = "";

                for (int i = 0; i < args.Length - 1; i++)
                {
                    if (args[i] == "-sceneCid")
                    {
                        sceneCid = args[i + 1];
                    }
                }

                if (string.IsNullOrEmpty(sceneCid))
                {
                    throw new ArgumentException("Invalid sceneCid argument! Please use -sceneCid <id> to establish the desired id to process.");
                }

                ExportSceneToAssetBundles_Internal(sceneCid);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);

                if (Application.isBatchMode)
                    EditorApplication.Exit(1);
            }
        }

        private static void ExportSceneToAssetBundles_Internal(string sceneCid)
        {
            Debug.Log($"Exporting scene... {sceneCid}");
            finalAssetBundlePath = ASSET_BUNDLES_PATH_ROOT;
            finalDownloadedPath = DOWNLOADED_PATH_ROOT;
            finalDownloadedAssetDbPath = DOWNLOADED_ASSET_DB_PATH_ROOT;

            if (File.Exists(finalAssetBundlePath + "/manifests/" + sceneCid))
            {
                Debug.Log("Scene already exists!");
                return;
            }

            string url = ContentServerUtils.GetMappingsAPIUrl(environment, sceneCid);
            UnityWebRequest w = UnityWebRequest.Get(url);
            w.SendWebRequest();

            while (w.isDone == false) { }

            MappingsAPIData parcelInfoApiData = JsonUtility.FromJson<MappingsAPIData>(w.downloadHandler.text);

            if (parcelInfoApiData.data.Length == 0 || parcelInfoApiData.data == null)
            {
                Debug.LogWarning("Data is null?");

                if (Application.isBatchMode)
                    EditorApplication.Exit(1);

                return;
            }

            InitializeDirectory(finalDownloadedPath);
            AssetDatabase.Refresh();

            DCL.ContentProvider.MappingPair[] rawContents = parcelInfoApiData.data[0].content.contents;

            var contentProvider = new DCL.ContentProvider();
            contentProvider.contents = new List<DCL.ContentProvider.MappingPair>(rawContents);
            contentProvider.baseUrl = ContentServerUtils.GetContentAPIUrlBase(environment);
            contentProvider.BakeHashes();

            var contentProviderAB = new DCL.ContentProvider();
            contentProviderAB.contents = new List<DCL.ContentProvider.MappingPair>(rawContents);
            contentProviderAB.baseUrl = ContentServerUtils.GetBundlesAPIUrlBase(environment);
            contentProviderAB.BakeHashes();

            string[] bufferExtensions = { ".bin" };
            string[] textureExtensions = { ".jpg", ".png" };
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
                PrepareUrlContents(contentProvider, hashToBufferPair, hash, hash + "/");
            }

            //NOTE(Brian): Prepare textures. We should prepare all the dependencies in this phase.
            foreach (var kvp in hashToTexturePair)
            {
                string hash = kvp.Key;

                //NOTE(Brian): try to get an AB before getting the original texture, so we bind the dependencies correctly
                string fullPathToTag = PrepareUrlContents(contentProvider, hashToTexturePair, hash, hash + "/");

                if (fullPathToTag != null)
                    pathsToTag.Add(fullPathToTag, hash);
            }

            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
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
                string path = PrepareUrlContents(contentProvider, hashToGltfPair, gltfHash, gltfHash + "/");

                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
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

        static void MarkForAssetBundleBuild(string path, string abName)
        {
            string assetPath = path.Substring(path.IndexOf("Assets"));
            assetPath = Path.ChangeExtension(assetPath, null);

            assetPath = assetPath.Substring(0, assetPath.Length - 1);
            AssetImporter a = AssetImporter.GetAtPath(assetPath);
            a.SetAssetBundleNameAndVariant(abName, "");
        }

        private static void BuildAssetBundles()
        {
            if (!Directory.Exists(finalAssetBundlePath))
                Directory.CreateDirectory(finalAssetBundlePath);

            EditorApplication.CallbackFunction delayedCall = null;

            float time = Time.realtimeSinceStartup;

            delayedCall = () =>
            {
                if (Time.realtimeSinceStartup - time < 2.0f)
                    return;

                EditorApplication.update -= delayedCall;
                AssetDatabase.SaveAssets();

                var manifest = BuildPipeline.BuildAssetBundles(finalAssetBundlePath, BuildAssetBundleOptions.UncompressedAssetBundle, BuildTarget.WebGL);

                if (manifest == null)
                {
                    Debug.LogError("Error generating asset bundle!");
                    OnBundleBuildFinish?.Invoke();

                    if (Application.isBatchMode)
                        EditorApplication.Exit(1);

                    return;
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

                OnBundleBuildFinish?.Invoke();

                if (Application.isBatchMode)
                    EditorApplication.Exit(0);
            };

            AssetDatabase.Refresh();
            EditorApplication.update += delayedCall;
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

        private static string PrepareUrlContents(DCL.ContentProvider contentProvider, Dictionary<string, DCL.ContentProvider.MappingPair> filteredPairs, string hash, string additionalPath = "")
        {
            string fileExt = Path.GetExtension(filteredPairs[hash].file);
            string outputPath = finalDownloadedPath + additionalPath + hash + fileExt;
            string outputPathDir = Path.GetDirectoryName(outputPath);
            string finalUrl = contentProvider.GetContentsUrl(filteredPairs[hash].file);
            string hashLowercase = hash.ToLowerInvariant();

            if (File.Exists(outputPath))
            {
                if (!hashLowercaseToHashProper.ContainsKey(hashLowercase))
                    hashLowercaseToHashProper.Add(hashLowercase, hash);

                return null;
            }

            UnityWebRequest req = UnityWebRequest.Get(finalUrl);
            req.SendWebRequest();
            while (req.isDone == false) { }

            if (req.isHttpError || req.isNetworkError)
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
        public static Dictionary<string, DCL.ContentProvider.MappingPair> FilterExtensions(DCL.ContentProvider.MappingPair[] pairsToSearch, string[] extensions)
        {
            var result = new Dictionary<string, DCL.ContentProvider.MappingPair>();

            for (int i = 0; i < pairsToSearch.Length; i++)
            {
                DCL.ContentProvider.MappingPair mappingPair = pairsToSearch[i];

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
