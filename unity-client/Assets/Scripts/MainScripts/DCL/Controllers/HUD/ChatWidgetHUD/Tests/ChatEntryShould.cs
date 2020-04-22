using NUnit.Framework;
using System.Collections;
using UnityEngine;
public class ChatEntryShould : TestsBase
{
    ChatEntry entry;
    Canvas canvas;
    protected override IEnumerator SetUp()
    {
        var canvasgo = new GameObject("canvas");
        canvas = canvasgo.AddComponent<Canvas>();
        (canvas.transform as RectTransform).sizeDelta = new Vector2(500, 500);
        var go = Object.Instantiate(Resources.Load("Chat Entry"), canvas.transform, false) as GameObject;
        entry = go.GetComponent<ChatEntry>();
        yield break;
    }


    protected override IEnumerator TearDown()
    {
        Object.Destroy(entry.gameObject);
        Object.Destroy(canvas.gameObject);
        yield break;
    }

    [Test]
    public void BePopulatedCorrectly()
    {
        var message = new ChatController.ChatMessage()
        {
            messageType = ChatController.ChatMessageType.PUBLIC,
            sender = "user-test",
            recipient = "",
            timestamp = 0,
            body = "test message",
        };

        entry.Populate(message);

        Assert.AreEqual(entry.worldMessageColor, entry.body.color);
        Assert.AreEqual("<b>user-test:</b>", entry.username.text);
        Assert.AreEqual("<b>user-test:</b> test message", entry.body.text);

        message.messageType = ChatController.ChatMessageType.PRIVATE;
        entry.Populate(message);
        Assert.AreEqual(entry.privateMessageColor, entry.body.color);

        message.messageType = ChatController.ChatMessageType.SYSTEM;
        entry.Populate(message);
        Assert.AreEqual(entry.systemColor, entry.body.color);

        entry.Populate(null);
    }

    [Test]
    public void HaveCorrectSizeForMessageBody()
    {
        var message = new ChatController.ChatMessage()
        {
            messageType = ChatController.ChatMessageType.NONE,
            sender = "user test",
            recipient = "",
            timestamp = 0,
            body = "\t\t\tLorem ipsum dolor sit amet, consectetur adipiscing elit. Cras convallis rutrum est id efficitur. Donec ornare, neque vitae dignissim mattis, sem diam varius augue, sollicitudin tempor dui sapien quis felis. Donec aliquet bibendum ligula, fringilla vestibulum sapien vehicula eu. Nulla auctor lectus a dui scelerisque tincidunt. Ut eget gravida ligula. Vestibulum pellentesque ac mi id aliquam. In hendrerit dignissim commodo. Nulla in fringilla dolor, at efficitur arcu. Nullam malesuada et est et mattis. Vestibulum quis massa sodales, consequat diam nec, condimentum diam. Aliquam eu sollicitudin dui, ac aliquet purus. Praesent id neque in erat tincidunt eleifend. Phasellus sit amet nisi luctus nibh imperdiet sollicitudin ut sit amet felis. Pellentesque dictum tempor lectus accumsan mollis. Vivamus in aliquam enim, vel scelerisque arcu.",
        };

        entry.Populate(message);
        UnityEngine.Assertions.Assert.AreApproximatelyEqual(143.82f, (entry.body.transform as RectTransform).sizeDelta.y, 0.5f);

        message.body = "Tiny message";
        entry.Populate(message);

        UnityEngine.Assertions.Assert.AreApproximatelyEqual(33.41f, (entry.body.transform as RectTransform).sizeDelta.y, 0.5f);
    }
}
