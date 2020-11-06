using UnityEngine;
using System;

internal class UserPositionMarker : IDisposable
{
    public Vector2Int coords { set; get; }
    public GameObject gameObject { private set; get; }

    private GameObject markerGameObject;

    public UserPositionMarker(GameObject gameObject)
    {
        this.gameObject = gameObject;
        gameObject.SetActive(false);
    }

    public void Dispose()
    {
        GameObject.Destroy(gameObject);
    }
}
