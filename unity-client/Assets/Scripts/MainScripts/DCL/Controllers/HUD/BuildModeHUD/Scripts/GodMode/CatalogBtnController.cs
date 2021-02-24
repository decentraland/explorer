using UnityEngine.EventSystems;

public interface ICatalogBtnController
{
    event System.Action OnClick;

    void Initialize(CatalogBtnView catalogBtnView, ITooltipController tooltipController);
    void Dispose();
    void Click();
    void ShowTooltip(BaseEventData eventData, string tooltipText);
    void HideTooltip();
}

public class CatalogBtnController : ICatalogBtnController
{
    public event System.Action OnClick;

    private CatalogBtnView catalogBtnView;
    private ITooltipController tooltipController;

    public void Initialize(CatalogBtnView catalogBtnView, ITooltipController tooltipController)
    {
        this.catalogBtnView = catalogBtnView;
        this.tooltipController = tooltipController;

        catalogBtnView.OnCatalogButtonClick += Click;
        catalogBtnView.OnShowTooltip += ShowTooltip;
        catalogBtnView.OnHideTooltip += HideTooltip;
    }

    public void Dispose()
    {
        catalogBtnView.OnCatalogButtonClick -= Click;
        catalogBtnView.OnShowTooltip -= ShowTooltip;
        catalogBtnView.OnHideTooltip -= HideTooltip;
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