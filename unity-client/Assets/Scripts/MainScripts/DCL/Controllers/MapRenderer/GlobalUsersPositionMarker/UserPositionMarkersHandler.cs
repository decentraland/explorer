using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

internal class UserPositionMarkersHandler : IDisposable
{
    readonly List<UserPositionMarker> availableMarkers;
    readonly List<UserPositionMarker> usedMarkers;
    readonly Func<float, float, Vector3> coordToMapPosition;

    readonly ExclusionArea exclusionArea;
    readonly ScenesFilter scenesFilter;

    int maxMarkers;

    public UserPositionMarkersHandler(GameObject markerPrefab, Transform overlayContainer, int maxMarkers, Func<float, float, Vector3> coordToMapPosFunc)
    {
        this.maxMarkers = maxMarkers;
        this.coordToMapPosition = coordToMapPosFunc;

        exclusionArea = new ExclusionArea();
        scenesFilter = new ScenesFilter();

        availableMarkers = new List<UserPositionMarker>(maxMarkers);
        usedMarkers = new List<UserPositionMarker>(maxMarkers);

        for (int i = 0; i < maxMarkers; i++)
        {
            var marker = new UserPositionMarker(GameObject.Instantiate(markerPrefab, overlayContainer));
            availableMarkers.Add(marker);
            marker.gameObject.SetActive(false);
        }
    }

    public void Dispose()
    {
        for (int i = 0; i < availableMarkers.Count; i++)
        {
            availableMarkers[i].Dispose();
        }
        for (int i = 0; i < usedMarkers.Count; i++)
        {
            usedMarkers[i].Dispose();
        }
        availableMarkers.Clear();
        usedMarkers.Clear();
    }

    public void SetExclusionArea(Vector2Int center, int area)
    {
        exclusionArea.position = center;
        exclusionArea.area = area;
        ApplyExclusionArea();
    }

    public void SetMarkers(List<HotScenesController.HotSceneInfo> hotScenes)
    {
        var parcelList = scenesFilter.Filter(hotScenes, maxMarkers);
        ResfreshMarkersPoolLists(parcelList.Count);
        for (int i = 0; i < parcelList.Count && i < usedMarkers.Count; i++)
        {
            SetMarker(usedMarkers[i], parcelList[i]);
        }
    }

    private void SetMarker(UserPositionMarker marker, Vector2Int coords)
    {
        SetMarkerAtCoord(marker, coords);
        marker.gameObject.SetActive(!exclusionArea.Contains(coords));
    }

    private void SetMarkerAtCoord(UserPositionMarker marker, Vector2Int coords)
    {
        marker.gameObject.name = $"UsersPositionMarker({coords.x},{coords.y})";

        marker.gameObject.transform.localPosition = coordToMapPosition(
            coords.x + Random.Range(-0.5f, 0.5f),
            coords.y + Random.Range(-0.5f, 0.5f));

        marker.coords = coords;
    }

    private void ApplyExclusionArea()
    {
        if (usedMarkers.Count == 0)
        {
            return;
        }

        using (var iterator = usedMarkers.GetEnumerator())
        {
            while (iterator.MoveNext())
            {
                iterator.Current.gameObject.SetActive(!exclusionArea.Contains(iterator.Current.coords));
            }
        }
    }

    private void ResfreshMarkersPoolLists(int amountToBeUsed)
    {
        if (amountToBeUsed > usedMarkers.Count)
        {
            for (int i = 0; i < amountToBeUsed - usedMarkers.Count && i < availableMarkers.Count; i++)
            {
                var marker = availableMarkers[i];
                availableMarkers.RemoveAt(i);
                usedMarkers.Add(marker);
            }
        }
        else if (amountToBeUsed < usedMarkers.Count)
        {
            for (int i = 0; i < usedMarkers.Count - amountToBeUsed && i < usedMarkers.Count; i++)
            {
                var marker = usedMarkers[i];
                usedMarkers.RemoveAt(i);
                marker.gameObject.SetActive(false);
                availableMarkers.Add(marker);
            }
        }
    }
}
