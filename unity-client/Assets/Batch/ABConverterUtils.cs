using DCL.Helpers;
using GLTF.Schema;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Networking;
using static DCL.ContentServerUtils;

namespace DCL
{
    public abstract partial class ABConverter
    {
        public static class Utils
        {
            internal static bool ParseOption(string[] fullCmdArgs, string optionName, int argsQty, out string[] foundArgs)
            {
                return ParseOptionExplicit(fullCmdArgs, optionName, argsQty, out foundArgs);
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
                AssetImporter importer = AssetImporter.GetAtPath(assetPath);
                importer.SetAssetBundleNameAndVariant(abName, "");
            }

            internal static void MarkForAssetBundleBuild(IAssetDatabase assetDb, UnityEngine.Object asset, string abName)
            {
                string assetPath = assetDb.GetAssetPath(asset);
                var importer = AssetImporter.GetAtPath(assetPath);
                importer.SetAssetBundleNameAndVariant(abName, "");
            }

            public static MD5 md5 = new MD5CryptoServiceProvider();

            public static string CidToGuid(string cid)
            {
                byte[] data = md5.ComputeHash(Encoding.UTF8.GetBytes(cid));
                StringBuilder sBuilder = new StringBuilder();

                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }

                return sBuilder.ToString();
            }

            public static HashSet<string> GetSceneCids(IWebRequest webRequest, ApiTLD tld, Vector2Int coords, Vector2Int size)
            {
                HashSet<string> sceneCids = new HashSet<string>();

                string url = GetScenesAPIUrl(tld, coords.x, coords.y, size.x, size.y);

                DownloadHandler downloadHandler = null;

                try
                {
                    downloadHandler = webRequest.Get(url);
                }
                catch (HttpRequestException e)
                {
                    throw new Exception($"Request error! Parcels couldn't be fetched! -- {e.Message}", e);
                }

                ScenesAPIData scenesApiData = JsonUtility.FromJson<ScenesAPIData>(downloadHandler.text);

                Assert.IsTrue(scenesApiData != null, "Invalid response from ScenesAPI");
                Assert.IsTrue(scenesApiData.data != null, "Invalid response from ScenesAPI");

                foreach (var data in scenesApiData.data)
                {
                    sceneCids.Add(data.root_cid);
                }

                return sceneCids;
            }

            public static HashSet<string> GetScenesCids(IWebRequest webRequest, ApiTLD tld, List<Vector2Int> coords)
            {
                HashSet<string> sceneCids = new HashSet<string>();

                foreach (Vector2Int v in coords)
                {
                    string url = GetScenesAPIUrl(tld, v.x, v.y, 0, 0);

                    DownloadHandler downloadHandler = null;

                    try
                    {
                        downloadHandler = webRequest.Get(url);
                    }
                    catch (HttpRequestException e)
                    {
                        throw new HttpRequestException($"Request error! Parcels couldn't be fetched! -- {url} -- {e.Message}", e);
                    }

                    ScenesAPIData scenesApiData = JsonUtility.FromJson<ScenesAPIData>(downloadHandler.text);

                    Assert.IsTrue(scenesApiData != null, "Invalid response from ScenesAPI");
                    Assert.IsTrue(scenesApiData.data != null, "Invalid response from ScenesAPI");

                    foreach (var data in scenesApiData.data)
                    {
                        sceneCids.Add(data.root_cid);
                    }
                }

                return sceneCids;
            }

            public static MappingsAPIData GetSceneMappingsData(IWebRequest webRequest, ApiTLD tld, string sceneCid)
            {
                string url = GetMappingsAPIUrl(tld, sceneCid);

                DownloadHandler downloadHandler = null;

                try
                {
                    downloadHandler = webRequest.Get(url);
                }
                catch (HttpRequestException e)
                {
                    throw new Exception($"Request error! mappings couldn't be fetched for scene {sceneCid}! -- {e.Message}");
                }

                MappingsAPIData parcelInfoApiData = JsonUtility.FromJson<MappingsAPIData>(downloadHandler.text);

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
            public static List<AssetPath> GetPathsFromPairs(string basePath, MappingPair[] pairsToSearch, string[] extensions)
            {
                var tmpResult = new Dictionary<(string, string), AssetPath>();

                for (int i = 0; i < pairsToSearch.Length; i++)
                {
                    MappingPair mappingPair = pairsToSearch[i];

                    bool hasExtension = extensions.Any((x) => mappingPair.file.ToLower().EndsWith(x));

                    if (hasExtension)
                    {
                        if (!tmpResult.ContainsKey((mappingPair.hash, mappingPair.file)))
                            tmpResult.Add((mappingPair.hash, mappingPair.file), new AssetPath(basePath, mappingPair));
                    }
                }

                return tmpResult.Values.ToList();
            }

            public static void FixGltfRootInvalidUriCharacters(GLTFRoot gltfRoot)
            {
                if (gltfRoot == null)
                {
                    Debug.LogError("FixGltfRootInvalidUriCharacters >>> gltfRoot is null!");
                    return;
                }

                GLTFRoot root = gltfRoot;

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

                string result = FixDirectorySeparator(relativePath);

                return result;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="fullPath"></param>
            /// <returns></returns>
            public static string FullPathToAssetPath(string fullPath)
            {
                char ps = Path.DirectorySeparatorChar;

                fullPath = fullPath.Replace('/', ps);
                fullPath = fullPath.Replace('\\', ps);

                string pattern = $".*?\\{ps}(?<assetpath>Assets\\{ps}.*?$)";

                var regex = new Regex(pattern);

                var match = regex.Match(fullPath);

                if (match.Success && match.Groups["assetpath"] != null)
                    return match.Groups["assetpath"].Value;

                return string.Empty;
            }

            public static string FixDirectorySeparator(string path)
            {
                char ps = Path.DirectorySeparatorChar;
                path = path.Replace('/', ps);
                path = path.Replace('\\', ps);
                return path;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="assetPath"></param>
            /// <returns></returns>
            public static string AssetPathToFullPath(string assetPath, string overrideDataPath = null)
            {
                assetPath = FixDirectorySeparator(assetPath);

                string dataPath = overrideDataPath ?? Application.dataPath;
                dataPath = FixDirectorySeparator(dataPath);

                char ps = Path.DirectorySeparatorChar;
                string dataPathWithoutAssets = dataPath.Replace($"{ps}Assets", "");
                return dataPathWithoutAssets + "/" + assetPath;
            }


            public static void CleanAssetBundleFolder(IFile file, string pathToSearch, string[] assetBundlesList, Dictionary<string, string> lowerToUpperDictionary)
            {
                for (int i = 0; i < assetBundlesList.Length; i++)
                {
                    if (string.IsNullOrEmpty(assetBundlesList[i]))
                        continue;

                    try
                    {
                        //NOTE(Brian): This is done for correctness sake, rename files to preserve the hash upper-case
                        if (lowerToUpperDictionary.TryGetValue(assetBundlesList[i], out string hashWithUppercase))
                        {
                            string oldPath = pathToSearch + assetBundlesList[i];
                            string path = pathToSearch + hashWithUppercase;
                            file.Move(oldPath, path);
                        }

                        string oldPathMf = pathToSearch + assetBundlesList[i] + ".manifest";
                        file.Delete(oldPathMf);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning("Error! " + e.Message);
                    }
                }
            }
        }
    }
}