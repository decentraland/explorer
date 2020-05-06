using UnityEngine;

public class NotificationHUDView : MonoBehaviour
{
    public NotificationFactory notificationFactory;

    [SerializeField]
    private RectTransform notificationPanel;

    public event System.Action<Notification> OnNotificationDismissedEvent;

    private const string VIEW_PATH = "NotificationHUD";
    private const string VIEW_OBJECT_NAME = "_NotificationHUD";

    internal static NotificationHUDView Create()
    {
        NotificationHUDView view = Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<NotificationHUDView>();
        view.Initialize();
        return view;
    }

    private void Initialize()
    {
        gameObject.name = VIEW_OBJECT_NAME;
    }

    public void ShowNotification(Notification notification)
    {
        if (notification == null)
            return;

        notification.OnNotificationDismissed += OnNotificationDismissed;
        notification.Initialize(notification.model);
    }

    public Notification ShowNotification(Notification.Model notificationModel)
    {
        if (notificationModel == null)
            return null;

        Notification notification = notificationFactory.CreateNotificationFromType(notificationModel.type, notificationPanel);
        ShowNotification(notification);
        return notification;
    }

    private void OnNotificationDismissed(Notification n)
    {
        OnNotificationDismissedEvent?.Invoke(n);
        Destroy(n.gameObject);
    }

    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }
}
