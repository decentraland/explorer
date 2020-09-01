using System;
using DCL.Helpers;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

internal class BaseCellView : MonoBehaviour
{
    [SerializeField] Image thumbnailImage;
    [SerializeField] GameObject loadingSpinner;
    [SerializeField] Sprite errorThumbnail;

    public event Action<Sprite> OnThumbnailFetched;

    UnityWebRequest thumbnailRequest = null;
    Texture2D thumbnailTexture;
    Sprite thumbnail;

    public void FetchThumbnail(string url, Action onFetchFail)
    {
        if (thumbnail)
        {
            OnThumbnailFetched?.Invoke(thumbnail);
        }
        else if (string.IsNullOrEmpty(url))
        {
            onFetchFail?.Invoke();
        }
        else if (thumbnailRequest == null)
        {
            thumbnailImage.sprite = null;

            thumbnailRequest = UnityWebRequestTexture.GetTexture(url);
            UnityWebRequestAsyncOperation op = thumbnailRequest.SendWebRequest();
            op.completed += (_) =>
            {
                if (thumbnailRequest == null)
                    return;

                bool success = thumbnailRequest.WebRequestSucceded();
                if (success)
                {
                    thumbnailTexture = ((DownloadHandlerTexture)thumbnailRequest.downloadHandler).texture;
                    thumbnailTexture.Compress(false);
                    var thumbnailSprite = Sprite.Create(thumbnailTexture, new Rect(0, 0, thumbnailTexture.width, thumbnailTexture.height), Vector2.zero);
                    SetThumbnail(thumbnailSprite);
                }

                thumbnailRequest.Dispose();
                thumbnailRequest = null;

                if (!success)
                {
                    Debug.Log($"Error downloading: {url}");
                    onFetchFail?.Invoke();
                }
            };
        }
    }

    public void SetDefaultThumbnail()
    {
        SetThumbnail(errorThumbnail);
    }

    public Sprite GetThumbnail()
    {
        return thumbnail;
    }

    protected virtual void OnEnable()
    {
        if (thumbnail == null)
        {
            loadingSpinner.SetActive(true);
        }
        else
        {
            loadingSpinner.SetActive(false);
        }
    }

    protected virtual void OnDestroy()
    {
        GameObject.Destroy(thumbnailTexture);
        if (thumbnailRequest != null)
        {
            thumbnailRequest.Abort();
            thumbnailRequest.Dispose();
            thumbnailRequest = null;
        }
    }

    private void SetThumbnail(Sprite thmbnail)
    {
        thumbnail = thmbnail;
        thumbnailImage.sprite = thumbnail;
        loadingSpinner.SetActive(false);
        OnThumbnailFetched?.Invoke(thumbnail);
    }
}
