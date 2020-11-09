﻿using System;
using UnityEngine;

internal class UserPositionHandler : IDisposable
{
    public Vector2Int playerCoords { private set; get; }
    public event Action<Vector2Int> OnPlayerCoordsChanged;

    public UserPositionHandler()
    {
        playerCoords = CommonScriptableObjects.playerCoords.Get();
        CommonScriptableObjects.playerCoords.OnChange += OnPlayerCoords;
    }

    public void Dispose()
    {
        CommonScriptableObjects.playerCoords.OnChange -= OnPlayerCoords;
    }

    private void OnPlayerCoords(Vector2Int current, Vector2Int prev)
    {
        playerCoords = current;
        OnPlayerCoordsChanged?.Invoke(playerCoords);
    }
}
