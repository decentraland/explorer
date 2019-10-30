using DCL;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using WaitUntil = DCL.WaitUntil;

public static class AssetBundleLoadHelper
{
    static bool VERBOSE = false;

    static Dictionary<string, AssetBundle> cachedBundles = new Dictionary<string, AssetBundle>();
    static Dictionary<string, AssetBundle> cachedBundlesWithDeps = new Dictionary<string, AssetBundle>();

    static Dictionary<string, List<string>> dependenciesMap = new Dictionary<string, List<string>>();
    static HashSet<string> failedRequests = new HashSet<string>();

    static List<string> downloadingBundle = new List<string>();
    static List<string> downloadingBundleWithDeps = new List<string>();

    static List<UnityEngine.Object> allLoadedAssets = new List<UnityEngine.Object>();
    static Dictionary<string, AssetBundleRequest> loadedAssets = new Dictionary<string, AssetBundleRequest>();

    [System.Serializable]
    public class AssetDependencyMap
    {
        public string[] dependencies;
    }

    public static IEnumerator GetDepMap(string hash)
    {
        string url = ContentServerUtils.GetBundlesAPIUrlBase(ContentServerUtils.ApiEnvironment.ZONE) + hash + ".depmap";

        if (failedRequests.Contains(url))
            yield break;

        if (dependenciesMap.ContainsKey(hash))
            yield break;

        if (downloadingBundle.Contains(url))
        {
            yield return new WaitUntil(() => !downloadingBundle.Contains(url));
            yield break;
        }

        UnityWebRequest depmapRequest = UnityWebRequest.Get(url);
        downloadingBundle.Add(url);
        yield return depmapRequest.SendWebRequest();

        if (depmapRequest.isHttpError || depmapRequest.isNetworkError)
        {
            failedRequests.Add(url);
            downloadingBundle.Remove(url);
            yield break;
        }

        AssetDependencyMap map = JsonUtility.FromJson<AssetDependencyMap>(depmapRequest.downloadHandler.text);

        dependenciesMap.Add(hash, new List<string>(map.dependencies));
        downloadingBundle.Remove(url);
    }

    public static IEnumerator GetAssetBundle(string url)
    {
        if (failedRequests.Contains(url) || cachedBundles.ContainsKey(url))
            yield break;

        UnityWebRequest assetBundleRequest = UnityWebRequestAssetBundle.GetAssetBundle(url);

        if (downloadingBundle.Contains(url))
        {
            yield return new WaitUntil(() => !downloadingBundle.Contains(url));
            yield break;
        }

        downloadingBundle.Add(url);

        yield return assetBundleRequest.SendWebRequest();

        if (assetBundleRequest.isHttpError || assetBundleRequest.isNetworkError)
        {
            failedRequests.Add(url);
            downloadingBundle.Remove(url);
            Debug.LogWarning("AssetBundle request fail! " + url);
            yield break;
        }

        if (!cachedBundles.ContainsKey(url))
        {
            AssetBundle assetBundle = DownloadHandlerAssetBundle.GetContent(assetBundleRequest);
            string[] assets = assetBundle.GetAllAssetNames();
            List<string> assetsToLoad = new List<string>();

            if (assetBundle != null)
            {
                foreach (string asset in assets)
                {
                    bool isTexture = asset.EndsWith("jpg") || asset.EndsWith("png");

                    if (isTexture)
                    {
                        assetsToLoad.Add(asset);
                    }
                }

                foreach (string asset in assets)
                {
                    bool isMaterial = asset.EndsWith("mat");

                    if (isMaterial)
                    {
                        assetsToLoad.Add(asset);
                    }
                }

                foreach (string asset in assets)
                {
                    bool isModel = asset.EndsWith("glb") || asset.EndsWith("gltf");
                    if (isModel)
                    {
                        assetsToLoad.Add(asset);
                    }
                }

                foreach (string asset in assets)
                {
                    assetsToLoad.Add(asset);
                }

                foreach (var asset in assetsToLoad)
                {
                    if (!loadedAssets.ContainsKey(asset))
                    {
                        var request = assetBundle.LoadAssetAsync(asset);
                        yield return request;
                        loadedAssets.Add(asset, request);
                    }
                }

                cachedBundles[url] = assetBundle;
                yield return null;
            }
            else
            {
                failedRequests.Add(url);
            }
        }

        assetBundleRequest.Dispose();
        downloadingBundle.Remove(url);
    }


    public static IEnumerator FetchAssetBundleWithDependencies(string hash, System.Action<GameObject> OnComplete = null, bool instantiate = true, bool verbose = true)
    {
        string url = ContentServerUtils.GetBundlesAPIUrlBase(ContentServerUtils.ApiEnvironment.ZONE) + hash;

        AssetBundle mainAssetBundle;

        if (downloadingBundleWithDeps.Contains(url))
        {
            yield return new WaitUntil(() => !downloadingBundleWithDeps.Contains(url));
        }

        if (cachedBundlesWithDeps.ContainsKey(url))
        {
            if (instantiate)
                yield return InstantiateAssetBundle(cachedBundlesWithDeps[url], OnComplete);

            yield break;
        }

        downloadingBundleWithDeps.Add(url);

        yield return GetDepMap(hash);

        if (dependenciesMap.ContainsKey(hash))
        {
            foreach (string dep in dependenciesMap[hash])
            {
                yield return FetchAssetBundleWithDependencies(dep, null, false, false);
            }
        }

        if (!cachedBundles.ContainsKey(url))
            yield return GetAssetBundle(url);

        if (!cachedBundles.TryGetValue(url, out mainAssetBundle))
        {
            downloadingBundleWithDeps.Remove(url);
            OnComplete?.Invoke(null);
            yield break;
        }

        cachedBundlesWithDeps.Add(url, mainAssetBundle);
        downloadingBundleWithDeps.Remove(url);

        if (instantiate)
            yield return InstantiateAssetBundle(mainAssetBundle, OnComplete);
    }

    public static IEnumerator InstantiateAssetBundle(AssetBundle bundle, System.Action<GameObject> OnComplete)
    {
        string[] assetNames = bundle.GetAllAssetNames();
        string targetAsset = null;

        for (int i = 0; i < assetNames.Length; i++)
        {
            string asset = assetNames[i];

            if (asset.Contains("glb") || asset.Contains("gltf"))
            {
                targetAsset = asset;
                break;
            }
        }

        if (string.IsNullOrEmpty(targetAsset))
        {
            Debug.Log("target asset not found?");
            yield break;
        }

        yield return loadedAssets[targetAsset];

        GameObject container = Object.Instantiate(loadedAssets[targetAsset].asset as GameObject);
        container.name = targetAsset;
#if UNITY_EDITOR
        container.GetComponentsInChildren<Renderer>().ToList().ForEach(ResetShader);
#endif
        OnComplete?.Invoke(container);
        yield break;
    }

#if UNITY_EDITOR
    private static void ResetShader(Renderer renderer)
    {
        if (renderer.material == null) return;

        for (int i = 0; i < renderer.materials.Length; i++)
        {
            renderer.materials[i].shader = Shader.Find(renderer.materials[i].shader.name);
        }
    }
#endif
}
