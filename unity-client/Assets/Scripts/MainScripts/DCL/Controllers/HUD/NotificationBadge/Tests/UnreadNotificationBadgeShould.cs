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

        Assert.AreEqual(0, unreadNotificationBadge.CurrentUnreadMessages);

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

        Assert.AreEqual(1, unreadNotificationBadge.CurrentUnreadMessages);
    }
}
