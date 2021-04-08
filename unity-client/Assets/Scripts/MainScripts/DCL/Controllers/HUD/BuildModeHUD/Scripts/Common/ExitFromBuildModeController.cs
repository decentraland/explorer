using System;

public interface IExitFromBuildModeController
{
    event Action OnCancelExit;
    event Action OnConfirmExit;

    void Initialize(IExitFromBuildModeView exitFromBiWModalView);
    void Dispose();
    void SetActive(bool isActive);
    void CancelExit();
    void ConfirmExit();
}

public class ExitFromBuildModeController : IExitFromBuildModeController
{
    public event Action OnCancelExit;
    public event Action OnConfirmExit;

    internal IExitFromBuildModeView exitFromBiWModalView;

    public void Initialize(IExitFromBuildModeView exitFromBiWModalView)
    {
        this.exitFromBiWModalView = exitFromBiWModalView;

        exitFromBiWModalView.OnCancelExit += CancelExit;
        exitFromBiWModalView.OnConfirmExit += ConfirmExit;
    }

    public void Dispose()
    {
        exitFromBiWModalView.OnCancelExit -= CancelExit;
        exitFromBiWModalView.OnConfirmExit -= ConfirmExit;
    }

    public void SetActive(bool isActive) { exitFromBiWModalView.SetActive(isActive); }

    public void CancelExit()
    {
        SetActive(false);
        OnCancelExit?.Invoke();
    }

    public void ConfirmExit()
    {
        SetActive(false);
        OnConfirmExit?.Invoke();
    }
}