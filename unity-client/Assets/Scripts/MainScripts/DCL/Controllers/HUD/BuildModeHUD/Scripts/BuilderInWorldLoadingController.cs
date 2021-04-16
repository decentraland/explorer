using System;

public interface IBuilderInWorldLoadingController
{
    event System.Action OnCancelLoading;

    void Initialize(IBuilderInWorldLoadingView initialLoadingView);
    void Dispose();
    void Show();
    void Hide(bool forzeHidding = false, Action onHideAction = null);
    void CancelLoading();
    void SetPercentage(float newValue);
}

public class BuilderInWorldLoadingController : IBuilderInWorldLoadingController
{
    public event System.Action OnCancelLoading;

    internal IBuilderInWorldLoadingView initialLoadingView;

    public void Initialize(IBuilderInWorldLoadingView initialLoadingView)
    {
        this.initialLoadingView = initialLoadingView;
        this.initialLoadingView.OnCancelLoading += CancelLoading;
    }

    public void Dispose()
    {
        initialLoadingView.StopTipsCarousel();
        initialLoadingView.OnCancelLoading -= CancelLoading;
    }

    public void Show() { initialLoadingView.Show(); }

    public void Hide(bool forzeHidding = false, Action onHideAction = null) { initialLoadingView.Hide(forzeHidding, onHideAction); }

    public void CancelLoading() { OnCancelLoading?.Invoke(); }

    public void SetPercentage(float newValue) { initialLoadingView.SetPercentage(newValue); }
}