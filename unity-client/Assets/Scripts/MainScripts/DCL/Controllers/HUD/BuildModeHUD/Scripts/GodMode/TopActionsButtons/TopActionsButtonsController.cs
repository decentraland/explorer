using System;

public interface ITopActionsButtonsController
{
    event Action OnChangeModeClick,
                 OnExtraClick,
                 OnTranslateClick,
                 OnRotateClick,
                 OnScaleClick,
                 OnResetClick,
                 OnDuplicateClick,
                 OnDeleteClick,
                 OnLogOutClick;

    void Initialize(TopActionsButtonsView topActionsButtonsView);
    void Dispose();
}

public class TopActionsButtonsController : ITopActionsButtonsController
{
    public event Action OnChangeModeClick,
                        OnExtraClick,
                        OnTranslateClick,
                        OnRotateClick,
                        OnScaleClick,
                        OnResetClick,
                        OnDuplicateClick,
                        OnDeleteClick,
                        OnLogOutClick;

    private TopActionsButtonsView topActionsButtonsView;

    public void Initialize(TopActionsButtonsView topActionsButtonsView)
    {
        this.topActionsButtonsView = topActionsButtonsView;

        topActionsButtonsView.OnChangeModeClicked += ChangeModeClicked;
        topActionsButtonsView.OnExtraClicked += ExtraClicked;
        topActionsButtonsView.OnTranslateClicked += TranslateClicked;
        topActionsButtonsView.OnRotateClicked += RotateClicked;
        topActionsButtonsView.OnScaleClicked += ScaleClicked;
        topActionsButtonsView.OnResetClicked += ResetClicked;
        topActionsButtonsView.OnDuplicateClicked += DuplicateClicked;
        topActionsButtonsView.OnDeleteClicked += DeleteClicked;
        topActionsButtonsView.OnLogOutClicked += LogOutClicked;
    }

    public void Dispose()
    {
        topActionsButtonsView.OnChangeModeClicked -= ChangeModeClicked;
        topActionsButtonsView.OnExtraClicked -= ExtraClicked;
        topActionsButtonsView.OnTranslateClicked -= TranslateClicked;
        topActionsButtonsView.OnRotateClicked -= RotateClicked;
        topActionsButtonsView.OnScaleClicked -= ScaleClicked;
        topActionsButtonsView.OnResetClicked -= ResetClicked;
        topActionsButtonsView.OnDuplicateClicked -= DuplicateClicked;
        topActionsButtonsView.OnDeleteClicked -= DeleteClicked;
        topActionsButtonsView.OnLogOutClicked -= LogOutClicked;
    }

    private void ChangeModeClicked()
    {
        OnChangeModeClick?.Invoke();
    }

    private void ExtraClicked()
    {
        OnExtraClick?.Invoke();
    }

    private void TranslateClicked()
    {
        OnTranslateClick?.Invoke();
    }

    private void RotateClicked()
    {
        OnRotateClick?.Invoke();
    }

    private void ScaleClicked()
    {
        OnScaleClick?.Invoke();
    }

    private void ResetClicked()
    {
        OnResetClick?.Invoke();
    }

    private void DuplicateClicked()
    {
        OnDuplicateClick?.Invoke();
    }

    private void DeleteClicked()
    {
        OnDeleteClick?.Invoke();
    }

    private void LogOutClicked()
    {
        OnLogOutClick?.Invoke();
    }
}
