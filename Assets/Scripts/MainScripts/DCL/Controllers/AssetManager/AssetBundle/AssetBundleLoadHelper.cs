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
    static readonly string ASSET_BUNDLE_SERVER_URL = "https://content-as-bundle.decentraland.org/contents/";
    static readonly string ASSET_BUNDLE_SERVER_URL_LOCAL = "http://localhost:1338/";

    static Dictionary<string, AssetBundle> cachedBundles = new Dictionary<string, AssetBundle>();
    static Dictionary<string, AssetBundle> cachedBundlesWithDeps = new Dictionary<string, AssetBundle>();

    static Dictionary<string, List<string>> dependenciesMap = new Dictionary<string, List<string>>();
    static HashSet<string> processedManifests = new HashSet<string>();
    static HashSet<string> failedRequests = new HashSet<string>();

    static List<string> downloadingBundle = new List<string>();
    static List<string> downloadingBundleWithDeps = new List<string>();
    static bool downloadingBundleManifests = false;

    static List<UnityEngine.Object> allLoadedAssets = new List<UnityEngine.Object>();
    static Dictionary<string, Object> loadedAssets = new Dictionary<string, Object>();

    public static bool HasManifest(string sceneId)
    {
        return processedManifests.Contains(sceneId);
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

            if (assetBundle != null)
            {
                foreach (string asset in assets)
                {
                    bool isTexture = asset.EndsWith("jpg") || asset.EndsWith("png");

                    if (!loadedAssets.ContainsKey(asset) && isTexture)
                    {
                        loadedAssets.Add(asset, assetBundle.LoadAsset(asset));
                    }
                }

                foreach (string asset in assets)
                {
                    bool isMaterial = asset.EndsWith("mat");

                    if (!loadedAssets.ContainsKey(asset) && isMaterial)
                    {
                        loadedAssets.Add(asset, assetBundle.LoadAsset(asset));
                    }
                }

                foreach (string asset in assets)
                {
                    bool isModel = asset.EndsWith("glb") || asset.EndsWith("gltf");
                    if (!loadedAssets.ContainsKey(asset) && isModel)
                    {
                        loadedAssets.Add(asset, assetBundle.LoadAsset(asset));
                    }
                }

                foreach (string asset in assets)
                {
                    if (!loadedAssets.ContainsKey(asset))
                    {
                        loadedAssets.Add(asset, assetBundle.LoadAsset(asset));
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

    public static IEnumerator FetchManifest(ContentProvider contentProvider, string hash, string sceneCid)
    {
        if (processedManifests.Contains(sceneCid) || dependenciesMap.ContainsKey(hash))
            yield break;

        string url = ASSET_BUNDLE_SERVER_URL + "manifests/" + hash;

        if (downloadingBundleManifests)
        {
            yield return new WaitUntil(() => !downloadingBundleManifests);
            yield break;
        }

        downloadingBundleManifests = true;

        yield return GetAssetBundle(url);

        AssetBundle mainAssetBundle;

        if (cachedBundles.TryGetValue(url, out mainAssetBundle))
        {
            AssetBundleManifest manifest = mainAssetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            processedManifests.Add(sceneCid);

            if (VERBOSE)
                Debug.Log("loading manifest: " + sceneCid);

            List<string> dependencies = new List<string>();

            foreach (string asset in manifest.GetAllAssetBundles())
            {
                string[] deps = manifest.GetAllDependencies(asset);
                dependencies = new List<string>();

                foreach (string dep in deps)
                {
                    if (dep == asset)
                        continue;

                    var matchingPair = contentProvider.contents.FirstOrDefault((pair) => pair.hash.ToLowerInvariant() == dep.ToLowerInvariant());

                    if (matchingPair == null)
                    {
                        Debug.Log($"matchingPair not found? {dep}\n{contentProvider.ToString()}");
                        continue;
                    }

                    string depHash = matchingPair.hash;

                    if (depHash != null)
                    {
                        dependencies.Add(depHash);
                    }
                }

                var matchingPair2 = contentProvider.contents.FirstOrDefault((pair) => pair.hash.ToLowerInvariant() == asset.ToLowerInvariant());

                if (matchingPair2 == null)
                {
                    Debug.Log($"matchingPair2 not found? {asset}\n{contentProvider.ToString()}");
                    continue;
                }

                if (!dependenciesMap.ContainsKey(matchingPair2.hash))
                {
                    dependenciesMap.Add(matchingPair2.hash, dependencies);
                }
            }


            mainAssetBundle.Unload(true);

            cachedBundles.Remove(url);
        }

        downloadingBundleManifests = false;
    }


    public static IEnumerator FetchAssetBundleWithDependencies(string hash, System.Action<GameObject> OnComplete = null, bool instantiate = true, bool verbose = true)
    {
        string url = ASSET_BUNDLE_SERVER_URL + hash;

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

        GameObject container = Object.Instantiate(loadedAssets[targetAsset] as GameObject);
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
