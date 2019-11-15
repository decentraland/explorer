using System;
using System.Collections.Generic;
using UnityEngine;
using DCL.Helpers;

public class MinimapMetadataController : MonoBehaviour
{
    private MinimapMetadata minimapMetadata => MinimapMetadata.GetMetadata();
    public static MinimapMetadataController i { get; private set; }

    [Serializable]
    private class MinimapSceneInfo
    {
        public string name;
        public int type;
        public List<int[]> parcels;
    }

    public void Awake()
    {
        i = this;
        minimapMetadata.Clear();
    }

    public void UpdateMinimapSceneInformation(string scenesInfoJson)
    {
        Debug.Log("received information");
        Debug.Log(scenesInfoJson);

        var scenesInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<MinimapSceneInfo[]>(scenesInfoJson);
        //var scenesInfo = Utils.ParseJsonArray<MinimapSceneInfo[]>(scenesInfoJson);
        Debug.Log("parsed information");
        foreach (var scene in scenesInfo)
        {
            foreach (var parcel in scene.parcels)
            {
                Debug.Log($"Set Tile: \n {parcel[0]},{parcel[1]}  {scene.type} {scene.name}");
                minimapMetadata.SetTile(parcel[0], parcel[1], new MinimapMetadata.Tile(scene.type, scene.name));
            }
        }
    }
}