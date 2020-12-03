using UnityEngine;
using System;
using Variables.RealmsInfo;


internal class UserPositionMarker : IDisposable
{
    static readonly Color SAME_REALM_COLOR = new Color(1, 0.07f, 0.38f);
    static readonly Color OTHER_REALM_COLOR = Color.blue;

    public Vector2Int coords { set; get; }
    public string realmServer { set; get; }
    public string realmLayer { set; get; }

    public string name { set { markerObject.name = value; } }
    public Vector3 localPosition { set { markerObject.transform.localPosition = value; } }

    private UserMarkerObject markerObject;

    public UserPositionMarker(UserMarkerObject markerObject)
    {
        this.markerObject = markerObject;
        markerObject.gameObject.SetActive(false);
    }

    public void SetActive(bool active)
    {
        if (active)
        {
            OnRealmChanged(DataStore.playerRealm.Get(), null);
            DataStore.playerRealm.OnChange -= OnRealmChanged;
            DataStore.playerRealm.OnChange += OnRealmChanged;
        }
        else
        {
            DataStore.playerRealm.OnChange -= OnRealmChanged;
        }
        markerObject.gameObject.SetActive(active);
    }

    public void Dispose()
    {
        DataStore.playerRealm.OnChange -= OnRealmChanged;
        GameObject.Destroy(markerObject.gameObject);
    }

    private void OnRealmChanged(CurrentRealmModel current, CurrentRealmModel prev)
    {
        if (current == null)
            return;

        SetColor(current.Equals(realmServer, realmLayer) ? markerObject.sameRealmColor : markerObject.otherRealmColor);
    }

    private void SetColor(Color color)
    {
        markerObject.color = color;
    }
}