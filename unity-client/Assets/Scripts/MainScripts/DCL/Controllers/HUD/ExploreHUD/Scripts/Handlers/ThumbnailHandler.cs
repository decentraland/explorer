using System;
using UnityEngine;
using UnityEngine.Networking;
using DCL.Helpers;

internal class ThumbnailHandler : IDisposable
{
    public Texture2D texture { private set; get; }

    UnityWebRequest thumbnailRequest = null;

    public void FetchThumbnail(string url, Action<Texture2D> onSuccess, Action onFail)
    {
        if (!(texture is null))
        {
            onSuccess?.Invoke(texture);
        }
        else if (string.IsNullOrEmpty(url))
        {
            onFail?.Invoke();
        }
        else if (thumbnailRequest is null)
        {
            thumbnailRequest = UnityWebRequestTexture.GetTexture(url);
            UnityWebRequestAsyncOperation op = thumbnailRequest.SendWebRequest();
            op.completed += (_) =>
            {
                if (thumbnailRequest == null)
                    return;

                bool success = thumbnailRequest.WebRequestSucceded();
                if (success)
                {
                    texture = ((DownloadHandlerTexture)thumbnailRequest.downloadHandler).texture;
                    texture.Compress(true);
                    onSuccess?.Invoke(texture);
                }

                thumbnailRequest.Dispose();
                thumbnailRequest = null;

                if (!success)
                {
                    Debug.Log($"Error downloading: {url}");
                    onFail?.Invoke();
                }
            };
        }
    }

    public void Dispose()
    {
        GameObject.Destroy(texture);
        if (!(thumbnailRequest is null))
        {
            thumbnailRequest.Abort();
            thumbnailRequest.Dispose();
            thumbnailRequest = null;
        }
    }
}
