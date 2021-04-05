using System;

internal interface ILandController
{ 
    event Action<LandData[]> OnLandsSet;
    void SetLands(LandData[] lands);
    void AddListener(ILandsListener listener);
    void RemoveListener(ILandsListener listener);
}

internal class LandController : ILandController
{
    public event Action<LandData[]> OnLandsSet;

    private LandData[] userLands = null;

    void ILandController.SetLands(LandData[] lands)
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