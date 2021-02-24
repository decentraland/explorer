using UnityEngine.EventSystems;

public interface IFirstPersonModeController
{
    event System.Action OnClick;

    void Initialize(FirstPersonModeView firstPersonModeView, ITooltipController tooltipController);
    void Dispose();
    void Click();
    void ShowTooltip(BaseEventData eventData, string tooltipText);
    void HideTooltip();
}

public class FirstPersonModeController : IFirstPersonModeController
{
    public event System.Action OnClick;

    private FirstPersonModeView firstPersonModeView;
    private ITooltipController tooltipController;

    public void Initialize(FirstPersonModeView firstPersonModeView, ITooltipController tooltipController)
    {
        this.firstPersonModeView = firstPersonModeView;
        this.tooltipController = tooltipController;

        firstPersonModeView.OnFirstPersonModeClick += Click;
        firstPersonModeView.OnShowTooltip += ShowTooltip;
        firstPersonModeView.OnHideTooltip += HideTooltip;
    }

    public void Dispose()
    {
        firstPersonModeView.OnFirstPersonModeClick -= Click;
        firstPersonModeView.OnShowTooltip -= ShowTooltip;
        firstPersonModeView.OnHideTooltip -= HideTooltip;
    }

    public void Click()
    {
        OnClick?.Invoke();
    }

    public void ShowTooltip(BaseEventData eventData, string tooltipText)
    {
        tooltipController.ShowTooltip(eventData);
        tooltipController.SetTooltipText(tooltipText);
    }

    public void HideTooltip()
    {
        tooltipController.HideTooltip();
    }
}
