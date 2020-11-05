using System;
using System.Collections.Generic;
using UnityEngine;

internal class UserPositionMarkersHandler : IDisposable
{
    Queue<UserPositionMarker> availableMarkers;
    Queue<UserPositionMarker> onUseMarkers;
    int maxMarkers;

    public UserPositionMarkersHandler(GameObject markerPrefab, Transform overlayContainer, int maxMarkers)
    {
        this.maxMarkers = maxMarkers;
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
        marker.SetActive(false);
    }

    private void SetMarkerAsUsed(UserPositionMarker marker)
    {
        onUseMarkers.Enqueue(marker);
        marker.SetActive(false);
    }
}
