using System;
using UnityEngine.EventSystems;

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

    void Initialize(ITopActionsButtonsView topActionsButtonsView, ITooltipController tooltipController);
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
    void TooltipPointerEntered(BaseEventData eventData, string tooltipText);
    void TooltipPointerExited();
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

    public IExtraActionsController extraActionsController { get; private set; }

    private ITopActionsButtonsView topActionsButtonsView;
    private ITooltipController tooltipController;

    public void Initialize(ITopActionsButtonsView topActionsButtonsView, ITooltipController tooltipController)
    {
        this.topActionsButtonsView = topActionsButtonsView;
        this.tooltipController = tooltipController;

        topActionsButtonsView.OnChangeModeClicked += ChangeModeClicked;
        topActionsButtonsView.OnExtraClicked += ExtraClicked;
        topActionsButtonsView.OnTranslateClicked += TranslateClicked;
        topActionsButtonsView.OnRotateClicked += RotateClicked;
        topActionsButtonsView.OnScaleClicked += ScaleClicked;
        topActionsButtonsView.OnResetClicked += ResetClicked;
        topActionsButtonsView.OnDuplicateClicked += DuplicateClicked;
        topActionsButtonsView.OnDeleteClicked += DeleteClicked;
        topActionsButtonsView.OnLogOutClicked += LogOutClicked;
        topActionsButtonsView.OnPointerExit += TooltipPointerExited;
        topActionsButtonsView.OnChangeCameraModePointerEnter += TooltipPointerEntered;
        topActionsButtonsView.OnTranslatePointerEnter += TooltipPointerEntered;
        topActionsButtonsView.OnRotatePointerEnter += TooltipPointerEntered;
        topActionsButtonsView.OnScalePointerEnter += TooltipPointerEntered;
        topActionsButtonsView.OnResetPointerEnter += TooltipPointerEntered;
        topActionsButtonsView.OnDuplicatePointerEnter += TooltipPointerEntered;
        topActionsButtonsView.OnDeletePointerEnter += TooltipPointerEntered;
        topActionsButtonsView.OnMoreActionsPointerEnter += TooltipPointerEntered;
        topActionsButtonsView.OnLogoutPointerEnter += TooltipPointerEntered;

        extraActionsController = new ExtraActionsController();
        topActionsButtonsView.ConfigureExtraActions(extraActionsController);
        extraActionsController.SetActive(false);
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
        topActionsButtonsView.OnPointerExit -= TooltipPointerExited;
        topActionsButtonsView.OnChangeCameraModePointerEnter -= TooltipPointerEntered;
        topActionsButtonsView.OnTranslatePointerEnter -= TooltipPointerEntered;
        topActionsButtonsView.OnRotatePointerEnter -= TooltipPointerEntered;
        topActionsButtonsView.OnScalePointerEnter -= TooltipPointerEntered;
        topActionsButtonsView.OnResetPointerEnter -= TooltipPointerEntered;
        topActionsButtonsView.OnDuplicatePointerEnter -= TooltipPointerEntered;
        topActionsButtonsView.OnDeletePointerEnter -= TooltipPointerEntered;
        topActionsButtonsView.OnMoreActionsPointerEnter -= TooltipPointerEntered;
        topActionsButtonsView.OnLogoutPointerEnter -= TooltipPointerEntered;
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

    public void TooltipPointerEntered(BaseEventData eventData, string tooltipText)
    {
        tooltipController.ShowTooltip(eventData);
        tooltipController.SetTooltipText(tooltipText);
    }

    public void TooltipPointerExited()
    {
        tooltipController.HideTooltip();
    }
}
