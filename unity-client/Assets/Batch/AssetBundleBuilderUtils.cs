using DCL.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

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

        internal static void MarkForAssetBundleBuild(string path, string abName)
        {
            string assetPath = path.Substring(path.IndexOf("Assets"));
            assetPath = Path.ChangeExtension(assetPath, null);

            assetPath = assetPath.Substring(0, assetPath.Length - 1);
            AssetImporter a = AssetImporter.GetAtPath(assetPath);
            a.SetAssetBundleNameAndVariant(abName, "");
        }


        /// <summary>
        /// This dumps .depmap files
        /// </summary>
        /// <param name="manifest"></param>
        internal static void GenerateDependencyMaps(Dictionary<string, string> hashLowercaseToHashProper, AssetBundleManifest manifest)
        {
            string[] assetBundles = manifest.GetAllAssetBundles();

            for (int i = 0; i < assetBundles.Length; i++)
            {
                if (string.IsNullOrEmpty(assetBundles[i]))
                    continue;

                var depMap = new AssetDependencyMap();
                string[] deps = manifest.GetAllDependencies(assetBundles[i]);

                if (deps.Length > 0)
                {
                    depMap.dependencies = deps.Select((x) =>
                    {
                        if (hashLowercaseToHashProper.ContainsKey(x))
                            return hashLowercaseToHashProper[x];
                        else
                            return x;

                    }).ToArray();
                }

                string json = JsonUtility.ToJson(depMap);
                string finalFilename = assetBundles[i];

                hashLowercaseToHashProper.TryGetValue(assetBundles[i], out finalFilename);
                File.WriteAllText(AssetBundleBuilder.finalAssetBundlePath + finalFilename + ".depmap", json);
            }
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

        internal static Texture2D GetTextureFromAssetBundle(string hash)
        {
            string url = ContentServerUtils.GetBundlesAPIUrlBase(AssetBundleBuilder.environment) + hash;

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


    }
}
