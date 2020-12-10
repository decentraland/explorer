using System.Collections;
using System;
using DCL.Helpers;
using UnityEngine.Networking;

public class GifProcessor
{
    private bool jsGIFProcessingEnabled = false;
    private UnityWebRequest webRequest;

    public GifProcessor()
    {
        KernelConfig.i.EnsureConfigInitialized().Then(config => jsGIFProcessingEnabled = config.gifSupported);
    }

    public IEnumerator Load(string url, Action<GifFrameData[]> OnSuccess, Action OnFail)
    {
        if (jsGIFProcessingEnabled)
        {
            yield return JSProcessorLoad(url, OnSuccess, OnFail);
        }
        else
        {
            yield return UniGifProcessorLoad(url, OnSuccess, OnFail);
        }
    }

    public void DisposeGif(string url)
    {
        if (jsGIFProcessingEnabled)
        {
            DCL.GIFProcessingBridge.i.DeleteGIF(url);
        }
    }

    private IEnumerator JSProcessorLoad(string url, Action<GifFrameData[]> OnSuccess, Action OnFail)
    {
        bool fetchFailed = false;
        yield return DCL.GIFProcessingBridge.i.RequestGIFProcessor(url,
            (GifFrameData[] newTextures) =>
            {
                if (newTextures == null || newTextures.Length == 0)
                {
                    fetchFailed = true;
                    return;
                }
                OnSuccess?.Invoke(newTextures);
            }, () => fetchFailed = true);

        if (fetchFailed)
        {
            OnFail?.Invoke();
        }
    }

    private IEnumerator UniGifProcessorLoad(string url, Action<GifFrameData[]> OnSuccess, Action OnFail)
    {
        webRequest = UnityWebRequest.Get(url);
        webRequest.SendWebRequest();
        yield return webRequest;

        bool success = webRequest != null && webRequest.WebRequestSucceded();
        if (success)
        {
            var bytes = webRequest.downloadHandler.data;
            yield return UniGif.GetTextureListCoroutine(bytes,
                (frames,loopCount, width, height) => OnSuccess?.Invoke(frames));
        }
        else
        {
            OnFail?.Invoke();
        }
        webRequest.Dispose();
        webRequest = null;
    }
}
