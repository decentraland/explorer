using System;
using System.Collections.Generic;
using DCL.Helpers;
using UnityEngine;

internal interface ILandController : IDisposable
{ 
    event Action<List<Land>> OnLandsSet;
    void SetLands(List<Land> lands);
    void AddListener(ILandsListener listener);
    void RemoveListener(ILandsListener listener);
    void FetchLands();
}

internal class LandController : ILandController
{
    public event Action<List<Land>> OnLandsSet;

    private List<Land> userLands = null;
    private Promise<List<Land>> landQueryPromise;
    
    private readonly ITheGraph theGraphService;

    public LandController(ITheGraph theGraphService)
    {
        this.theGraphService = theGraphService;
    }

    public void Dispose()
    {
        landQueryPromise?.Dispose();
    }

    void ILandController.FetchLands()
    {
        var address = UserProfile.GetOwnUserProfile().ethAddress;

#if UNITY_EDITOR
        // NOTE: to be able to test in editor without getting a profile we hardcode an address here
        address = !string.IsNullOrEmpty(address) ? address : "0x421d69294ce3d86ff40ca35174ad32fe82f41d05";
#endif

        if (string.IsNullOrEmpty(address))
            return;

        landQueryPromise = theGraphService.QueryLands(KernelConfig.i.Get().tld, address, TheGraphCache.UseCache);
        landQueryPromise
            .Then(landList =>
            {
                for (int i = 0; i < landList.Count; i++)
                {
                    if (landList[i].type == LandType.PARCEL)
                        continue;
                    landList[i].x = landList[i].parcels[0].x;
                    landList[i].y = landList[i].parcels[0].y;
                }
                ((ILandController)this).SetLands(landList);
            })
            .Catch(Debug.LogError);
    }

    void ILandController.SetLands(List<Land> lands)
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