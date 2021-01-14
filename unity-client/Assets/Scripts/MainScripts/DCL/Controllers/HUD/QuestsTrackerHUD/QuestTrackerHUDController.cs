using System;
using UnityEngine;

public class QuestTrackerHUDController : IHUD
{
    private QuestTrackerHUDView view;

    public void Dispose()
    {
        throw new System.NotImplementedException();
    }

    public void Initialize()
    {
        view = QuestTrackerHUDView.Create();
    }

    public void SetVisibility(bool visible)
    {
        view?.gameObject.SetActive(visible);
    }
}

public class QuestTrackerHUDView : MonoBehaviour
{
    public static QuestTrackerHUDView Create()
    {
        throw new NotImplementedException();
    }
}