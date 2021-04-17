using System;
using System.Collections.Generic;
using DCL.Helpers;
using UnityEngine;

internal interface ILandController
{ 
    event Action<List<LandWithAccess>> OnLandsSet;
    void SetLands(List<LandWithAccess> lands);
    void AddListener(ILandsListener listener);
    void RemoveListener(ILandsListener listener);
}

internal class LandController : ILandController
{
    public event Action<List<LandWithAccess>> OnLandsSet;

    private List<LandWithAccess> userLands = null;

    void ILandController.SetLands(List<LandWithAccess> lands)
    {
        userLands = lands;
        OnLandsSet?.Invoke(lands);
    }

    void ILandController.AddListener(ILandsListener listener)
    {
        OnLandsSet += listener.OnSetLands;
        listener.OnSetLands(userLands);
    }

    void ILandController.RemoveListener(ILandsListener listener)
    {
        OnLandsSet -= listener.OnSetLands;
    }
}