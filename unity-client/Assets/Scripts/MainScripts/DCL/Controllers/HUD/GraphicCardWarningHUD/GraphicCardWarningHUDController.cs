
public class GraphicCardWarningHUDController : IHUD
{
    private const string GRAPHIC_CARD_MESSAGE = "An integrated Graphic Card has been detected.\nYou might encounter performance issues";

    [System.Serializable]
    public class Model
    {
        public string graphicCardVendor;
    }

    private Model model;

    public GraphicCardWarningHUDController() { }

    public GraphicCardWarningHUDController(Model model)
    {
        this.model = model;
    }

    public void SetVisibility(bool visible)
    {
        if (visible)
        {
            NotificationsController.i.ShowNotification(new Notification.Model
            {
                buttonMessage = "Dismish",
                destroyOnFinish = true,
                groupID = "GraphicCard",
                message = GRAPHIC_CARD_MESSAGE,
                timer = 0,
                type = NotificationFactory.Type.GRAPHIC_CARD_WARNING
            });
        }
    }

    public void Dispose()
    {

    }
}
