using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

internal class UserPositionMarkersHandler : IDisposable
{
    readonly Queue<UserPositionMarker> availableMarkers;
    readonly Queue<UserPositionMarker> onUseMarkers;
    readonly Func<float, float, Vector3> coordToMapPosition;

    int maxMarkers;

    public UserPositionMarkersHandler(GameObject markerPrefab, Transform overlayContainer, int maxMarkers, Func<float, float, Vector3> coordToMapPosFunc)
    {
        this.maxMarkers = maxMarkers;
        this.coordToMapPosition = coordToMapPosFunc;

        availableMarkers = new Queue<UserPositionMarker>(maxMarkers);
        onUseMarkers = new Queue<UserPositionMarker>(maxMarkers);

        for (int i = 0; i < maxMarkers; i++)
        {
            var marker = new UserPositionMarker(GameObject.Instantiate(markerPrefab, overlayContainer));
            SetMarkerAsAvailable(marker);
        }
    }

    public void Dispose()
    {
        while (availableMarkers.Count > 0)
        {
            var marker = availableMarkers.Dequeue();
            marker.Dispose();
        }
        while (onUseMarkers.Count > 0)
        {
            var marker = onUseMarkers.Dequeue();
            marker.Dispose();
        }
    }

    public void SetExclusionArea(Vector2Int center, int area)
    {

    }

    public void SetMarkers(List<HotScenesController.HotSceneInfo> hotScenes)
    {

    }

    private void SetMarkerAsAvailable(UserPositionMarker marker)
    {
        availableMarkers.Enqueue(marker);
        marker.gameObject.SetActive(false);
    }

    private void SetMarkerAsUsed(UserPositionMarker marker, Vector2Int coords)
    {
        onUseMarkers.Enqueue(marker);
        SetMarkerAtCoord(marker, coords);
        marker.gameObject.SetActive(true);
    }

    private void SetMarkerAtCoord(UserPositionMarker marker, Vector2Int coords)
    {
        marker.gameObject.name = $"UsersPositionMarker({coords.x},{coords.y})";

        marker.gameObject.transform.localPosition = coordToMapPosition(
            coords.x + Random.Range(-0.5f, 0.5f),
            coords.y + Random.Range(-0.5f, 0.5f));

        marker.coords = coords;
    }
}
