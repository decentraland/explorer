using System;
using System.Collections;
using DCL;
using DCL.Controllers.Gif;
using UnityEngine;

internal interface INFTAsset : IDisposable
{
    bool isGif { get; }
    bool isHQ { get; }
    DCL.ITexture previewAsset { get; }
    DCL.ITexture hqAsset { get; }
    Action<Texture2D> UpdateTextureCallback { set; }
    void FetchAndSetHQAsset(string url, Action onSuccess, Action onFail);
    void RestorePreviewAsset();
}

internal static class NFTAssetFactory
{
    public static INFTAsset CreateAsset(ITexture asset)
    {
        if (asset == null)
        {
            return null;
        }

        if (asset is Asset_Gif gif)
        {
            return new GifAsset(gif);
        }
        return new ImageAsset(asset);
    }
}

internal class ImageAsset : INFTAsset
{
    public bool isGif => false;
    public bool isHQ => hqAsset != null;
    public Action<Texture2D> UpdateTextureCallback { set; get; }
    public ITexture previewAsset => previewTexture;
    public ITexture hqAsset => hqTexture != null ? hqTexture.asset : null;

    private AssetPromise_Texture hqTexture;
    private ITexture previewTexture;

    public ImageAsset(ITexture previewTexture)
    {
        this.previewTexture = previewTexture;
    }

    public void Dispose()
    {
        if (hqTexture == null)
            return;

        AssetPromiseKeeper_Texture.i.Forget(hqTexture);
        hqTexture = null;
        UpdateTextureCallback = null;
    }

    public void FetchAndSetHQAsset(string url, Action onSuccess, Action onFail)
    {
        hqTexture = new AssetPromise_Texture(url);
        AssetPromiseKeeper_Texture.i.Keep(hqTexture);

        hqTexture.OnSuccessEvent += (asset) =>
        {
            UpdateTextureCallback?.Invoke(hqTexture.asset.texture);
            onSuccess?.Invoke();
        };
        hqTexture.OnFailEvent += (asset) =>
        {
            hqTexture = null;
            onFail?.Invoke();
        };
    }

    public void RestorePreviewAsset()
    {
        if (hqTexture != null)
        {
            AssetPromiseKeeper_Texture.i.Forget(hqTexture);
            hqTexture = null;
        }
        UpdateTextureCallback?.Invoke(previewAsset.texture);
    }
}

internal class GifAsset : INFTAsset
{
    public bool isGif => true;
    public bool isHQ => hqAsset != null;
    public Action<Texture2D> UpdateTextureCallback { set; get; }
    public ITexture previewAsset => previewGif;
    public ITexture hqAsset => hqTexture != null ? hqTexture : null;

    private Asset_Gif hqTexture;
    private Asset_Gif previewGif;
    private Coroutine loadRoutine;

    public GifAsset(Asset_Gif previewGif)
    {
        this.previewGif = previewGif;
    }

    public void Dispose()
    {
        StopGifLoad();

        if (hqTexture != null)
        {
            hqTexture.Dispose();
            hqTexture = null;
        }
        UpdateTextureCallback = null;
    }

    public void FetchAndSetHQAsset(string url, Action onSuccess, Action onFail)
    {
        hqTexture = new Asset_Gif(url,
            (downloadedTex, texturePromise) =>
            {
                StopGif(previewGif);
                StartGif(hqTexture);
                onSuccess?.Invoke();
            },
            () =>
            {
                hqTexture = null;
                onFail?.Invoke();
            });
        loadRoutine = CoroutineStarter.Start(LoadGif(hqTexture));
    }

    public void RestorePreviewAsset()
    {
        StopGifLoad();

        if (hqTexture != null)
        {
            hqTexture.Dispose();
            hqTexture = null;
        }
        StartGif(previewGif);
    }

    private void StartGif(Asset_Gif gif)
    {
        if (UpdateTextureCallback != null)
        {
            gif.OnFrameTextureChanged -= UpdateTextureCallback;
            gif.OnFrameTextureChanged += UpdateTextureCallback;
        }
        gif.Play(false);
    }

    private void StopGif(Asset_Gif gif)
    {
        if (UpdateTextureCallback != null)
        {
            gif.OnFrameTextureChanged -= UpdateTextureCallback;
        }
        gif.Stop();
    }

    private IEnumerator LoadGif(Asset_Gif gif)
    {
        yield return gif.Load();
        loadRoutine = null;
    }

    private void StopGifLoad()
    {
        if (loadRoutine != null)
        {
            CoroutineStarter.Stop(loadRoutine);
            loadRoutine = null;
        }
    }
}