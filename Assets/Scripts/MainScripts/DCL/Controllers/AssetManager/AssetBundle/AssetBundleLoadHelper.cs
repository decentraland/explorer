using DCL;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using WaitUntil = DCL.WaitUntil;

public static class AssetBundleLoadHelper
{
    static Dictionary<string, AssetBundle> cachedBundles = new Dictionary<string, AssetBundle>();
    static Dictionary<string, AssetBundle> cachedBundlesWithDeps = new Dictionary<string, AssetBundle>();

    static Dictionary<string, List<string>> dependenciesMap = new Dictionary<string, List<string>>();
    static HashSet<string> processedManifests = new HashSet<string>();
    static HashSet<string> failedRequests = new HashSet<string>();


    static List<string> downloadingBundle = new List<string>();
    static List<string> downloadingBundleWithDeps = new List<string>();

    static List<UnityEngine.Object> allLoadedAssets = new List<UnityEngine.Object>();

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

            if (assetBundle != null)
            {
                UnityEngine.Object[] loadedAssets;
                var req = assetBundle.LoadAllAssetsAsync();
                yield return req;
                loadedAssets = req.allAssets;
                cachedBundles[url] = assetBundle;
                allLoadedAssets.AddRange(loadedAssets); //NOTE(Brian): Done to prevent Resources.UnloadUnusedAssets to strip them before they are used.
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

        string url = $"http://localhost:1338/manifests/{sceneCid}";

        yield return GetAssetBundle(url);

        AssetBundle mainAssetBundle;

        if (cachedBundles.TryGetValue(url, out mainAssetBundle))
        {
            AssetBundleManifest manifest = mainAssetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            processedManifests.Add(sceneCid);

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
    }


    public static IEnumerator FetchAssetBundleWithDependencies(string hash, System.Action<GameObject> OnComplete = null)
    {
        string url = $"http://localhost:1338/{hash}";

        AssetBundle mainAssetBundle;

        if (downloadingBundleWithDeps.Contains(url))
        {
            yield return new WaitUntil(() => !downloadingBundleWithDeps.Contains(url));
        }

        if (cachedBundlesWithDeps.ContainsKey(url))
        {
            yield return InstantiateAssetBundle(cachedBundlesWithDeps[url], OnComplete);
            yield break;
        }

        downloadingBundleWithDeps.Add(url);

        if (!cachedBundles.ContainsKey(url))
            yield return GetAssetBundle(url);

        if (!cachedBundles.TryGetValue(url, out mainAssetBundle))
        {
            downloadingBundleWithDeps.Remove(url);
            OnComplete?.Invoke(null);
            yield break;
        }

        if (dependenciesMap.ContainsKey(hash))
        {
            foreach (string dep in dependenciesMap[hash])
            {
                yield return FetchAssetBundleWithDependencies(dep);
            }
        }

        cachedBundlesWithDeps.Add(url, mainAssetBundle);
        downloadingBundleWithDeps.Remove(url);
        yield return InstantiateAssetBundle(mainAssetBundle, OnComplete);
    }

    public static IEnumerator InstantiateAssetBundle(AssetBundle bundle, System.Action<GameObject> OnComplete)
    {
        string[] assetNames = bundle.GetAllAssetNames();

        for (int i = 0; i < assetNames.Length; i++)
        {
            string asset = assetNames[i];

            if (asset.Contains("glb") || asset.Contains("gltf"))
            {
                var req = bundle.LoadAssetAsync<GameObject>(asset);
                yield return req;
                GameObject model = req.asset as GameObject;
                GameObject container = Object.Instantiate(model);
                container.name = asset;
#if UNITY_EDITOR
                container.GetComponentsInChildren<Renderer>().ToList().ForEach(ResetShader);
#endif
                OnComplete?.Invoke(container);
                yield break;
            }
        }
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
