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

public static class ContentServerUtils
{
    public enum ApiEnvironment
    {
        NONE,
        TODAY,
        ZONE,
        ORG,
    }


    public static string GetEnvString(ApiEnvironment env)
    {
        switch (env)
        {
            case ApiEnvironment.NONE:
                break;
            case ApiEnvironment.TODAY:
                return "today";
            case ApiEnvironment.ZONE:
                return "zone";
            case ApiEnvironment.ORG:
                return "org";
        }

        return "org";
    }

    public static string GetScenesAPIUrl(ApiEnvironment env, int x1, int y1, int width, int height)
    {
        string envString = GetEnvString(env);
        return $"https://content.decentraland.{envString}/scenes?x1={x1}&x2={x1 + width}&y1={y1}&y2={y1 + height}";
    }

    public static string GetMappingsAPIUrl(ApiEnvironment env, string cid)
    {
        string envString = GetEnvString(env);
        return $"https://content.decentraland.{envString}/parcel_info?cids={cid}";
    }

    public static string GetContentAPIUrlBase(ApiEnvironment env)
    {
        string envString = GetEnvString(env);
        return $"https://content.decentraland.{envString}/contents/";
    }

    public static string GetBundlesAPIUrlBase(ApiEnvironment env)
    {
        //string envString = GetEnvString(env);
        return $"http://localhost:1338/";//"https://content-as-bundle.decentraland.org/contents/";
    }
}

public static class AssetBundleBuilder
{
    private static string DOWNLOADED_ASSET_DB_PATH_ROOT = "Assets/_Downloaded/";
    private static string DOWNLOADED_PATH_ROOT = Application.dataPath + "/_Downloaded/";
    private static string ASSET_BUNDLE_FOLDER_NAME = "_AssetBundles";
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
        ExportSceneToAssetBundles_Internal("QmQCHrim1KkQ4KDfHKqgmxwGzeZzY9bDB63MfHrqEG3hcm");
    }

    [MenuItem("AssetBundleBuilder/Dump Zone 64,-64")]
    public static void DumpZoneArea()
    {
        environment = ContentServerUtils.ApiEnvironment.ZONE;
        HashSet<string> sceneCids = new HashSet<string>();

        string url = ContentServerUtils.GetScenesAPIUrl(environment, 64, -64, 0, 0);
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

        List<string> sceneCidsList = sceneCids.ToList();
        ExportSceneToAssetBundles_Internal(sceneCidsList[0]);
    }

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
            string[] args = System.Environment.GetCommandLineArgs();

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

    private static void ExportSceneToAssetBundles_Internal(string sceneCid, System.Action OnFinish = null)
    {
        Debug.Log($"Exporting scene... {sceneCid}");
        finalAssetBundlePath = ASSET_BUNDLES_PATH_ROOT;
        finalDownloadedPath = DOWNLOADED_PATH_ROOT + $"{sceneCid}/";
        finalDownloadedAssetDbPath = DOWNLOADED_ASSET_DB_PATH_ROOT + $"{sceneCid}/";

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
        InitializeDirectory(finalAssetBundlePath);
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

        List<string> assetBundleOutputPaths = new List<string>();

        var stringToAB = new Dictionary<string, AssetBundle>();

        var hashToTexturePair = FilterExtensions(rawContents, textureExtensions);
        var hashToGltfPair = FilterExtensions(rawContents, gltfExtensions);
        var hashToBufferPair = FilterExtensions(rawContents, bufferExtensions);

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
            //bool dependencyAlreadyIsAB = PrepareUrlContents(contentProviderAB, hashToTexturePair, hash, hash + "/");

            //if (!dependencyAlreadyIsAB)
            //{
            PrepareUrlContentsForBundleBuild(contentProvider, hashToTexturePair, hash, hash + "/");
            //}
            //else
            //{
            //    Debug.Log("Recycling hash " + hash + "!");
            //}
        }

        GLTFImporter.OnGLTFRootIsConstructed -= FixGltfDependencyPaths;
        GLTFImporter.OnGLTFRootIsConstructed += FixGltfDependencyPaths;

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

                    Texture2D t2d = AssetDatabase.LoadAssetAtPath<Texture2D>(outputPath);

                    if (t2d != null)
                    {
                        //NOTE(Brian): This cache will be used by the GLTF importer when seeking textures. This way the importer will
                        //             consume the asset bundle dependencies instead of trying to create new textures.
                        PersistentAssetCache.ImageCacheByUri[relativePath] = new RefCountedTextureData(relativePath, t2d);
                    }
                }

                bool endsWithBufferExtensions = bufferExtensions.Any((x) => mappingPair.file.ToLower().EndsWith(x));

                if (endsWithBufferExtensions)
                {
                    string relativePath = GetRelativePathTo(hashToGltfPair[gltfHash].file, mappingPair.file);
                    string fileExt = Path.GetExtension(mappingPair.file);
                    string outputPath = finalDownloadedAssetDbPath + mappingPair.hash + "/" + mappingPair.hash + fileExt;

                    Stream stream = File.OpenRead(outputPath);

                    //NOTE(Brian): This cache will be used by the GLTF importer when seeking streams. This way the importer will
                    //             consume the asset bundle dependencies instead of trying to create new streams.
                    PersistentAssetCache.StreamCacheByUri[relativePath] = new RefCountedStreamData(relativePath, stream);
                }
            }

            //NOTE(Brian): Finally, load the gLTF. The GLTFImporter will use the PersistentAssetCache to resolve the external dependencies.
            PrepareUrlContentsForBundleBuild(contentProvider, hashToGltfPair, gltfHash, gltfHash + "/");

            foreach (var streamDataKvp in PersistentAssetCache.StreamCacheByUri)
            {
                streamDataKvp.Value.stream?.Dispose();
            }
        }

        BuildAssetBundles(sceneCid, contentProvider, OnFinish);
    }

    private static void BuildAssetBundles(string sceneCid, DCL.ContentProvider contentProvider, System.Action OnFinish)
    {
        if (!Directory.Exists(finalAssetBundlePath))
            Directory.CreateDirectory(finalAssetBundlePath);

        float time = Time.realtimeSinceStartup;

        EditorApplication.CallbackFunction delayedCall = null;

        delayedCall = () =>
        {
            EditorApplication.update -= delayedCall;
            AssetDatabase.SaveAssets();

            while (Time.realtimeSinceStartup - time < 0.1f) { }

            var manifest = BuildPipeline.BuildAssetBundles(finalAssetBundlePath, BuildAssetBundleOptions.UncompressedAssetBundle, BuildTarget.WebGL);

            if (manifest == null)
            {
                Debug.LogError("Error generating asset bundle!");
                OnFinish?.Invoke();

                if (Application.isBatchMode)
                    EditorApplication.Exit(1);

                return;
            }

            string[] assetBundles = manifest.GetAllAssetBundles();
            string[] assetBundlePaths = new string[assetBundles.Length];

            Debug.Log($"Total generated asset bundles: {assetBundles.Length}");

            for (int i = 0; i < assetBundles.Length; i++)
            {
                if (string.IsNullOrEmpty(assetBundles[i]))
                    continue;

                Debug.Log($"#{i} Generated asset bundle name: {assetBundles[i]}");

                DCL.ContentProvider.MappingPair pair = contentProvider.contents.FirstOrDefault((x) => assetBundles[i].Contains(x.hash.ToLower()));

                if (pair == null)
                {
                    continue;
                }

                //NOTE(Brian): This is done for correctness sake, rename files to preserve the hash upper-case
                string hashWithUppercase = pair.hash;
                string oldPath = finalAssetBundlePath + assetBundles[i];
                string path = finalAssetBundlePath + hashWithUppercase;

                string oldPathMf = finalAssetBundlePath + assetBundles[i] + ".manifest";

                File.Move(oldPath, path);
                File.Delete(oldPathMf);

                assetBundlePaths[i] = path;
            }

            //NOTE(Brian): Move manifest bundle to manifests folder
            string manifestDirectoryName = finalAssetBundlePath + "manifests/";

            if (!Directory.Exists(manifestDirectoryName))
                Directory.CreateDirectory(manifestDirectoryName);

            File.Move(finalAssetBundlePath + ASSET_BUNDLE_FOLDER_NAME, manifestDirectoryName + sceneCid);
            File.Delete(finalAssetBundlePath + ASSET_BUNDLE_FOLDER_NAME + ".manifest");

            OnFinish?.Invoke();

            if (Application.isBatchMode)
                EditorApplication.Exit(0);
        };

        EditorApplication.update += delayedCall;

        AssetDatabase.Refresh();
    }

    private static bool PrepareUrlContents(DCL.ContentProvider contentProvider, Dictionary<string, DCL.ContentProvider.MappingPair> filteredPairs, string hash, string additionalPath = "")
    {
        string finalUrl = contentProvider.GetContentsUrl(filteredPairs[hash].file);
        UnityWebRequest req = UnityWebRequest.Get(finalUrl);
        req.SendWebRequest();
        while (req.isDone == false) { }

        if (req.isHttpError || req.isNetworkError)
            return false;

        string fileExt = Path.GetExtension(filteredPairs[hash].file);
        string outputPath = finalDownloadedPath + additionalPath + hash + fileExt;
        string outputPathDir = Path.GetDirectoryName(outputPath);

        if (!Directory.Exists(outputPathDir))
            Directory.CreateDirectory(outputPathDir);

        File.WriteAllBytes(outputPath, req.downloadHandler.data);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        return true;
    }

    private static bool PrepareUrlContentsForBundleBuild(DCL.ContentProvider contentProvider, Dictionary<string, DCL.ContentProvider.MappingPair> filteredPairs, string hash, string additionalPath = "")
    {
        bool prepareSuccess = PrepareUrlContents(contentProvider, filteredPairs, hash, additionalPath);

        if (prepareSuccess)
        {
            List<AssetImporter> importers = GetAssetList(finalDownloadedPath + additionalPath);

            foreach (AssetImporter importer in importers)
            {
                importer.SetAssetBundleNameAndVariant(hash, "");
            }
        }

        return prepareSuccess;
    }

    public static List<AssetImporter> GetAssetList(string path)
    {
        string[] fileEntries = Directory.GetFiles(path);

        return fileEntries.Select(fileName =>
        {
            string assetPath = fileName.Substring(fileName.IndexOf("Assets"));
            assetPath = Path.ChangeExtension(assetPath, null);
            return AssetImporter.GetAtPath(assetPath);
        })
        .OfType<AssetImporter>()
        .ToList();
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
