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
    public List<Vector2Int> Filter(List<HotScenesController.HotSceneInfo> hotScenesList, int maxMarkers)
    {
        List<Vector2Int> result = new List<Vector2Int>(maxMarkers);
        List<Vector2Int> rawParcelCoords = GetRawParcelCoords(hotScenesList);
        float stepAmount = rawParcelCoords.Count / (float)maxMarkers;
        if (stepAmount < 1) stepAmount = 1;

        float lastIndex = -1;
        for (float step = 0; step < rawParcelCoords.Count; step += stepAmount)
        {
            if ((step - lastIndex) >= 1)
            {
                lastIndex = step;
                result.Add(rawParcelCoords[(int)lastIndex]);

                if (result.Count >= maxMarkers)
                    break;
            }
        }

        return result;
    }

    private List<Vector2Int> GetRawParcelCoords(List<HotScenesController.HotSceneInfo> hotScenesList)
    {
        List<Vector2Int> result = new List<Vector2Int>();

        HotScenesController.HotSceneInfo sceneInfo;
        int scenesCount = hotScenesList.Count;
        for (int sceneIdx = 0; sceneIdx < scenesCount; sceneIdx++)
        {
            sceneInfo = hotScenesList[sceneIdx];
            if (sceneInfo.usersTotalCount <= 0) continue;

            for (int realmIdx = 0; realmIdx < sceneInfo.realms.Length; realmIdx++)
            {
                for (int parcelIdx = 0; parcelIdx < sceneInfo.realms[realmIdx].userParcels.Length; parcelIdx++)
                {
                    result.Add(sceneInfo.realms[realmIdx].userParcels[parcelIdx]);
                }
            }
        }
        return result;
    }
}