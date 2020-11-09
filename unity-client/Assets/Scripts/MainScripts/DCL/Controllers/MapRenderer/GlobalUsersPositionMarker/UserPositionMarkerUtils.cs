using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class ExclusionArea
{
    public Vector2Int position;
    public int area;

    public bool Contains(Vector2Int coords)
    {
        return (coords - position).sqrMagnitude <= area * area;
    }
}

internal class ScenesFilter
{
    public List<Vector2Int> Filter(List<HotScenesController.HotSceneInfo> hotScenes, int maxMarkers)
    {
        List<Vector2Int> result = new List<Vector2Int>(maxMarkers);
        List<Vector2Int> rawParcelCoords = GetRawParcelCoords(hotScenes);
        int step = rawParcelCoords.Count / maxMarkers;
        if (step < 1) step = 1;

        for (int i = 0; i < rawParcelCoords.Count; i += step)
        {
            result.Add(rawParcelCoords[i]);
        }

        return result;
    }

    private List<Vector2Int> GetRawParcelCoords(List<HotScenesController.HotSceneInfo> hotScenes)
    {
        List<Vector2Int> result = new List<Vector2Int>();

        HotScenesController.HotSceneInfo sceneInfo;
        for (int sceneIdx = 0; sceneIdx < HotScenesController.i.hotScenesList.Count; sceneIdx++)
        {
            sceneInfo = HotScenesController.i.hotScenesList[sceneIdx];
            if (sceneInfo.usersTotalCount <= 0) continue;

            for (int realmIdx = 0; realmIdx < sceneInfo.realms.Length; realmIdx++)
            {
                for (int parcelIdx = 0; parcelIdx < sceneInfo.realms[realmIdx].crowdedParcels.Length; parcelIdx++)
                {
                    result.Add(sceneInfo.realms[realmIdx].crowdedParcels[parcelIdx]);
                }
            }
        }
        return result;
    }
}