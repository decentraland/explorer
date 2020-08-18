﻿using DCL.Components;
using DCL.Configuration;
using DCL.Helpers;
using DCL.Helpers.NFT;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;
using System;
using System.Runtime.InteropServices;
using DCL;
using DCL.Controllers.Gif;

public class NFTShapeLoaderController : MonoBehaviour
{
    public enum NoiseType
    {
        ClassicPerlin,
        PeriodicPerlin,
        Simplex,
        SimplexNumericalGrad,
        SimplexAnalyticalGrad,
        None
    }

    public MeshRenderer meshRenderer;
    public new BoxCollider collider;
    public Color backgroundColor;
    public GameObject spinner;

    [HideInInspector] public bool alreadyLoadedAsset = false;

    public event System.Action OnLoadingAssetSuccess;
    public event System.Action OnLoadingAssetFail;

    [Header("Material Indexes")] [SerializeField]
    int materialIndex_Background = -1;

    [SerializeField] int materialIndex_NFTImage = -1;
    [SerializeField] int materialIndex_Frame = -1;

    [Header("Noise Shader")] [SerializeField]
    NoiseType noiseType = NoiseType.Simplex;

    [SerializeField] bool noiseIs3D = false;
    [SerializeField] bool noiseIsFractal = false;

    System.Action<LoadWrapper> OnSuccess;
    System.Action<LoadWrapper> OnFail;
    string sceneId;
    string componentId;
    string darURLProtocol;
    string darURLRegistry;
    string darURLAsset;

    Material frameMaterial = null;
    Material imageMaterial = null;
    Material backgroundMaterial = null;

    int BASEMAP_SHADER_PROPERTY = Shader.PropertyToID("_BaseMap");
    int COLOR_SHADER_PROPERTY = Shader.PropertyToID("_BaseColor");

    DCL.ITexture nftAsset;

    bool VERBOSE = false;

    void Awake()
    {
        if (materialIndex_NFTImage >= 0) imageMaterial = meshRenderer.materials[materialIndex_NFTImage];
        if (materialIndex_Background >= 0) backgroundMaterial = meshRenderer.materials[materialIndex_Background];
        if (materialIndex_Frame >= 0) frameMaterial = meshRenderer.materials[materialIndex_Frame];

        // NOTE: we use half scale to keep backward compatibility cause we are using 512px to normalize the scale with a 256px value that comes from the images
        meshRenderer.transform.localScale = new Vector3(0.5f, 0.5f, 1);

        InitializePerlinNoise();
    }

    public void LoadAsset(string url, string sceneId, string componentId, bool loadEvenIfAlreadyLoaded = false)
    {
        if (string.IsNullOrEmpty(url) || (!loadEvenIfAlreadyLoaded && alreadyLoadedAsset)) return;

        this.sceneId = sceneId;
        this.componentId = componentId;

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
        if (backgroundMaterial == null)
            return;

        backgroundMaterial.SetColor(COLOR_SHADER_PROPERTY, newColor);
    }

    IEnumerator FetchNFTImage()
    {
        Debug.Log($"pravs - NFTShapeLoaderController.FetchNFTImage()");

        if (spinner != null)
            spinner.SetActive(true);

        string thumbnailImageURL = null;
        string previewImageURL = null;
        string originalImageURL = null;

        yield return NFTHelper.FetchNFTInfo(darURLRegistry, darURLAsset,
            (nftInfo) =>
            {
                thumbnailImageURL = nftInfo.thumbnailUrl;
                previewImageURL = nftInfo.previewImageUrl;
                originalImageURL = nftInfo.originalImageUrl;
            },
            (error) =>
            {
                Debug.LogError($"Didn't find any asset image for '{darURLRegistry}/{darURLAsset}' for the NFTShape.\n{error}");
                OnLoadingAssetFail?.Invoke();
            });

        yield return new DCL.WaitUntil(() => (CommonScriptableObjects.playerUnityPosition - transform.position).sqrMagnitude < 900f);

        // We the "preview" 256px image
        bool foundDCLImage = false;
        if (!string.IsNullOrEmpty(previewImageURL))
        {
            // IEnumerator fetchRoutine;
            // Debug.Log($"pravs - NFTShapeLoaderController.FetchNFTImage() - Will download PREVIEW IMAGE: " + previewImageURL);

            // yield return fetchRoutine = WrappedTextureUtils.Fetch(previewImageURL, downloadedAsset =>
            yield return WrappedTextureUtils.Fetch(previewImageURL, downloadedAsset =>
            {
                // Debug.Log($"pravs - NFTShapeLoaderController.FetchNFTImage() - downloaded still picture: {downloadedAsset.width}x{downloadedAsset.height}");
                foundDCLImage = true;
                // SetFrameImage(downloadedAsset, resizeFrameMesh: true); // TODO: resixing is not working correctly
                SetFrameImage(downloadedAsset, resizeFrameMesh: false);
            }, Asset_Gif.MaxSize.DONT_RESIZE, sceneId, componentId);

            // if (fetchRoutine.Current == null)
            //     yield break;
            yield break;
        }

        //We fall back to the nft original image which can have a really big size
        if (!foundDCLImage && !string.IsNullOrEmpty(originalImageURL))
        {
            // Debug.Log($"pravs - NFTShapeLoaderController.FetchNFTImage() - Will download LARGE IMAGE: " + originalImageURL);

            yield return WrappedTextureUtils.Fetch(originalImageURL, (downloadedAsset) =>
            {
                foundDCLImage = true;
                // SetFrameImage(downloadedAsset, resizeFrameMesh: true); // TODO: resixing is not working correctly
                SetFrameImage(downloadedAsset, resizeFrameMesh: false);
            }, Asset_Gif.MaxSize._256, sceneId, componentId);
        }

        FinishLoading(foundDCLImage);
    }

    void FinishLoading(bool loadedSuccessfully)
    {
        if (loadedSuccessfully)
        {
            if (spinner != null)
                spinner.SetActive(false);

            OnLoadingAssetSuccess?.Invoke();
        }
        else
        {
            OnLoadingAssetFail?.Invoke();
        }
    }

    public void UpdateGIFPointer(int width, int height, System.IntPtr pointer)
    {
        if (width == 0 || height == 0)
        {
            Debug.Log("pravs - Couldn't create external texture! width or height are 0!");
            return;
        }

        Debug.Log("pravs - NFTShapeLoaderController.UpdaeGIFPointer() - creating external texture, tex name/id/pointer: " + pointer);
        Texture2D newTex = Texture2D.CreateExternalTexture(width, height, TextureFormat.ARGB32, false, false, pointer);

        if (newTex == null)
        {
            Debug.Log("pravs - Couldn't create external texture!");
            return;
        }

        newTex.wrapMode = TextureWrapMode.Clamp;
        imageMaterial.SetTexture(BASEMAP_SHADER_PROPERTY, newTex);
        imageMaterial.SetColor(COLOR_SHADER_PROPERTY, Color.white);

        FinishLoading(true);
    }

    void SetFrameImage(DCL.ITexture newAsset, bool resizeFrameMesh = false)
    {
        if (newAsset == null) return;

        nftAsset = newAsset;

        if (nftAsset is Asset_Gif gifAsset)
        {
            gifAsset.OnFrameTextureChanged -= UpdateTexture;
            gifAsset.OnFrameTextureChanged += UpdateTexture;
            gifAsset.Play(false);
        }

        UpdateTexture(nftAsset.texture);

        if (resizeFrameMesh)
        {
            Vector3 newScale = new Vector3(newAsset.width / NFTDataFetchingSettings.NORMALIZED_DIMENSIONS.x,
                newAsset.height / NFTDataFetchingSettings.NORMALIZED_DIMENSIONS.y, 1f);

            meshRenderer.transform.localScale = newScale;
        }
    }

    void UpdateTexture(Texture2D texture)
    {
        if (imageMaterial == null)
            return;

        imageMaterial.SetTexture(BASEMAP_SHADER_PROPERTY, texture);
        imageMaterial.SetColor(COLOR_SHADER_PROPERTY, Color.white);
    }

    void InitializePerlinNoise()
    {
        if (frameMaterial == null)
            return;

        frameMaterial.shaderKeywords = null;

        if (noiseType == NoiseType.None)
            return;

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
}