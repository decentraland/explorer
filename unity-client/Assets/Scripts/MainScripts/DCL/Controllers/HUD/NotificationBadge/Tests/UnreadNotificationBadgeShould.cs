using DCL.Interface;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

public class UnreadNotificationBadgeShould : TestsBase
{
    private const string UNREAD_NOTIFICATION_BADGE_RESOURCE_NAME = "UnreadNotificationBadge";
    private const string TEST_USER_ID = "testFriend";

    private ChatController_Mock chatController;
    private UnreadNotificationBadge unreadNotificationBadge;

    [UnitySetUp]
    protected override IEnumerator SetUp()
    {
        chatController = new ChatController_Mock();

        GameObject go = Object.Instantiate((GameObject)Resources.Load(UNREAD_NOTIFICATION_BADGE_RESOURCE_NAME));
        unreadNotificationBadge = go.GetComponent<UnreadNotificationBadge>();
        unreadNotificationBadge.Initialize(chatController, TEST_USER_ID);

        Assert.AreEqual(0, unreadNotificationBadge.CurrentUnreadMessages, "There shouldn't be any unread notification after initialization");
        Assert.AreEqual(false, unreadNotificationBadge.notificationContainer.activeSelf, "Notificaton container should be deactivated");

        yield break;
    }

    protected override IEnumerator TearDown()
    {
        Object.Destroy(unreadNotificationBadge.gameObject);
        yield break;
    }

    [Test]
    public void ReceivePrivateMessage()
    {
        chatController.RaiseAddMessage(new ChatMessage
        {
            messageType = ChatMessage.Type.PRIVATE,
            sender = TEST_USER_ID,
            body = "test body",
            recipient = "test recipient",
            timestamp = (ulong) System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        });

        Assert.AreEqual(1, unreadNotificationBadge.CurrentUnreadMessages, "There should be 1 unread notification related to the sent private message");
        Assert.AreEqual(true, unreadNotificationBadge.notificationContainer.activeSelf, "Notificaton container should be activated");
        Assert.AreEqual("1", unreadNotificationBadge.notificationText.text, "Notification text should be 1");
    }

    [Test]
    public void ReceivePublicMessage()
    {
        chatController.RaiseAddMessage(new ChatMessage
        {
            messageType = ChatMessage.Type.PUBLIC,
            sender = TEST_USER_ID,
            body = "test body",
            recipient = "test recipient",
            timestamp = (ulong)System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        });

        Assert.AreEqual(0, unreadNotificationBadge.CurrentUnreadMessages, "There shouldn't be any unread notification related to the sent public message");
        Assert.AreEqual(false, unreadNotificationBadge.notificationContainer.activeSelf, "Notificaton container should be deactivated");
    }
}
