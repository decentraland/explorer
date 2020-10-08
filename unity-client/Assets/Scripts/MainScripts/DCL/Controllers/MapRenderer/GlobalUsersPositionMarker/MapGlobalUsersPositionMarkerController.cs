using System;
using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.Configuration;
using DCL.Helpers;
using DCL.Interface;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class MapGlobalUsersPositionMarkerController : IDisposable
{
    const string POOL_NAME = "GlobalUsersPositionMarkerPool";
    const int POOL_PREWARM = 30;

    private const float UPDATE_INTERVAL = 60f;
    private const float UPDATE_INTERVAL_BACKGROUND = 5*60f;

    private readonly Pool markersPool;
    private readonly Transform overlayContainer;
    private readonly Func<float, float, Vector3> coordToMapPosition;

    private float updateInterval = UPDATE_INTERVAL_BACKGROUND;
    private Coroutine updateCoroutine;

    public MapGlobalUsersPositionMarkerController(GameObject markerPrefab, Transform overlayContainer, Func<float, float, Vector3> coordToMapPosFunc)
    {
        this.overlayContainer = overlayContainer;
        this.coordToMapPosition = coordToMapPosFunc;

        markersPool = PoolManager.i.AddPool(
            POOL_NAME,
            Object.Instantiate(markerPrefab, overlayContainer),
            maxPrewarmCount: POOL_PREWARM,
            isPersistent: true);

        if (!EnvironmentSettings.RUNNING_TESTS)
            markersPool.ForcePrewarm();
    }

    public void Enable()
    {
        updateCoroutine = CoroutineStarter.Start(UpdateCoroutine());

        HotScenesController.i.OnHotSceneListFinishUpdating -= OnHotSceneListFinishUpdating;
        HotScenesController.i.OnHotSceneListFinishUpdating += OnHotSceneListFinishUpdating;
    }

    public void Disable()
    {
        HotScenesController.i.OnHotSceneListFinishUpdating -= OnHotSceneListFinishUpdating;

        if (updateCoroutine != null)
        {
            CoroutineStarter.Stop(updateCoroutine);
            updateCoroutine = null;
        }
    }

    public void SetInForeground()
    {
        updateInterval = UPDATE_INTERVAL;
    }

    public void SetInBackground()
    {
        updateInterval = UPDATE_INTERVAL_BACKGROUND;
    }

    public void Dispose()
    {
        Disable();
        markersPool?.Cleanup();
        PoolManager.i.RemovePool(POOL_NAME);
    }

    private IEnumerator UpdateCoroutine()
    {
        while (true)
        {
            float time = Time.realtimeSinceStartup;

            while (Time.realtimeSinceStartup - time < updateInterval)
            {
                yield return null;
            }

            if (HotScenesController.i.timeSinceLastUpdate > updateInterval)
            {
                WebInterface.FetchHotScenes();
            }
            else
            {
                OnHotSceneListFinishUpdating();
            }
        }
    }

    private void OnHotSceneListFinishUpdating()
    {
        markersPool.ReleaseAll();

        HotScenesController.HotSceneInfo sceneInfo;
        for (int sceneIdx = 0; sceneIdx < HotScenesController.i.hotScenesList.Count; sceneIdx++)
        {
            sceneInfo = HotScenesController.i.hotScenesList[sceneIdx];
            if (sceneInfo.usersTotalCount <= 0) continue;

            for (int realmIdx = 0; realmIdx < sceneInfo.realms.Length; realmIdx++)
            {
                for (int parcelIdx = 0; parcelIdx < sceneInfo.realms[realmIdx].crowdedParcels.Length; parcelIdx++)
                {
                    SetMarkerAtCoord(sceneInfo.realms[realmIdx].crowdedParcels[parcelIdx]);
                }
            }
        }
    }

    private void SetMarkerAtCoord(Vector2Int coord)
    {
        PoolableObject marker = markersPool.Get();
        marker.gameObject.name = $"UsersPositionMarker({coord.x},{coord.y})";
        marker.gameObject.transform.SetParent(overlayContainer, true);
        marker.gameObject.transform.localScale = Vector3.one;
        marker.gameObject.transform.localPosition = coordToMapPosition(
            coord.x + Random.Range(-0.5f,0.5f),
            coord.y + Random.Range(-0.5f,0.5f));
    }
}
