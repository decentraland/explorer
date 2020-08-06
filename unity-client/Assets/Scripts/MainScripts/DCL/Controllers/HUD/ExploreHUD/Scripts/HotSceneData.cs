using System;
using UnityEngine;
using UnityEngine.Networking;

internal class HotSceneData : IDisposable
{
    static public event Action<HotSceneData, bool> OnDisplayStateChanged;
    static public event Action<SceneCellView> OnCellViewFreed;

    public MinimapMetadata.MinimapSceneInfo mapInfo { private set; get; }
    public HotScenesController.HotSceneInfo crowdInfo { private set; get; }
    public Sprite thumbnail { private set; get; }
    public SceneCellView cellView { private set; get; }

    UnityWebRequest thumbnailRequest = null;
    bool triggerDisplayWhenReady = false;
    Texture2D thumbnailTexture;

    public HotSceneData(SceneCellView view)
    {
        cellView = view;
        view.gameObject.SetActive(false);
    }

    public void ResolveCrowdInfo(HotScenesController.HotSceneInfo info, int priority)
    {
        if (cellView == null)
        {
            return;
        }

        crowdInfo = info;
        cellView.transform.SetSiblingIndex(priority);

        SetDisplay(true);

        if (!IsMapInfoResolved())
        {
            ResolveMapInfo(MinimapMetadata.GetMetadata().GetSceneInfo(crowdInfo.baseCoords.x, crowdInfo.baseCoords.y));
        }
    }

    public void ResolveMapInfo(MinimapMetadata.MinimapSceneInfo info)
    {
        if (cellView == null)
        {
            return;
        }

        mapInfo = info;

        if (mapInfo != null)
        {
            FetchThumbnail();
            SetupCellView();
        }
    }

    public bool ShouldResolveMapInfo()
    {
        return !IsMapInfoResolved() && cellView != null;
    }

    public bool IsMapInfoResolved()
    {
        return mapInfo != null;
    }

    public void SetDisplay(bool display)
    {
        if (cellView == null)
        {
            return;
        }

        triggerDisplayWhenReady = display;

        if (!IsMapInfoResolved())
        {
            return;
        }

        if (display)
        {
            SetupCellView();
        }
        else
        {
            OnDisplayStateChanged?.Invoke(this, false);
        }
    }

    public void Dispose()
    {
        GameObject.Destroy(thumbnailTexture);
        GameObject.Destroy(thumbnail);
        GameObject.Destroy(cellView?.gameObject);
    }

    public void DiscardCellView()
    {
        if (cellView == null)
        {
            return;
        }
        OnDisplayStateChanged?.Invoke(this, false);
        OnCellViewFreed?.Invoke(cellView);
        cellView = null;
    }

    void SetupCellView()
    {
        if (cellView == null)
        {
            return;
        }

        cellView.Setup(this);
        if (triggerDisplayWhenReady)
        {
            triggerDisplayWhenReady = false;
            OnDisplayStateChanged?.Invoke(this, true);
        }
    }

    void FetchThumbnail()
    {
        if (cellView == null)
        {
            return;
        }

        if (!IsMapInfoResolved() || thumbnailRequest != null)
        {
            return;
        }

        string url = mapInfo.previewImageUrl;
        if (string.IsNullOrEmpty(url))
        {
            url = GetMarketPlaceThumbnailUrl(mapInfo, 196, 194, 50);
        }

        thumbnailRequest = UnityWebRequestTexture.GetTexture(url);
        UnityWebRequestAsyncOperation op = thumbnailRequest.SendWebRequest();
        op.completed += (_) =>
        {
            if (!thumbnailRequest.isNetworkError && !thumbnailRequest.isHttpError)
            {
                thumbnailTexture = ((DownloadHandlerTexture)thumbnailRequest.downloadHandler).texture;
                thumbnailTexture.Compress(false);
                thumbnail = Sprite.Create(thumbnailTexture, new Rect(0, 0, thumbnailTexture.width, thumbnailTexture.height), Vector2.zero);
                cellView.SetThumbnailSprite(thumbnail);
            }
            else
            {
                Debug.Log($"Error downloading: {url} {thumbnailRequest.error}");
            }
            thumbnailRequest.Dispose();
            thumbnailRequest = null;
        };
    }

    static string GetMarketPlaceThumbnailUrl(MinimapMetadata.MinimapSceneInfo info, int width, int height, int sizeFactor)
    {
        string parcels = "";
        Vector2Int min = new Vector2Int(int.MaxValue, int.MaxValue);
        Vector2Int max = new Vector2Int(int.MinValue, int.MinValue);
        Vector2Int coord;

        for (int i = 0; i < info.parcels.Count; i++)
        {
            coord = info.parcels[i];
            parcels += string.Format("{0},{1}", coord.x, coord.y);
            if (i < info.parcels.Count - 1) parcels += ";";

            if (coord.x < min.x) min.x = coord.x;
            if (coord.y < min.y) min.y = coord.y;
            if (coord.x > max.x) max.x = coord.x;
            if (coord.y > max.y) max.y = coord.y;
        }

        int centerX = (int)(min.x + (max.x - min.x) * 0.5f);
        int centerY = (int)(min.y + (max.y - min.y) * 0.5f);
        int sceneMaxSize = Mathf.Clamp(Mathf.Max(max.x - min.x, max.y - min.y), 1, int.MaxValue);
        int size = sizeFactor / sceneMaxSize;

        return string.Format("https://api.decentraland.org/v1/map.png?width={0}&height={1}&size={2}&center={3}&selected={4}",
            width, height, size, $"{centerX},{centerY}", parcels);
    }
}
