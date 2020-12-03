using UnityEngine;
using System;
using Variables.RealmsInfo;


internal class UserPositionMarker : IDisposable
{
    static readonly Color SAME_REALM_COLOR = new Color(255, 18, 98);
    static readonly Color OTHER_REALM_COLOR = Color.blue;

    public Vector2Int coords { set; get; }
    public string realmServer { set; get; }
    public string realmLayer { set; get; }

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
        if (active)
        {
            OnRealmChanged(DataStore.playerRealm.Get(), null);
            DataStore.playerRealm.OnChange += OnRealmChanged;
        }
        else
        {
            DataStore.playerRealm.OnChange -= OnRealmChanged;
        }
        gameObject.SetActive(active);
    }

    public void Dispose()
    {
        DataStore.playerRealm.OnChange -= OnRealmChanged;
        GameObject.Destroy(gameObject);
    }

    private void OnRealmChanged(CurrentRealmModel current, CurrentRealmModel prev)
    {
        SetColor(current.Equals(realmServer, realmLayer) ? SAME_REALM_COLOR : OTHER_REALM_COLOR);
    }

    private void SetColor(Color color)
    {

    }
}