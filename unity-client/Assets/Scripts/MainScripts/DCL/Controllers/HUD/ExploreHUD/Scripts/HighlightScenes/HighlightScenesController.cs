using System.Collections.Generic;
using UnityEngine;
using DCL.Interface;

internal class HighlightScenesController : MonoBehaviour
{
    const float SCENES_UPDATE_INTERVAL = 5;

    [SerializeField] HotSceneCellView hotsceneBaseCellView;
    [SerializeField] GameObject highlightScenesContent;
    [SerializeField] GameObject loadingSpinner;

    Dictionary<Vector2Int, HotSceneCellView> cachedHotScenes = new Dictionary<Vector2Int, HotSceneCellView>();
    List<GameObject> activeCellsView = new List<GameObject>();

    ExploreMiniMapDataController miniMapDataController;
    ViewPool<HotSceneCellView> hotScenesViewPool;

    public void Initialize(ExploreMiniMapDataController mapDataController)
    {
        miniMapDataController = mapDataController;
        hotScenesViewPool = new ViewPool<HotSceneCellView>(hotsceneBaseCellView, 5);
    }

    public void RefreshIfNeeded()
    {
        if (cachedHotScenes.Count == 0 || HotScenesController.i.timeSinceLastUpdate >= SCENES_UPDATE_INTERVAL)
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

        miniMapDataController.ClearPending();
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

        if (cachedHotScenes.ContainsKey(baseCoords))
        {
            hotSceneView = cachedHotScenes[baseCoords];
            if (!hotSceneView) return;
        }
        else
        {
            hotSceneView = hotScenesViewPool.GetView();
            cachedHotScenes.Add(baseCoords, hotSceneView);
        }

        hotSceneView.transform.SetSiblingIndex(priority);

        ICrowdDataView crowdView = hotSceneView;
        crowdView.SetCrowdInfo(hotSceneInfo);

        IMapDataView mapView = hotSceneView;
        miniMapDataController.SetMinimapData(baseCoords, mapView,
            (resolvedView) =>
            {
                SetActiveCell(resolvedView.GetGameObject());
            },
            (rejectedView) =>
            {
                var sceneView = rejectedView as HotSceneCellView;
                if (sceneView)
                {
                    hotScenesViewPool.PoolView(sceneView);
                    cachedHotScenes[rejectedView.GetBaseCoord()] = null;
                }
            });
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
        hotScenesViewPool.Dispose();
    }
}
