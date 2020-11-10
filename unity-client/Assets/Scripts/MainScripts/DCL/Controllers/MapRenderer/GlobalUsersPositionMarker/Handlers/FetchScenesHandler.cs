using System;
using System.Collections.Generic;
using UnityEngine;
using DCL.Interface;
using System.Collections;
using UpdateMode = MapGlobalUsersPositionMarkerController.UpdateMode;

internal class FetchScenesHandler : IDisposable
{
    public event Action<List<HotScenesController.HotSceneInfo>> OnScenesFetched;

    float initialIntevalTime;
    float backgroundIntervalTime;
    float foregroundIntervalTime;

    Coroutine updateCoroutine;
    UpdateMode updateMode;

    internal bool isFirstFetch;
    internal float updateInterval;

    public FetchScenesHandler(float initialIntevalTime, float foregroundIntervalTime, float backgroundIntervalTime)
    {
        this.initialIntevalTime = initialIntevalTime;
        this.backgroundIntervalTime = backgroundIntervalTime;
        this.foregroundIntervalTime = foregroundIntervalTime;
        this.updateInterval = initialIntevalTime;
    }

    public void Init()
    {
        if (updateCoroutine != null)
            return;

        this.updateInterval = initialIntevalTime;
        isFirstFetch = true;
        updateCoroutine = CoroutineStarter.Start(UpdateCoroutine());
    }

    public void SetUpdateMode(UpdateMode mode)
    {
        updateMode = mode;
        if (isFirstFetch)
            return;

        switch (updateMode)
        {
            case UpdateMode.BACKGROUND:
                updateInterval = backgroundIntervalTime;
                break;
            case UpdateMode.FOREGROUND:
                updateInterval = foregroundIntervalTime;
                break;
        }
    }

    public void Dispose()
    {
        CoroutineStarter.Stop(updateCoroutine);
        HotScenesController.i.OnHotSceneListFinishUpdating -= OnHotSceneListFinishUpdating;
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
                HotScenesController.i.OnHotSceneListFinishUpdating += OnHotSceneListFinishUpdating;
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
        OnHotScenesFetched(HotScenesController.i.hotScenesList);
    }

    private void OnHotScenesFetched(List<HotScenesController.HotSceneInfo> scenes)
    {
        HotScenesController.i.OnHotSceneListFinishUpdating -= OnHotSceneListFinishUpdating;

        bool fetchSuccess = scenes.Count > 0 && scenes[0].usersTotalCount > 0;

        if (!fetchSuccess)
            return;

        if (isFirstFetch)
        {
            isFirstFetch = false;
            SetUpdateMode(updateMode);
        }
        OnScenesFetched?.Invoke(scenes);
    }
}
