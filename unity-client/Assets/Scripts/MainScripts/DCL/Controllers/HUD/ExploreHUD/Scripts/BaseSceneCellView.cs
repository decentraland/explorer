using UnityEngine;
using UnityEngine.UI;
using TMPro;

internal class BaseSceneCellView : BaseCellView, IMapDataView
{
    public delegate void JumpInDelegate(Vector2Int coords, string serverName, string layerName);
    static public event JumpInDelegate OnJumpIn;

    [SerializeField] TextMeshProUGUI sceneName;
    [SerializeField] Button jumpIn;

    MinimapMetadata.MinimapSceneInfo mapInfo;
    Vector2Int baseCoords;

    protected virtual void Awake()
    {
        jumpIn.onClick.AddListener(JumpInPressed);
    }

    public virtual void JumpInPressed()
    {
        if (mapInfo != null)
        {
            JumpIn(baseCoords, null, null);
        }
    }

    protected void JumpIn(Vector2Int coords, string serverName, string layerName)
    {
        OnJumpIn?.Invoke(coords, serverName, layerName);
    }

    MinimapMetadata.MinimapSceneInfo IMapDataView.GetMinimapSceneInfo()
    {
        return mapInfo;
    }

    bool IMapDataView.HasMinimapSceneInfo()
    {
        return mapInfo != null;
    }

    void IMapDataView.SetBaseCoord(Vector2Int coords)
    {
        baseCoords = coords;
    }

    Vector2Int IMapDataView.GetBaseCoord()
    {
        return baseCoords;
    }

    void IMapDataView.SetMinimapSceneInfo(MinimapMetadata.MinimapSceneInfo info)
    {
        mapInfo = info;
        sceneName.text = info.name;

        if (GetThumbnail() == null)
        {
            string url = mapInfo.previewImageUrl;
            if (string.IsNullOrEmpty(url))
            {
                url = GetMarketPlaceThumbnailUrl(mapInfo, 196, 194, 50);
            }
            FetchThumbnail(url);
        }
    }

    bool IMapDataView.ContainCoords(Vector2Int coords)
    {
        if (mapInfo == null) return false;
        return mapInfo.parcels.Contains(coords);
    }

    GameObject IMapDataView.GetGameObject()
    {
        return gameObject;
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
