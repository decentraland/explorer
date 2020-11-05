using UnityEngine;
using System;

internal class UserPositionMarker : IDisposable
{
    public Vector2Int coords { set; get; }

    private GameObject markerGameObject;

    public UserPositionMarker(GameObject gameObject)
    {
        markerGameObject = gameObject;
        markerGameObject.SetActive(false);
    }

    public void Dispose()
    {
        GameObject.Destroy(markerGameObject);
    }

    public void SetActive(bool active)
    {
        markerGameObject.SetActive(active);
    }
}
