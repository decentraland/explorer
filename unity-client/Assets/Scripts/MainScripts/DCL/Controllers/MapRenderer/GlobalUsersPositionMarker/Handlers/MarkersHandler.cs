﻿using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

internal class MarkersHandler : IDisposable
{
    internal readonly List<UserPositionMarker> availableMarkers;
    internal readonly List<UserPositionMarker> usedMarkers;
    readonly Func<float, float, Vector3> coordToMapPosition;

    readonly ExclusionArea exclusionArea;
    readonly ScenesFilter scenesFilter;

    int maxMarkers;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="markerPrefab">prefab for markers</param>
    /// <param name="overlayContainer">parent for markers</param>
    /// <param name="maxMarkers">max amount of markers (pool)</param>
    /// <param name="coordToMapPosFunc">function to transform coords to map position</param>
    public MarkersHandler(GameObject markerPrefab, Transform overlayContainer, int maxMarkers, Func<float, float, Vector3> coordToMapPosFunc)
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

    /// <summary>
    /// Set exclusion area. Markers inside this area will be hidden, to avoid overlapping with markers set with comms info for example.
    /// After set it will iterate through current markers to hide or show them respectively.
    /// </summary>
    /// <param name="center">center of the exclusion area</param>
    /// <param name="area">size of the exclusion area</param>
    public void SetExclusionArea(Vector2Int center, int area)
    {
        exclusionArea.position = center;
        exclusionArea.area = area;
        ApplyExclusionArea();
    }

    /// <summary>
    /// Set scenes to set markers to. Scenes will be filtered and it coordinates will be extracted.
    /// Then markers will be set and will be shown or hidden according to the current exclusion area.
    /// </summary>
    /// <param name="hotScenes">list of populated scenes</param>
    public void SetMarkers(List<HotScenesController.HotSceneInfo> hotScenes)
    {
        var parcelList = scenesFilter.Filter(hotScenes, maxMarkers);
        ResfreshMarkersPoolLists(parcelList.Count);
        for (int i = 0; i < parcelList.Count && i < usedMarkers.Count; i++)
        {
            SetMarker(usedMarkers[i], parcelList[i]);
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
            int addAmount = amountToBeUsed - usedMarkers.Count;
            for (int i = 0; i < addAmount && i < availableMarkers.Count; i++)
            {
                var marker = availableMarkers[i];
                availableMarkers.RemoveAt(i);
                usedMarkers.Add(marker);
            }
        }
        else if (amountToBeUsed < usedMarkers.Count)
        {
            int removeAmount = usedMarkers.Count - amountToBeUsed;
            for (int i = 0; i < removeAmount && i < usedMarkers.Count; i++)
            {
                var marker = usedMarkers[i];
                usedMarkers.RemoveAt(i);
                marker.gameObject.SetActive(false);
                availableMarkers.Add(marker);
            }
        }
    }
}
