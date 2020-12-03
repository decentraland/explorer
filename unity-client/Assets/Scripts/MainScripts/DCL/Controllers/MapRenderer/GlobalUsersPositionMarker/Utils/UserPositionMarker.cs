using UnityEngine;
using System;

internal class UserPositionMarker : IDisposable
{
    public Vector2Int coords { set; get; }
    public string name { set { gameObject.name = value; } }
    public Vector3 localPosition { set { gameObject.transform.localPosition = value; } }

    private GameObject gameObject;

    public UserPositionMarker(GameObject gameObject)
    {
        this.gameObject = gameObject;
        gameObject.SetActive(false);
    }

    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }

    public void Dispose()
    {
        GameObject.Destroy(gameObject);
    }
}
