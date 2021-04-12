using System;

public enum BuildModeModalType
{
    EXIT,
    PUBLISH
}

public interface IBuildModeConfirmationModalController
{
    event Action<BuildModeModalType> OnCancelExit;
    event Action<BuildModeModalType> OnConfirmExit;

    void Initialize(IBuildModeConfirmationModalView exitFromBiWModalView);
    void Dispose();
    void SetActive(bool isActive, BuildModeModalType modalType);
    void SetTitle(string text);
    void SetSubTitle(string text);
    void SetCancelButtonText(string text);
    void SetConfirmButtonText(string text);
    void CancelExit();
    void ConfirmExit();
}

public class BuildModeConfirmationModalController : IBuildModeConfirmationModalController
{
    public event Action<BuildModeModalType> OnCancelExit;
    public event Action<BuildModeModalType> OnConfirmExit;

    internal IBuildModeConfirmationModalView exitFromBiWModalView;
    internal BuildModeModalType modalType;

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

    public void SetActive(bool isActive, BuildModeModalType modalType)
    {
        this.modalType = modalType;
        exitFromBiWModalView.SetActive(isActive);
    }

    public void SetTitle(string text) { exitFromBiWModalView.SetTitle(text); }

    public void SetSubTitle(string text) { exitFromBiWModalView.SetSubTitle(text); }

    public void SetCancelButtonText(string text) { exitFromBiWModalView.SetCancelButtonText(text); }

    public void SetConfirmButtonText(string text) { exitFromBiWModalView.SetConfirmButtonText(text); }

    public void CancelExit()
    {
        SetActive(false, modalType);
        OnCancelExit?.Invoke(modalType);
    }

    public void ConfirmExit()
    {
        SetActive(false, modalType);
        OnConfirmExit?.Invoke(modalType);
    }
}