using UnityEngine.EventSystems;

public interface ITooltipController
{
    void Initialize(ITooltipView view);
    void SetTooltipText(string text);
    void ShowTooltip(BaseEventData data);
    void HideTooltip();
}

public class TooltipController : ITooltipController
{
    private ITooltipView view;

    public void Initialize(ITooltipView view)
    {
        this.view = view;
    }

    public void SetTooltipText(string text)
    {
        view.SetText(text);
    }

    public void ShowTooltip(BaseEventData data)
    {
        view.OnHoverEnter(data);
    }

    public void HideTooltip()
    {
        view.OnHoverExit();
    }
}
