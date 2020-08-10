using System.Collections.Generic;
using UnityEngine;
using DCL.Interface;

internal class HighlightScenesController : MonoBehaviour
{
    const float SCENES_UPDATE_INTERVAL = 5;

    [SerializeField] HotSceneCellView hotsceneBaseCellView;
    [SerializeField] GameObject highlightScenesContent;
    [SerializeField] GameObject loadingSpinner;

    Dictionary<Vector2Int, HotSceneCellView> cachedScenes = new Dictionary<Vector2Int, HotSceneCellView>();
    Queue<HotSceneCellView> pooledHotScenCells = new Queue<HotSceneCellView>();

    List<GameObject> activeCellsView = new List<GameObject>();
    List<IMapDataView> pendingSceneInfo = new List<IMapDataView>();

    public void Initialize()
    {
        SetPooledHotSceneCell(hotsceneBaseCellView);
        for (int i = 0; i < 5; i++)
        {
            SetPooledHotSceneCell(CreateHotSceneCell());
        }

        MinimapMetadata.GetMetadata().OnSceneInfoUpdated -= OnMapInfoUpdated;
        MinimapMetadata.GetMetadata().OnSceneInfoUpdated += OnMapInfoUpdated;
    }

    public void RefreshIfNeeded()
    {
        if (cachedScenes.Count == 0 || HotScenesController.i.timeSinceLastUpdate >= SCENES_UPDATE_INTERVAL)
        {
            FetchHotScenes();
        }
        else
        {
            loadingSpinner.SetActive(false);
        }
    }

    void FetchHotScenes()
    {
        loadingSpinner.SetActive(true);

        WebInterface.FetchHotScenes();

        HotScenesController.i.OnHotSceneListFinishUpdating -= OnFetchHotScenes;
        HotScenesController.i.OnHotSceneListFinishUpdating += OnFetchHotScenes;
    }

    void OnFetchHotScenes()
    {
        HotScenesController.i.OnHotSceneListFinishUpdating -= OnFetchHotScenes;

        ClearMapInfoPendingList();
        HideActiveCells();

        for (int i = 0; i < HotScenesController.i.hotScenesList.Count; i++)
        {
            ProcessReceivedHotScene(HotScenesController.i.hotScenesList[i], i);
        }
    }

    void ProcessReceivedHotScene(HotScenesController.HotSceneInfo hotSceneInfo, int priority)
    {
        Vector2Int baseCoords = hotSceneInfo.baseCoords;
        HotSceneCellView hotSceneView = null;

        if (cachedScenes.ContainsKey(baseCoords))
        {
            hotSceneView = cachedScenes[baseCoords];
            if (!hotSceneView) return;
        }
        else
        {
            hotSceneView = GetPooledHotSceneCell();
            cachedScenes.Add(baseCoords, hotSceneView);
        }

        hotSceneView.transform.SetSiblingIndex(priority);

        ((ICrowdDataView)hotSceneView).SetCrowdInfo(hotSceneInfo);
        IMapDataView mapView = hotSceneView;

        if (!mapView.HasMinimapSceneInfo())
        {
            mapView.SetBaseCoord(baseCoords);

            var mapInfo = MinimapMetadata.GetMetadata().GetSceneInfo(baseCoords.x, baseCoords.y);
            if (mapInfo != null)
            {
                mapView.SetMinimapSceneInfo(mapInfo);
                SetActiveCell(mapView.GetGameObject());
            }
            else
            {
                pendingSceneInfo.Add(mapView);
            }
        }
        else
        {
            SetActiveCell(mapView.GetGameObject());
        }
    }

    void OnMapInfoUpdated(MinimapMetadata.MinimapSceneInfo mapInfo)
    {
        Vector2Int baseCoords;
        for (int i = pendingSceneInfo.Count - 1; i >= 0; i--)
        {
            baseCoords = pendingSceneInfo[i].GetBaseCoord();
            if (mapInfo.parcels.Contains(baseCoords))
            {
                pendingSceneInfo[i].SetMinimapSceneInfo(mapInfo);
                SetActiveCell(pendingSceneInfo[i].GetGameObject());
                pendingSceneInfo.RemoveAt(i);
                break;
            }
        }
    }

    HotSceneCellView GetPooledHotSceneCell()
    {
        HotSceneCellView ret;
        if (pooledHotScenCells.Count > 0)
        {
            ret = pooledHotScenCells.Dequeue();
        }
        ret = CreateHotSceneCell();
        ret.gameObject.SetActive(false);
        return ret;
    }

    HotSceneCellView CreateHotSceneCell()
    {
        return GameObject.Instantiate(hotsceneBaseCellView, hotsceneBaseCellView.transform.parent);
    }

    void SetPooledHotSceneCell(HotSceneCellView cellView)
    {
        cellView.gameObject.SetActive(false);
        pooledHotScenCells.Enqueue(cellView);
    }

    void ClearMapInfoPendingList()
    {
        for (int i = 0; i < pendingSceneInfo.Count; i++)
        {
            var hotSceneView = pendingSceneInfo[i] as HotSceneCellView;
            if (hotSceneView)
            {
                SetPooledHotSceneCell(hotSceneView);
                cachedScenes[pendingSceneInfo[i].GetBaseCoord()] = null;
            }
        }
        pendingSceneInfo.Clear();
    }

    void HideActiveCells()
    {
        for (int i = 0; i < activeCellsView.Count; i++)
        {
            activeCellsView[i].SetActive(false);
        }
    }

    void SetActiveCell(GameObject view)
    {
        if (view == null) return;

        view.gameObject.SetActive(true);
        activeCellsView.Add(view.gameObject);
        loadingSpinner.SetActive(false);
    }

    void OnDestroy()
    {
        HotScenesController.i.OnHotSceneListFinishUpdating -= OnFetchHotScenes;
        MinimapMetadata.GetMetadata().OnSceneInfoUpdated -= OnMapInfoUpdated;
    }
}
