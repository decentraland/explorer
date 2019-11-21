using DCL.Helpers;
using GLTF.Schema;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Networking;
using static DCL.ContentServerUtils;

[assembly: InternalsVisibleTo("AssetBundleBuilderTests")]
namespace DCL
{

    public static class AssetBundleBuilderUtils
    {
        public static List<Vector2Int> GetBottomLeftZoneArray(Vector2Int bottomLeftAnchor, Vector2Int size)
        {
            List<Vector2Int> coords = new List<Vector2Int>();

            for (int x = bottomLeftAnchor.x; x < bottomLeftAnchor.x + size.x; x++)
            {
                for (int y = bottomLeftAnchor.y; y < bottomLeftAnchor.y + size.y; y++)
                {
                    coords.Add(new Vector2Int(x, y));
                }
            }

            return coords;
        }

        public static List<Vector2Int> GetCenteredZoneArray(Vector2Int center, Vector2Int size)
        {
            List<Vector2Int> coords = new List<Vector2Int>();

            for (int x = center.x - size.x; x < center.x + size.x; x++)
            {
                for (int y = center.y - size.y; y < center.y + size.y; y++)
                {
                    coords.Add(new Vector2Int(x, y));
                }
            }

            return coords;
        }

        internal static bool ParseOption(string optionName, int argsQty, out string[] foundArgs)
        {
            return ParseOptionExplicit(Environment.GetCommandLineArgs(), optionName, argsQty, out foundArgs);
        }

        internal static bool ParseOptionExplicit(string[] rawArgsList, string optionName, int expectedArgsQty, out string[] foundArgs)
        {
            foundArgs = null;

            if (rawArgsList == null || rawArgsList.Length < expectedArgsQty + 1)
                return false;

            expectedArgsQty = Mathf.Min(expectedArgsQty, 100);

            var foundArgsList = new List<string>();
            int argState = 0;

            for (int i = 0; i < rawArgsList.Length; i++)
            {
                switch (argState)
                {
                    case 0:
                        if (rawArgsList[i] == "-" + optionName)
                        {
                            argState++;
                        }
                        break;
                    default:
                        foundArgsList.Add(rawArgsList[i]);
                        argState++;
                        break;
                }

                if (argState > 0 && foundArgsList.Count == expectedArgsQty)
                    break;
            }

            if (argState == 0 || foundArgsList.Count < expectedArgsQty)
                return false;

            if (expectedArgsQty > 0)
                foundArgs = foundArgsList.ToArray();

            return true;
        }

        internal static void Exit(int errorCode = 0)
        {
            Debug.Log($"Process finished with code {errorCode}");

            if (Application.isBatchMode)
                EditorApplication.Exit(errorCode);
        }

        public static void DeleteFile(string path)
        {
            try
            {
                File.Delete(path);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error trying to delete file {path}!\n{e.Message}");
            }
        }

        public static void DeleteDirectory(string path)
        {
            try
            {
                if (Directory.Exists(path))
                    Directory.Delete(path, true);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error trying to delete directory {path}!\n{e.Message}");
            }
        }


        internal static void MarkForAssetBundleBuild(string path, string abName)
        {
            string assetPath = path.Substring(path.IndexOf("Assets"));
            assetPath = Path.ChangeExtension(assetPath, null);

            assetPath = assetPath.Substring(0, assetPath.Length - 1);
            AssetImporter a = AssetImporter.GetAtPath(assetPath);
            a.SetAssetBundleNameAndVariant(abName, "");
        }



        internal static bool CheckProviderItemExists(DCL.ContentProvider contentProvider, string fileName)
        {
            string finalUrl = contentProvider.GetContentsUrl(fileName);
            return CheckUrlExists(finalUrl);
        }

        internal static bool CheckUrlExists(string url)
        {
            bool result = false;

            using (UnityWebRequest req = UnityWebRequest.Get(url))
            {
                req.SendWebRequest();

                while (req.downloadedBytes > 0) { }

                if (req.WebRequestSucceded())
                    result = true;

                req.Abort();
            }

            return result;
        }

        internal static Texture2D GetTextureFromAssetBundle(ApiEnvironment environment, string hash)
        {
            string url = ContentServerUtils.GetBundlesAPIUrlBase(environment) + hash;

            using (UnityWebRequest assetBundleRequest = UnityWebRequestAssetBundle.GetAssetBundle(url))
            {
                var asyncOp = assetBundleRequest.SendWebRequest();

                while (!asyncOp.isDone) { }

                if (assetBundleRequest.isHttpError || assetBundleRequest.isNetworkError)
                {
                    Debug.LogWarning("AssetBundle request fail! " + url);
                    return null;
                }

                AssetBundle assetBundle = DownloadHandlerAssetBundle.GetContent(assetBundleRequest);

                if (assetBundle != null)
                {
                    Texture2D[] txs = assetBundle.LoadAllAssets<Texture2D>();
                    return txs[0];
                }
            }

            return null;
        }

        public static MD5 md5 = new MD5CryptoServiceProvider();
        public static string GetGUID(string cid)
        {
            byte[] data = md5.ComputeHash(Encoding.UTF8.GetBytes(cid));
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }

        public static HashSet<string> GetSceneCids(ApiEnvironment environment, Vector2Int coords, Vector2Int size)
        {
            HashSet<string> sceneCids = new HashSet<string>();

            string url = GetScenesAPIUrl(environment, coords.x, coords.y, size.x, size.y);

            UnityWebRequest w = UnityWebRequest.Get(url);
            w.SendWebRequest();

            while (!w.isDone) { }

            if (!w.WebRequestSucceded())
            {
                throw new Exception($"Request error! Parcels couldn't be fetched! -- {w.error}");
            }

            ScenesAPIData scenesApiData = JsonUtility.FromJson<ScenesAPIData>(w.downloadHandler.text);

            Assert.IsTrue(scenesApiData != null, "Invalid response from ScenesAPI");
            Assert.IsTrue(scenesApiData.data != null, "Invalid response from ScenesAPI");

            foreach (var data in scenesApiData.data)
            {
                sceneCids.Add(data.root_cid);
            }

            return sceneCids;
        }

        public static HashSet<string> GetScenesCids(ApiEnvironment environment, List<Vector2Int> coords)
        {
            HashSet<string> sceneCids = new HashSet<string>();

            foreach (Vector2Int v in coords)
            {
                string url = GetScenesAPIUrl(environment, v.x, v.y, 0, 0);

                UnityWebRequest w = UnityWebRequest.Get(url);
                w.SendWebRequest();

                while (!w.isDone) { }

                if (!w.WebRequestSucceded())
                {
                    Debug.LogWarning($"Request error! Parcels couldn't be fetched! -- {w.error}");
                    continue;
                }

                ScenesAPIData scenesApiData = JsonUtility.FromJson<ScenesAPIData>(w.downloadHandler.text);

                Assert.IsTrue(scenesApiData != null, "Invalid response from ScenesAPI");
                Assert.IsTrue(scenesApiData.data != null, "Invalid response from ScenesAPI");

                foreach (var data in scenesApiData.data)
                {
                    sceneCids.Add(data.root_cid);
                }
            }

            return sceneCids;
        }

        public static MappingsAPIData GetSceneMappingsData(ApiEnvironment environment, string sceneCid)
        {
            string url = GetMappingsAPIUrl(environment, sceneCid);
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

            return parcelInfoApiData;
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

        public static void InitializeDirectory(string path, bool deleteIfExists)
        {
            try
            {
                if (deleteIfExists)
                {
                    if (Directory.Exists(path))
                        Directory.Delete(path, true);
                }


                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
            }
            catch (Exception e)
            {
                Debug.LogError($"Exception trying to clean up folder. Continuing anyways.\n{e.Message}");
            }
        }


    }
}
