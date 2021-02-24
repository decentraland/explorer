public interface IPublishPopupController
{
    void Initialize(PublishPopupView publishPopupView);
    void Dispose();
    void PublishStart();
    void PublishEnd(string message);
}

public class PublishPopupController : IPublishPopupController
{
    private PublishPopupView publishPopupView;

    public void Initialize(PublishPopupView publishPopupView)
    {
        this.publishPopupView = publishPopupView;
    }

    public void Dispose()
    {
    }

    public void PublishStart()
    {
        publishPopupView.PublishStart();
    }

    public void PublishEnd(string message)
    {
        publishPopupView.PublishEnd(message);
    }
}
