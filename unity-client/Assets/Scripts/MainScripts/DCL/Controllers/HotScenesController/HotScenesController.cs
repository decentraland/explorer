using System;
using System.Collections.Generic;
using DCL.Helpers;
using UnityEngine;

public class HotScenesController : MonoBehaviour
{
    public static HotScenesController i { get; private set; }

    public event Action OnHotSceneListFinishUpdating;
    public event Action<HotSceneInfo[]> OnHotSceneListChunkUpdate;

    public List<HotSceneInfo> hotScenesList { get; private set; }
    public bool isUpdating { get; private set; }
    public float timeSinceLastUpdate { get { return Time.realtimeSinceStartup - lastUpdateTime; } }

    private List<HotSceneInfo> tempHotScenesList = new List<HotSceneInfo>();
    private float lastUpdateTime = float.MinValue * .5f;

    [Serializable]
    public struct HotSceneInfo
    {
        [Serializable]
        public struct Realm
        {
            public string serverName;
            public string layer;
            public int usersCount;
            public int usersMax;
        }
        public Vector2Int baseCoords;
        public int usersTotalCount;
        public Realm[] realms;
    }

    void Awake()
    {
        i = this;
    }

    public void UpdateHotScenesList(string json)
    {
        isUpdating = true;
        var hotScenes = Utils.ParseJsonArray<HotSceneInfo[]>(json);
        tempHotScenesList.AddRange(hotScenes);
        OnHotSceneListChunkUpdate?.Invoke(hotScenes);
    }

    public void FinishUpdateHotScenesList()
    {
        isUpdating = false;
        lastUpdateTime = Time.realtimeSinceStartup;
        hotScenesList = tempHotScenesList;
        tempHotScenesList = new List<HotSceneInfo>();
        OnHotSceneListFinishUpdating?.Invoke();
    }
}
