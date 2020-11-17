﻿
public class GraphicCardWarningHUDController : IHUD
{
    private const string GRAPHIC_CARD_MESSAGE = "An integrated graphics card has been detected on your machine.\nYou may encounter performance issues as a result.";

    public GraphicCardWarningHUDController() { }

    public void SetVisibility(bool visible)
    {
        CommonScriptableObjects.rendererState.OnChange -= RendererStateChanged;

        if (!visible)
            return;

        if (CommonScriptableObjects.rendererState)
            TryShowNotification();
        else
            CommonScriptableObjects.rendererState.OnChange += RendererStateChanged;

    }

    private void RendererStateChanged(bool newState, bool oldState)
    {
        if (!newState) return;

        CommonScriptableObjects.rendererState.OnChange -= RendererStateChanged;
        TryShowNotification();
    }

    private void TryShowNotification()
    {
        if (IsIntegratedGraphicCard())
        {
            NotificationsController.i.ShowNotification(new Notification.Model
            {
                buttonMessage = "Dismiss",
                destroyOnFinish = true,
                groupID = "GraphicCard",
                message = GRAPHIC_CARD_MESSAGE,
                timer = 0,
                type = NotificationFactory.Type.GRAPHIC_CARD_WARNING
            });
        }
    }

    private bool IsIntegratedGraphicCard() => DCL.Interface.WebInterface.GetGraphicCard().ToLower().Contains("intel");

    public void Dispose() { }
}
