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
        ExportSceneToAssetBundles("QmbKgHPENpzGGEfagGP5BbEd7CvqXeXuuXLeMEkuswGvrK", "");
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

    class MappingPairFilter
    {
        public Dictionary<string, DCL.ContentProvider.MappingPair> hashToFilteredPair;

        public void Filter(string[] extensions)
        {
        }
    }

    static string[] textureExtensions = { ".jpg", ".png" };
    static string[] gltfExtensions = { ".glb", ".gltf" };
    static List<string> assetBundleOutputPaths = new List<string>();
    static Dictionary<string, AssetBundle> stringToAB = new Dictionary<string, AssetBundle>();
    static Dictionary<string, DCL.ContentProvider.MappingPair> hashToTexturePair = new Dictionary<string, DCL.ContentProvider.MappingPair>();
    static Dictionary<string, DCL.ContentProvider.MappingPair> hashToGltfPair = new Dictionary<string, DCL.ContentProvider.MappingPair>();

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
        contentProviderAB.baseUrl = "https://content.decentraland.org/contents/";
        contentProviderAB.BakeHashes();


        //TODO: Download all jpg/png files
        foreach (var mappingPair in rawContents)
        {
            bool endsWithTextureExtensions = textureExtensions.Any((x) => mappingPair.file.ToLower().EndsWith(x));

            if (endsWithTextureExtensions)
            {
                if (!hashToTexturePair.ContainsKey(mappingPair.hash))
                {
                    hashToTexturePair.Add(mappingPair.hash, mappingPair);
                }
            }

            bool endsWithGltfExtensions = gltfExtensions.Any((x) => mappingPair.file.ToLower().EndsWith(x));

            if (endsWithGltfExtensions)
            {
                if (!hashToGltfPair.ContainsKey(mappingPair.hash))
                {
                    hashToGltfPair.Add(mappingPair.hash, mappingPair);
                }
            }
        }

        string[] textureHashes = hashToTexturePair.Keys.ToArray();

        foreach (string hash in textureHashes)
        {
            //TODO(Brian): try to get an AB before getting the original texture, so we bind the dependencies correctly
            string finalUrl = contentProvider.GetContentsUrl(hashToTexturePair[hash].file);

            UnityWebRequest req = UnityWebRequest.Get(finalUrl);
            req.SendWebRequest();
            while (req.isDone == false) { }

            //TODO: Make AB with jpg/png files
            string fileExt = Path.GetExtension(hashToTexturePair[hash].file);
            string outputPath = DOWNLOADED_PATH + hash + fileExt;
            string outputPathDir = Path.GetDirectoryName(outputPath);

            if (!Directory.Exists(outputPathDir))
                Directory.CreateDirectory(outputPathDir);

            File.WriteAllBytes(outputPath, req.downloadHandler.data);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            AssetImporter importer = null;

            importer = AssetImporter.GetAtPath(DOWNLOADED_ASSET_DB_PATH + hash + fileExt);

            importer.SetAssetBundleNameAndVariant(hash, "");
        }

        string[] gltfHashes = hashToGltfPair.Keys.ToArray();

        foreach (string gltfHash in gltfHashes)
        {
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
                    PersistentAssetCache.ImageCacheByUri[relativePath] = new RefCountedTextureData(relativePath, t2d);
                }
            }

            {
                string finalUrl = contentProvider.GetContentsUrl(hashToGltfPair[gltfHash].file);
                UnityWebRequest req = UnityWebRequest.Get(finalUrl);
                req.SendWebRequest();
                while (req.isDone == false) { }

                //TODO: Import GLTF so it uses the new images
                string fileExt = Path.GetExtension(hashToGltfPair[gltfHash].file);
                string outputPath = DOWNLOADED_PATH + gltfHash + "/" + gltfHash + fileExt;
                string outputPathDir = Path.GetDirectoryName(outputPath);

                if (!Directory.Exists(outputPathDir))
                    Directory.CreateDirectory(outputPathDir);

                File.WriteAllBytes(outputPath, req.downloadHandler.data);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                AssetImporter importer = null;

                importer = AssetImporter.GetAtPath(DOWNLOADED_ASSET_DB_PATH + gltfHash + "/" + gltfHash + fileExt);
                importer.SetAssetBundleNameAndVariant(gltfHash, "");
            }
        }


        if (!Directory.Exists(ASSET_BUNDLES_PATH))
            Directory.CreateDirectory(ASSET_BUNDLES_PATH);

        EditorApplication.delayCall += () =>
        {
            AssetDatabase.SaveAssets();
            var manifest = BuildPipeline.BuildAssetBundles(ASSET_BUNDLES_PATH, BuildAssetBundleOptions.UncompressedAssetBundle, BuildTarget.WebGL);

            string[] abs = manifest.GetAllAssetBundles();

            for (int i = 0; i < abs.Length; i++)
            {
                if (string.IsNullOrEmpty(abs[i]))
                    continue;

                DCL.ContentProvider.MappingPair pair = contentProvider.contents.FirstOrDefault((x) => abs[i].Contains(x.hash.ToLower()));

                if (pair == null)
                {
                    continue;
                }

                string hashWithUppercase = pair.hash;
                string oldPath = ASSET_BUNDLES_PATH + abs[i];
                string path = ASSET_BUNDLES_PATH + hashWithUppercase;

                string oldPathMf = ASSET_BUNDLES_PATH + abs[i] + ".manifest";
                string pathMf = ASSET_BUNDLES_PATH + hashWithUppercase + ".manifest";

                File.Move(oldPath, path);
                File.Move(oldPathMf, pathMf);
            }
        };

        AssetDatabase.Refresh();
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
