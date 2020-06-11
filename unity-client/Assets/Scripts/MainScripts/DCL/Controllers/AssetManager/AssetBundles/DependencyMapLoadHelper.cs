using System;
using DCL.Helpers;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using WaitUntil = DCL.WaitUntil;


public static class DependencyMapLoadHelper
{
    static bool VERBOSE = false;

    private const string PERSISTENT_CACHE_KEY = "DepMapCache";
    private static bool persistentCacheLoaded = false;

    public static Dictionary<string, List<string>> dependenciesMap = new Dictionary<string, List<string>>();

    static HashSet<string> failedRequests = new HashSet<string>();
    static HashSet<string> downloadingDepmap = new HashSet<string>();

    [System.Serializable]
    public class AssetDependencyMap
    {
        public string[] dependencies;
    }

    public static IEnumerator WaitUntilDepMapIsResolved(string hash)
    {
        yield return new WaitUntil(() => !downloadingDepmap.Contains(hash));
        yield return new WaitUntil(() => dependenciesMap.ContainsKey(hash) || failedRequests.Contains(hash));
    }

    public static IEnumerator GetDepMap(string baseUrl, string hash)
    {
        string url = baseUrl + hash + ".depmap";

        LoadPersistentCache();

        if (failedRequests.Contains(hash))
            yield break;

        if (dependenciesMap.ContainsKey(hash))
            yield break;

        if (downloadingDepmap.Contains(hash))
        {
            yield return WaitUntilDepMapIsResolved(hash);
            yield break;
        }

        using (UnityWebRequest depmapRequest = UnityWebRequest.Get(url))
        {
            downloadingDepmap.Add(hash);

            yield return depmapRequest.SendWebRequest();

            if (!depmapRequest.WebRequestSucceded())
            {
                failedRequests.Add(hash);
                downloadingDepmap.Remove(hash);
                yield break;
            }

            AssetDependencyMap map = JsonUtility.FromJson<AssetDependencyMap>(depmapRequest.downloadHandler.text);

            dependenciesMap.Add(hash, new List<string>(map.dependencies));

            SavePersistentCache();

            downloadingDepmap.Remove(hash);
        }
    }

    private static void SavePersistentCache()
    {
        //NOTE(Brian): Use JsonConvert because unity JsonUtility doesn't support dictionaries
        string cacheJson = JsonConvert.SerializeObject(dependenciesMap);
        PlayerPrefs.SetString(PERSISTENT_CACHE_KEY, cacheJson);
    }

    private static void LoadPersistentCache()
    {
        if (!persistentCacheLoaded)
        {
            persistentCacheLoaded = true;

            string depMapCache = PlayerPrefs.GetString(PERSISTENT_CACHE_KEY, String.Empty);

            if (!string.IsNullOrEmpty(depMapCache))
            {
                dependenciesMap = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(depMapCache);
            }
        }
    }
}