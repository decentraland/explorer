using UnityEngine.EventSystems;

public interface IFirstPersonModeController
{
    void Initialize(FirstPersonModeView firstPersonModeView, ITooltipController tooltipController);
    void Dispose();
    void ShowTooltip(BaseEventData eventData, string tooltipText);
    void HideTooltip();
}

public class FirstPersonModeController : IFirstPersonModeController
{
    private FirstPersonModeView firstPersonModeView;
    private ITooltipController tooltipController;

    public void Initialize(FirstPersonModeView firstPersonModeView, ITooltipController tooltipController)
    {
        this.firstPersonModeView = firstPersonModeView;
        this.tooltipController = tooltipController;

        firstPersonModeView.OnShowTooltip += ShowTooltip;
        firstPersonModeView.OnHideTooltip += HideTooltip;
    }

    public void Dispose()
    {
        firstPersonModeView.OnShowTooltip -= ShowTooltip;
        firstPersonModeView.OnHideTooltip -= HideTooltip;
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
