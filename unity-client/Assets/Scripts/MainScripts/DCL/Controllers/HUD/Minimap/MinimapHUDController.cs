﻿using System;
using UnityEngine;

public class MinimapHUDController : IDisposable, IHUD
{
    private static bool VERBOSE = false;

    private MinimapHUDView view;

    private FloatVariable minimapZoom => CommonScriptableObjects.minimapZoom;

    public MinimapHUDModel model { get; private set; }

    public MinimapHUDController() : this(new MinimapHUDModel()) { }

    public MinimapHUDController(MinimapHUDModel model)
    {
        CommonScriptableObjects.playerCoords.OnChange += OnPlayerCoordsChange;
        minimapZoom.Set(1f);

        view = MinimapHUDView.Create(this);
        UpdateData(model);
    }

    public void Dispose()
    {
        CommonScriptableObjects.playerCoords.OnChange -= OnPlayerCoordsChange;
    }

    private void OnPlayerCoordsChange(Vector2Int current, Vector2Int previous)
    {
        UpdatePlayerPosition(current);
        UpdateSceneName(MinimapMetadata.GetMetadata().GetTile(current.x, current.y)?.name);
    }

    public void PartialSceneInfoUpdate(Vector2Int coordinate, string newSceneName)
    {
        if (VERBOSE)
        {
            Debug.Log($"Updating {coordinate.x},{coordinate.y} to {newSceneName}");
        }
    }

    public void PartialSceneInfoUpdate(Vector2Int coordinate, string newSceneName, MinimapMetadata.TileType type)
    {
        if (VERBOSE)
        {
            Debug.Log($"Updating {coordinate.x},{coordinate.y} to {newSceneName} (tyle type: {type})");
        }
    }

    public void UpdateData(MinimapHUDModel model)
    {
        this.model = model;
        view.UpdateData(this.model);
    }

    public void UpdateSceneName(string sceneName)
    {
        model.sceneName = sceneName;
        view.UpdateData(model);
    }

    public void UpdatePlayerPosition(Vector2 position)
    {
        const string format = "{0},{1}";
        UpdatePlayerPosition(string.Format(format, position.x, position.y));
    }

    public void UpdatePlayerPosition(string position)
    {
        model.playerPosition = position;
        view.UpdateData(model);
    }

    public void AddZoomDelta(float delta)
    {
        minimapZoom.Set(Mathf.Clamp01(minimapZoom.Get() + delta));
    }

    public void ToggleOptions()
    {
        view.ToggleOptions();
    }

    public void AddBookmark()
    {
        //TODO:
        if (VERBOSE)
        {
            Debug.Log("Add bookmark pressed");
        }
    }

    public void ReportScene()
    {
        //TODO:
        if (VERBOSE)
        {
            Debug.Log("Report scene pressed");
        }
    }

    public void SetVisibility(bool visible)
    {
        view.SetVisibility(visible);
    }
}