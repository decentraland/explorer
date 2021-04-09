using System;

public interface IBuildModeConfirmationModalController
{
    event Action OnCancelExit;
    event Action OnConfirmExit;

    void Initialize(IBuildModeConfirmationModalView exitFromBiWModalView);
    void Dispose();
    void SetActive(bool isActive);
    void SetTitle(string text);
    void SetSubTitle(string text);
    void CancelExit();
    void ConfirmExit();
}

public class BuildModeConfirmationModalController : IBuildModeConfirmationModalController
{
    public event Action OnCancelExit;
    public event Action OnConfirmExit;

    internal IBuildModeConfirmationModalView exitFromBiWModalView;

    public void Initialize(IBuildModeConfirmationModalView exitFromBiWModalView)
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

    public void SetTitle(string text) { exitFromBiWModalView.SetTitle(text); }

    public void SetSubTitle(string text) { exitFromBiWModalView.SetSubTitle(text); }

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