using DCL;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityGLTF.Cache;
using WaitUntil = DCL.WaitUntil;

public static class MaterialCachingHelper
{
    public static void UseCachedMaterials(GameObject obj)
    {
        foreach (var rend in obj.GetComponentsInChildren<Renderer>(true))
        {
            var matList = new List<Material>(1);

            foreach (var mat in rend.sharedMaterials)
            {
                string crc = mat.ComputeCRC() + mat.name;

                RefCountedMaterialData refCountedMat;

                if (!PersistentAssetCache.MaterialCacheByCRC.ContainsKey(crc))
                {
                    mat.enableInstancing = true;
                    PersistentAssetCache.MaterialCacheByCRC.Add(crc, new RefCountedMaterialData(crc, mat));
                }

                refCountedMat = PersistentAssetCache.MaterialCacheByCRC[crc];
                refCountedMat.IncreaseRefCount();

                matList.Add(refCountedMat.material);
            }

            rend.sharedMaterials = matList.ToArray();
        }
    }
}

public static class AssetBundleLoadHelper
{
    static bool VERBOSE = true;

    static Dictionary<string, AssetBundle> cachedBundles = new Dictionary<string, AssetBundle>();
    static Dictionary<string, AssetBundle> cachedBundlesWithDeps = new Dictionary<string, AssetBundle>();
    static Dictionary<AssetBundle, List<string>> bundleToMainAssets = new Dictionary<AssetBundle, List<string>>();

    static Dictionary<string, List<string>> dependenciesMap = new Dictionary<string, List<string>>();
    static HashSet<string> failedRequests = new HashSet<string>();

    static List<string> downloadingBundle = new List<string>();
    static List<string> downloadingBundleWithDeps = new List<string>();

    static Dictionary<string, Object> loadedAssets = new Dictionary<string, Object>();

    static Dictionary<string, int> loadOrderByExtension = new Dictionary<string, int>()
    {
        { "png", 0 },
        { "jpg", 1 },
        { "peg", 2 },
        { "bmp", 3 },
        { "psd", 4 },
        { "iff", 5 },
        { "mat", 6 },
        { "ltf", 7 },
        { "glb", 8 }
    };

    static float maxLoadBudgetTime = 0.032f;
    static float currentLoadBudgetTime = 0;

    [System.Serializable]
    public class AssetDependencyMap
    {
        public string[] dependencies;
    }

    public static IEnumerator GetDepMap(string baseUrl, string hash)
    {
        string url = baseUrl + hash + ".depmap";

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

        using (UnityWebRequest assetBundleRequest = UnityWebRequestAssetBundle.GetAssetBundle(url))
        {
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
                    string[] assets = assetBundle.GetAllAssetNames();
                    List<string> assetsToLoad = new List<string>();
                    assetsToLoad = assets.OrderBy(
                        (x) =>
                        {
                            string ext = x.Substring(x.Length - 3);

                            if (loadOrderByExtension.ContainsKey(ext))
                                return loadOrderByExtension[ext];
                            else
                                return 99;
                        }).ToList();

                    foreach (string asset in assetsToLoad)
                    {
                        if (!loadedAssets.ContainsKey(asset))
                        {
                            float time = Time.realtimeSinceStartup;

#if UNITY_EDITOR
                            if (VERBOSE)
                                Debug.Log("loading asset = " + asset);
#endif

                            loadedAssets.Add(asset, assetBundle.LoadAsset(asset));
#if UNITY_EDITOR
                            if (VERBOSE)
                            {
                                if (asset.EndsWith("mat"))
                                {
                                    Texture tex = (loadedAssets[asset] as Material).GetTexture("_BaseMap");

                                    if (tex != null)
                                        Debug.Log("material has texture " + tex.name);
                                    else
                                        Debug.Log("no texture!!!");
                                }
                            }
#endif
                            if (asset.EndsWith("glb") || asset.EndsWith("gltf"))
                            {
                                if (!bundleToMainAssets.ContainsKey(assetBundle))
                                    bundleToMainAssets.Add(assetBundle, new List<string>(1));

                                bundleToMainAssets[assetBundle].Add(asset);
                            }

                            if (RenderingController.i.renderingEnabled)
                            {
                                currentLoadBudgetTime += Time.realtimeSinceStartup - time;

                                if (currentLoadBudgetTime > maxLoadBudgetTime)
                                {
                                    currentLoadBudgetTime = 0;
                                    yield return null;
                                }
                            }
                        }
                    }

                    cachedBundles[url] = assetBundle;
                }
                else
                {
                    failedRequests.Add(url);
                }
            }

            downloadingBundle.Remove(url);
        }
    }


    public static IEnumerator FetchAssetBundleWithDependencies(string baseUrl, string hash, System.Action<GameObject> OnComplete = null, bool instantiate = true, bool verbose = true)
    {
        string url = baseUrl + hash;

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

        yield return GetDepMap(baseUrl, hash);

        if (dependenciesMap.ContainsKey(hash))
        {
            foreach (string dep in dependenciesMap[hash])
            {
                yield return FetchAssetBundleWithDependencies(baseUrl, dep, null, false, false);
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
        if (!bundleToMainAssets.ContainsKey(bundle))
        {
            Debug.Log("target asset not found?");
            yield break;
        }

        string targetAsset = bundleToMainAssets[bundle][0];

        if (!loadedAssets.ContainsKey(targetAsset))
        {
            Debug.Log("target asset not loaded?");
            yield break;
        }

        GameObject container = Object.Instantiate(loadedAssets[targetAsset] as GameObject);

        MaterialCachingHelper.UseCachedMaterials(container);

        container.name = targetAsset;
#if UNITY_EDITOR
        container.GetComponentsInChildren<Renderer>().ToList().ForEach(ResetShader);
#endif
        container.transform.position = Vector3.one * 5000;
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
