﻿using System;
using System.Collections.Generic;
using UnityEngine;

public class MapGlobalUsersPositionMarkerController : IDisposable
{
    private const float UPDATE_INTERVAL_INITIAL = 10f;
    private const float UPDATE_INTERVAL_FOREGROUND = 60f;
    private const float UPDATE_INTERVAL_BACKGROUND = 5 * 60f;

    private const int MAX_MARKERS = 200;

    FetchScenesHandler fetchScenesHandler;
    MarkersHandler markersHandler;
    UserPositionHandler userPositionHandler;

    int commsRadius = 4;

    public MapGlobalUsersPositionMarkerController(GameObject markerPrefab, Transform overlayContainer, Func<float, float, Vector3> coordToMapPosFunc)
    {
        fetchScenesHandler = new FetchScenesHandler(UPDATE_INTERVAL_INITIAL, UPDATE_INTERVAL_FOREGROUND, UPDATE_INTERVAL_BACKGROUND);
        markersHandler = new MarkersHandler(markerPrefab, overlayContainer, MAX_MARKERS, coordToMapPosFunc);
        userPositionHandler = new UserPositionHandler();

        fetchScenesHandler.OnScenesFetched += OnScenesFetched;
        userPositionHandler.OnPlayerCoordsChanged += OnPlayerCoordsChanged;
        CommonScriptableObjects.rendererState.OnChange += OnRenderStateChanged;

        KernelConfig.i.EnsureConfigInitialized().Then(config =>
        {
            commsRadius = (int)config.comms.commRadius;
            OnPlayerCoordsChanged(userPositionHandler.playerCoords);
        });
        OnRenderStateChanged(CommonScriptableObjects.rendererState.Get(), false);
    }

    public void SetInForeground()
    {
        fetchScenesHandler.SetUpdateMode(FetchScenesHandler.UpdateMode.FOREGROUND);
    }

    public void SetInBackground()
    {
        fetchScenesHandler.SetUpdateMode(FetchScenesHandler.UpdateMode.BACKGROUND);
    }

    public void Dispose()
    {
        fetchScenesHandler.OnScenesFetched -= OnScenesFetched;
        userPositionHandler.OnPlayerCoordsChanged -= OnPlayerCoordsChanged;

        fetchScenesHandler.Dispose();
        markersHandler.Dispose();
        userPositionHandler.Dispose();
    }

    private void OnScenesFetched(List<HotScenesController.HotSceneInfo> sceneList)
    {
        markersHandler.SetMarkers(sceneList);
    }

    private void OnPlayerCoordsChanged(Vector2Int coords)
    {
        markersHandler.SetExclusionArea(coords, commsRadius);
    }

    private void OnRenderStateChanged(bool current, bool prev)
    {
        if (!current)
            return;

        // NOTE: we start fetching scenes after the renderer is activated for the first time
        CommonScriptableObjects.rendererState.OnChange -= OnRenderStateChanged;
        fetchScenesHandler.Init();
    }
}
