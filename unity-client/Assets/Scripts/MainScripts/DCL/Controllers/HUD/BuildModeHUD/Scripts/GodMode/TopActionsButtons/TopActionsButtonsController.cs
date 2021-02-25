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

    IExtraActionsController extraActionsController { get; }

    void Initialize(TopActionsButtonsView topActionsButtonsView);
    void Dispose();

    void ChangeModeClicked();
    void ExtraClicked();
    void TranslateClicked();
    void RotateClicked();
    void ScaleClicked();
    void ResetClicked();
    void DuplicateClicked();
    void DeleteClicked();
    void LogOutClicked();
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

    public IExtraActionsController extraActionsController => topActionsButtonsView.extraActionsController;

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

        topActionsButtonsView.ConfigureExtraActions(new ExtraActionsController());
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

    public void ChangeModeClicked()
    {
        OnChangeModeClick?.Invoke();
    }

    public void ExtraClicked()
    {
        OnExtraClick?.Invoke();
    }

    public void TranslateClicked()
    {
        OnTranslateClick?.Invoke();
    }

    public void RotateClicked()
    {
        OnRotateClick?.Invoke();
    }

    public void ScaleClicked()
    {
        OnScaleClick?.Invoke();
    }

    public void ResetClicked()
    {
        OnResetClick?.Invoke();
    }

    public void DuplicateClicked()
    {
        OnDuplicateClick?.Invoke();
    }

    public void DeleteClicked()
    {
        OnDeleteClick?.Invoke();
    }

    public void LogOutClicked()
    {
        OnLogOutClick?.Invoke();
    }
}
