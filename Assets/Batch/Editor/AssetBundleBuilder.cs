using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityGLTF.Cache;

public static class AssetBundleBuilder
{
    private static string DOWNLOADED_ASSET_DB_PATH = "Assets/_Downloaded/";
    private static string DOWNLOADED_PATH = Application.dataPath + "/_Downloaded/";
    private static string ASSET_BUNDLES_PATH = "/_AssetBundles/";

    [MenuItem("AssetBundleBuilder/Download Test")]
    public static void TestExample()
    {
        ExportSceneToAssetBundles_Internal("QmbKgHPENpzGGEfagGP5BbEd7CvqXeXuuXLeMEkuswGvrK");
    }


    [System.Serializable]
    public class ParcelInfoAPI
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
        //TODO(Brian): Read arguments from command line
        ExportSceneToAssetBundles_Internal("QmSAIOJDAOSIDJO");
    }


    private static void ExportSceneToAssetBundles_Internal(string sceneCid)
    {
        InitializeDirectory(DOWNLOADED_PATH);
        InitializeDirectory(ASSET_BUNDLES_PATH);
        string url = "https://content.decentraland.org/parcel_info?cids=" + sceneCid;
        UnityWebRequest w = UnityWebRequest.Get(url);
        w.SendWebRequest();

        while (w.isDone == false) { }

        ParcelInfoAPI parcelInfoApiData = JsonUtility.FromJson<ParcelInfoAPI>(w.downloadHandler.text);

        DCL.ContentProvider.MappingPair[] rawContents = parcelInfoApiData.data[0].content.contents;

        var contentProvider = new DCL.ContentProvider();
        contentProvider.contents = new List<DCL.ContentProvider.MappingPair>(rawContents);
        contentProvider.baseUrl = "https://content.decentraland.org/contents/";
        contentProvider.BakeHashes();

        var contentProviderAB = new DCL.ContentProvider();
        contentProviderAB.contents = new List<DCL.ContentProvider.MappingPair>(rawContents);
        contentProviderAB.baseUrl = "https://asset-bundle-content.decentraland.org/contents/";
        contentProviderAB.BakeHashes();

        string[] textureExtensions = { ".jpg", ".png" };
        string[] gltfExtensions = { ".glb", ".gltf" };

        List<string> assetBundleOutputPaths = new List<string>();

        var stringToAB = new Dictionary<string, AssetBundle>();
        var hashToTexturePair = new Dictionary<string, DCL.ContentProvider.MappingPair>();
        var hashToGltfPair = new Dictionary<string, DCL.ContentProvider.MappingPair>();

        hashToTexturePair = FilterExtensions(rawContents, textureExtensions);
        hashToGltfPair = FilterExtensions(rawContents, gltfExtensions);

        //NOTE(Brian): Prepare textures. We should prepare all the dependencies in this phase.
        foreach (var kvp in hashToTexturePair)
        {
            string hash = kvp.Key;

            //NOTE(Brian): try to get an AB before getting the original texture, so we bind the dependencies correctly
            bool dependencyAlreadyIsAB = PrepareUrlContents(contentProviderAB, hashToTexturePair, hash);

            if (!dependencyAlreadyIsAB)
            {
                PrepareUrlContentsForBundleBuild(contentProvider, hashToTexturePair, hash);
            }
        }

        //NOTE(Brian): Prepare gltfs gathering its dependencies first and filling the importer's static cache.
        foreach (var kvp in hashToGltfPair)
        {
            string gltfHash = kvp.Key;
            PersistentAssetCache.ImageCacheByUri.Clear();

            foreach (var mappingPair in rawContents)
            {
                bool endsWithTextureExtensions = textureExtensions.Any((x) => mappingPair.file.ToLower().EndsWith(x));

                if (endsWithTextureExtensions)
                {
                    string relativePath = GetRelativePathTo(hashToGltfPair[gltfHash].file, mappingPair.file);
                    string fileExt = Path.GetExtension(mappingPair.file);
                    string outputPath = DOWNLOADED_ASSET_DB_PATH + mappingPair.hash + fileExt;

                    Texture2D t2d = AssetDatabase.LoadAssetAtPath<Texture2D>(outputPath);

                    if (t2d != null)
                    {
                        //NOTE(Brian): This cache will be used by the GLTF importer when seeking textures. This way the importer will
                        //             consume the asset bundle dependencies instead of trying to create new textures.
                        PersistentAssetCache.ImageCacheByUri[relativePath] = new RefCountedTextureData(relativePath, t2d);
                    }
                }
            }

            PrepareUrlContentsForBundleBuild(contentProvider, hashToGltfPair, gltfHash, gltfHash + "/");
        }


        if (!Directory.Exists(ASSET_BUNDLES_PATH))
            Directory.CreateDirectory(ASSET_BUNDLES_PATH);

        EditorApplication.delayCall += () =>
        {
            AssetDatabase.SaveAssets();
            var manifest = BuildPipeline.BuildAssetBundles(ASSET_BUNDLES_PATH, BuildAssetBundleOptions.UncompressedAssetBundle, BuildTarget.WebGL);

            string[] assetBundles = manifest.GetAllAssetBundles();
            string[] assetBundlePaths = new string[assetBundles.Length];

            for (int i = 0; i < assetBundles.Length; i++)
            {
                if (string.IsNullOrEmpty(assetBundles[i]))
                    continue;

                DCL.ContentProvider.MappingPair pair = contentProvider.contents.FirstOrDefault((x) => assetBundles[i].Contains(x.hash.ToLower()));

                if (pair == null)
                {
                    continue;
                }

                //NOTE(Brian): This is done for correctness sake, rename files to preserve the hash upper-case
                string hashWithUppercase = pair.hash;
                string oldPath = ASSET_BUNDLES_PATH + assetBundles[i];
                string path = ASSET_BUNDLES_PATH + hashWithUppercase;

                string oldPathMf = ASSET_BUNDLES_PATH + assetBundles[i] + ".manifest";
                string pathMf = ASSET_BUNDLES_PATH + hashWithUppercase + ".manifest";

                File.Move(oldPath, path);
                File.Move(oldPathMf, pathMf);

                assetBundlePaths[i] = path;
            }

            UploadBundles(assetBundlePaths);
        };

        AssetDatabase.Refresh();
    }

    private static void UploadBundles(string[] bundlePaths)
    {
        List<UnityWebRequest> requests = new List<UnityWebRequest>();

        foreach (string assetBundlePath in bundlePaths)
        {
            byte[] rawData = File.ReadAllBytes(assetBundlePath);
            string fileName = Path.GetFileNameWithoutExtension(assetBundlePath);

            string fullUrl = $"http://content-assets-as-bundle.decentraland.zone.s3.amazonaws.com/{fileName}";

            var req = UnityWebRequest.Put(fullUrl, rawData);
            req.SendWebRequest();

            requests.Add(req);
        }

        bool requestsAreDone = false;

        while (requestsAreDone == false)
        {
            requestsAreDone = true;

            foreach (var request in requests)
            {
                if (!request.isDone)
                {
                    requestsAreDone = false;
                }
                else
                {
                    if (request.isHttpError || request.isNetworkError)
                    {
                        Debug.Log($"FAIL!. Upload request response: {request.responseCode}... error: {request.error}");
                    }
                    else
                    {
                        Debug.Log($"SUCCESS!. Upload request response: {request.responseCode}... progress: {request.uploadProgress}... bytes uploaded: {request.uploadedBytes}");
                    }
                }
            }
        }
    }

    private static bool PrepareUrlContents(DCL.ContentProvider contentProvider, Dictionary<string, DCL.ContentProvider.MappingPair> filteredPairs, string hash, string additionalPath = "")
    {
        string finalUrl = contentProvider.GetContentsUrl(filteredPairs[hash].file);

        UnityWebRequest req = UnityWebRequest.Get(finalUrl);
        req.SendWebRequest();
        while (req.isDone == false) { }

        if (req.isHttpError)
            return false;

        //TODO: Make AB with jpg/png files
        string fileExt = Path.GetExtension(filteredPairs[hash].file);
        string outputPath = DOWNLOADED_PATH + additionalPath + hash + fileExt;
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
        PrepareUrlContents(contentProvider, filteredPairs, hash, additionalPath);

        string fileExt = Path.GetExtension(filteredPairs[hash].file);
        AssetImporter importer = AssetImporter.GetAtPath(DOWNLOADED_ASSET_DB_PATH + additionalPath + hash + fileExt);
        importer.SetAssetBundleNameAndVariant(hash, "");
        return true;
    }

    private static void InitializeDirectory(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        DirectoryInfo di = new DirectoryInfo(path);

        foreach (FileInfo file in di.GetFiles())
        {
            file.Delete();
        }
        foreach (DirectoryInfo dir in di.GetDirectories())
        {
            dir.Delete(true);
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
