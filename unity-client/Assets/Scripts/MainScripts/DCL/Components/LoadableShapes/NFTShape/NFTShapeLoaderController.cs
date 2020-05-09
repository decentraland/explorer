﻿using DCL.Components;
using DCL.Configuration;
using DCL.Helpers;
using DCL.Controllers.Gif;
using System;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;

public class NFTShapeLoaderController : MonoBehaviour
{
    [Serializable]
    public class NFTAssetData
    {
        [Serializable]
        public class File
        {
            public string url;
            public string role;
        }

        public File[] files;
    }

    public enum NoiseType
    {
        ClassicPerlin,
        PeriodicPerlin,
        Simplex,
        SimplexNumericalGrad,
        SimplexAnalyticalGrad
    }

    public MeshRenderer meshRenderer;
    public new BoxCollider collider;
    public Color backgroundColor;

    [HideInInspector] public bool alreadyLoadedAsset = false;

    public event System.Action OnLoadingAssetSuccess;
    public event System.Action OnLoadingAssetFail;

    [Header("Noise Shader")]
    [SerializeField] NoiseType noiseType = NoiseType.Simplex;
    [SerializeField] bool noiseIs3D = false;
    [SerializeField] bool noiseIsFractal = false;

    Material frameMaterial;
    System.Action<LoadWrapper> OnSuccess;
    System.Action<LoadWrapper> OnFail;
    string darURLProtocol;
    string darURLRegistry;
    string darURLAsset;
    MaterialPropertyBlock imageMaterialPropertyBlock;
    MaterialPropertyBlock backgroundMaterialPropertyBlock;

    int BASEMAP_SHADER_PROPERTY = Shader.PropertyToID("_BaseMap");
    int COLOR_SHADER_PROPERTY = Shader.PropertyToID("_BaseColor");

    INFTAsset nftAsset;

    bool VERBOSE = false;

    void Awake()
    {
        imageMaterialPropertyBlock = new MaterialPropertyBlock();
        backgroundMaterialPropertyBlock = new MaterialPropertyBlock();
        frameMaterial = meshRenderer.materials[2];

        InitializePerlinNoise();
    }

    public void LoadAsset(string url = "", bool loadEvenIfAlreadyLoaded = false)
    {
        if (string.IsNullOrEmpty(url) || (!loadEvenIfAlreadyLoaded && alreadyLoadedAsset)) return;

        UpdateBackgroundColor(backgroundColor);

        // Check the src follows the needed format e.g.: 'ethereum://0x06012c8cf97BEaD5deAe237070F9587f8E7A266d/558536'
        var regexMatches = Regex.Matches(url, "(?<protocol>[^:]+)://(?<registry>[^/]+)(?:/(?<asset>.+))?");
        if (regexMatches.Count == 0)
        {
            Debug.LogError($"Couldn't fetch DAR url '{url}' for NFTShape. The accepted format is 'ethereum://ContractAddress/TokenID'");

            OnLoadingAssetFail?.Invoke();

            return;
        }

        Match match = regexMatches[0];
        if (match.Groups["protocol"] == null || match.Groups["registry"] == null || match.Groups["asset"] == null)
        {
            Debug.LogError($"Couldn't fetch DAR url '{url}' for NFTShape.");

            OnLoadingAssetFail?.Invoke();

            return;
        }

        darURLProtocol = match.Groups["protocol"].ToString();
        if (darURLProtocol != "ethereum")
        {
            Debug.LogError($"Couldn't fetch DAR url '{url}' for NFTShape. The only protocol currently supported is 'ethereum'");

            OnLoadingAssetFail?.Invoke();

            return;
        }

        darURLRegistry = match.Groups["registry"].ToString();
        darURLAsset = match.Groups["asset"].ToString();

        if (VERBOSE)
        {
            Debug.Log("protocol: " + darURLProtocol);
            Debug.Log("registry: " + darURLRegistry);
            Debug.Log("asset: " + darURLAsset);
        }

        alreadyLoadedAsset = false;

        StartCoroutine(FetchNFTImage());
    }

    public void UpdateBackgroundColor(Color newColor)
    {
        meshRenderer.GetPropertyBlock(backgroundMaterialPropertyBlock, 1);
        backgroundMaterialPropertyBlock.SetColor(COLOR_SHADER_PROPERTY, newColor);
        meshRenderer.SetPropertyBlock(backgroundMaterialPropertyBlock, 1);
    }

    IEnumerator FetchNFTImage()
    {
        string jsonURL = $"{NFTDataFetchingSettings.DAR_API_URL}/{darURLRegistry}/asset/{darURLAsset}";

        UnityWebRequest www = UnityWebRequest.Get(jsonURL);
        yield return www.SendWebRequest();

        if (!www.WebRequestSucceded())
        {
            OnLoadingAssetFail?.Invoke();

            Debug.LogError($"{www.responseCode} - {www.url}", gameObject);

            yield break;
        }

        NFTAssetData currentAssetData = JsonUtility.FromJson<NFTAssetData>(www.downloadHandler.text);
        if (currentAssetData.files == null)
        {
            Debug.LogError($"Didn't find any asset image for '{jsonURL}' for the NFTShape.");

            yield break;
        }

        if (VERBOSE)
        {
            Debug.Log("NFT fetched JSON: " + www.downloadHandler.text);

            Debug.Log("NFT Assets Found: ");
            for (int i = 0; i < currentAssetData.files.Length; i++)
            {
                Debug.Log("file url: " + currentAssetData.files[i]?.url);
            }
        }

        string thumbnailImageURL = null;
        string dclImageURL = null;
        string previewImageURL = null;

        for (int i = 0; i < currentAssetData.files.Length; i++)
        {
            if (currentAssetData.files[i].role == "thumbnail")
                thumbnailImageURL = currentAssetData.files[i].url;
            else if (currentAssetData.files[i].role == "preview")
                previewImageURL = currentAssetData.files[i].url;
            else if (currentAssetData.files[i].role == "dcl-picture-frame-image")
                dclImageURL = currentAssetData.files[i].url;
        }

        // We fetch and show the thumbnail image first
        if (!string.IsNullOrEmpty(thumbnailImageURL))
        {
            yield return FetchNFTImageAsset(thumbnailImageURL, (downloadedAsset) =>
            {
                SetFrameImage(downloadedAsset);
            });
        }

        // We fetch the final image
        bool foundDCLImage = false;
        if (!string.IsNullOrEmpty(dclImageURL))
        {
            yield return FetchNFTImageAsset(dclImageURL, (downloadedAsset) =>
            {
                // Dispose thumbnail
                if (nftAsset != null) nftAsset.Dispose();
                foundDCLImage = true;
                SetFrameImage(downloadedAsset, resizeFrameMesh: true);
            });
        }

        // We fall back to a common "preview" image if no "dcl image" was found
        if (!foundDCLImage && !string.IsNullOrEmpty(previewImageURL))
        {
            yield return FetchNFTImageAsset(previewImageURL, (downloadedAsset) =>
            {
                // Dispose thumbnail
                if (nftAsset != null) nftAsset.Dispose();
                SetFrameImage(downloadedAsset, resizeFrameMesh: true);
            });
        }

        OnLoadingAssetSuccess?.Invoke();
    }

    void SetFrameImage(INFTAsset newAsset, bool resizeFrameMesh = false)
    {
        if (newAsset == null) return;

        nftAsset = newAsset;

        var gifAsset = nftAsset as NFTGifAsset;
        if (gifAsset != null)
        {
            gifAsset.SetUpdateTextureCallback(UpdateTexture);
        }

        UpdateTexture(nftAsset.texture);

        if (resizeFrameMesh)
        {
            Vector3 newScale = new Vector3(newAsset.width / NFTDataFetchingSettings.NORMALIZED_DIMENSIONS.x,
                                            newAsset.height / NFTDataFetchingSettings.NORMALIZED_DIMENSIONS.y, 1f);

            meshRenderer.transform.localScale = newScale;
        }
        else
        {
            meshRenderer.transform.localScale = Vector3.one;
        }
    }

    void UpdateTexture(Texture2D texture)
    {
        meshRenderer.GetPropertyBlock(imageMaterialPropertyBlock, 0);
        imageMaterialPropertyBlock.SetTexture(BASEMAP_SHADER_PROPERTY, texture);
        imageMaterialPropertyBlock.SetColor(COLOR_SHADER_PROPERTY, Color.white);
        meshRenderer.SetPropertyBlock(imageMaterialPropertyBlock, 0);
    }

    void InitializePerlinNoise()
    {
        frameMaterial.shaderKeywords = null;

        switch (noiseType)
        {
            case NoiseType.ClassicPerlin:
                frameMaterial.EnableKeyword("CNOISE");
                break;
            case NoiseType.PeriodicPerlin:
                frameMaterial.EnableKeyword("PNOISE");
                break;
            case NoiseType.Simplex:
                frameMaterial.EnableKeyword("SNOISE");
                break;
            case NoiseType.SimplexNumericalGrad:
                frameMaterial.EnableKeyword("SNOISE_NGRAD");
                break;
            default: // SimplexAnalyticalGrad
                frameMaterial.EnableKeyword("SNOISE_AGRAD");
                break;
        }

        if (noiseIs3D)
            frameMaterial.EnableKeyword("THREED");

        if (noiseIsFractal)
            frameMaterial.EnableKeyword("FRACTAL");
    }

    void OnDestroy()
    {
        if (nftAsset != null)
        {
            nftAsset.Dispose();
        }
    }

    static IEnumerator FetchNFTImageAsset(string url, Action<INFTAsset> OnSuccess)
    {
        string contentType = null;
        byte[] bytes = null;

        yield return Utils.FetchAsset(url, UnityWebRequest.Get(url), (request) =>
        {
            contentType = request.GetResponseHeader("Content-Type");
            bytes = request.downloadHandler.data;
        });

        if (contentType != null && bytes != null)
        {
            yield return InstantiateNFTAsset(contentType, bytes, OnSuccess);
        }
    }

    static IEnumerator InstantiateNFTAsset(string contentType, byte[] bytes, Action<INFTAsset> OnSuccess)
    {
        if (contentType == "image/gif")
        {
            var gif = new DCLGif();
            yield return gif.Load(bytes, () =>
            {
                gif.Play();
                OnSuccess?.Invoke(new NFTGifAsset(gif));
            });
        }
        else
        {
            var texture = new Texture2D(1, 1);
            texture.LoadImage(bytes);
            OnSuccess?.Invoke(new NFTImageAsset(texture));
        }
    }

    interface INFTAsset : IDisposable
    {
        Texture2D texture { get; }
        int width { get; }
        int height { get; }
    }

    class NFTImageAsset : INFTAsset
    {
        private Texture2D _texture;

        public Texture2D texture => _texture;
        public int width => _texture.width;
        public int height => _texture.height;

        public void Dispose()
        {
            if (_texture != null)
            {
                UnityEngine.Object.Destroy(_texture);
                _texture = null;
            }
        }

        public NFTImageAsset(Texture2D t)
        {
            _texture = t;
        }
    }

    class NFTGifAsset : INFTAsset
    {
        DCLGif _gif;
        Coroutine _updateRoutine = null;

        public Texture2D texture => _gif.texture;
        public int width => _gif.textureWidth;
        public int height => _gif.textureHeight;

        public void Dispose()
        {
            if (_updateRoutine != null)
            {
                CoroutineStarter.Stop(_updateRoutine);
            }
            if (_gif != null)
            {
                _gif.Dispose();
            }
        }

        public void SetUpdateTextureCallback(Action<Texture2D> callback)
        {
            _gif.OnFrameTextureChanged += callback;
            _updateRoutine = CoroutineStarter.Start(_gif.UpdateRoutine());
        }

        public NFTGifAsset(DCLGif gif)
        {
            _gif = gif;
        }
    }
}
