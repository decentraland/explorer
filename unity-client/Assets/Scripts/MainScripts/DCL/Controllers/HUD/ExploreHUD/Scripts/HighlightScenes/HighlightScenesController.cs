using System.Collections.Generic;
using UnityEngine;
using DCL.Interface;

internal class HighlightScenesController : MonoBehaviour
{
    const float SCENES_UPDATE_INTERVAL = 5;

    [SerializeField] SceneCellView sceneCellViewRoot;
    [SerializeField] GameObject highlightScenesContent;
    [SerializeField] GameObject loadingSpinner;

    Dictionary<Vector2Int, HotSceneData> cachedScenes = new Dictionary<Vector2Int, HotSceneData>();
    List<HotSceneData> activeScenes = new List<HotSceneData>();

    List<HotSceneData> pendingSceneInfo = new List<HotSceneData>();
    Queue<SceneCellView> pooledScenCells = new Queue<SceneCellView>();

    bool isActiveListLocked = false;

    public void Initialize()
    {
        SetPooledSceneCell(sceneCellViewRoot);
        for (int i = 0; i < 5; i++)
        {
            SetPooledSceneCell(CreateSceneCell());
        }
        HotSceneData.OnDisplayStateChanged += OnHotSceneDisplayStateChanged;
        HotSceneData.OnCellViewFreed += SetPooledSceneCell;
    }

    public void RefreshIfNeeded()
    {
        if (cachedScenes.Count == 0 || HotScenesController.i.timeSinceLastUpdate >= SCENES_UPDATE_INTERVAL)
        {
            FetchHotScenes();
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

        isActiveListLocked = true;
        for (int i = 0; i < pendingSceneInfo.Count; i++)
        {
            pendingSceneInfo[i].DiscardCellView();
        }
        pendingSceneInfo.Clear();

        for (int i = 0; i < activeScenes.Count; i++)
        {
            activeScenes[i].SetDisplay(false);
        }
        activeScenes.Clear();
        isActiveListLocked = false;

        for (int i = 0; i < HotScenesController.i.hotScenesList.Count; i++)
        {
            ProcessReceivedHotScene(HotScenesController.i.hotScenesList[i], i);
        }

        if (pendingSceneInfo.Count > 0)
        {
            MinimapMetadata.GetMetadata().OnSceneInfoUpdated -= OnMapInfoUpdated;
            MinimapMetadata.GetMetadata().OnSceneInfoUpdated += OnMapInfoUpdated;
        }
    }

    void ProcessReceivedHotScene(HotScenesController.HotSceneInfo hotSceneInfo, int priority)
    {
        HotSceneData hotSceneDataController = null;
        cachedScenes.TryGetValue(hotSceneInfo.baseCoords, out hotSceneDataController);

        if (hotSceneDataController == null)
        {
            hotSceneDataController = new HotSceneData(GetPooledSceneCell());
            cachedScenes.Add(hotSceneInfo.baseCoords, hotSceneDataController);
        }

        hotSceneDataController.ResolveCrowdInfo(hotSceneInfo, priority);

        if (hotSceneDataController.ShouldResolveMapInfo())
        {
            pendingSceneInfo.Add(hotSceneDataController);
            WebInterface.RequestScenesInfoAroundParcel(hotSceneInfo.baseCoords, 0);
        }
    }

    void OnMapInfoUpdated(MinimapMetadata.MinimapSceneInfo mapInfo)
    {
        for (int i = pendingSceneInfo.Count - 1; i >= 0; i--)
        {
            if (mapInfo.parcels.Contains(pendingSceneInfo[i].crowdInfo.baseCoords))
            {
                pendingSceneInfo[i].ResolveMapInfo(mapInfo);
                pendingSceneInfo.RemoveAt(i);
            }
        }

        if (pendingSceneInfo.Count == 0)
        {
            MinimapMetadata.GetMetadata().OnSceneInfoUpdated -= OnMapInfoUpdated;
        }
    }

    void OnHotSceneDisplayStateChanged(HotSceneData sceneData, bool display)
    {
        if (isActiveListLocked)
        {
            return;
        }

        if (display)
        {
            sceneData.cellView.gameObject.SetActive(true);
            activeScenes.Add(sceneData);
            loadingSpinner.SetActive(false);
        }
        else
        {
            sceneData.cellView.gameObject.SetActive(false);
            activeScenes.Remove(sceneData);
        }
    }

    SceneCellView GetPooledSceneCell()
    {
        if (pooledScenCells.Count > 0)
        {
            return pooledScenCells.Dequeue();
        }
        return CreateSceneCell();
    }

    SceneCellView CreateSceneCell()
    {
        return GameObject.Instantiate(sceneCellViewRoot, sceneCellViewRoot.transform.parent);
    }

    void SetPooledSceneCell(SceneCellView cellView)
    {
        cellView.gameObject.SetActive(false);
        pooledScenCells.Enqueue(cellView);
    }

    void OnDestroy()
    {
        HotSceneData.OnDisplayStateChanged -= OnHotSceneDisplayStateChanged;
        HotScenesController.i.OnHotSceneListFinishUpdating -= OnFetchHotScenes;
        MinimapMetadata.GetMetadata().OnSceneInfoUpdated -= OnMapInfoUpdated;
        HotSceneData.OnCellViewFreed -= SetPooledSceneCell;

        using (var iterator = cachedScenes.GetEnumerator())
        {
            while (iterator.MoveNext())
            {
                iterator.Current.Value.Dispose();
            }
        }
    }
}
