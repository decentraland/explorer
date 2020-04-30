using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

public class FriendRequestEntryShould : TestsBase
{
    static string FRIEND_REQUEST_ENTRY_RESOURCE_NAME = "FriendRequestEntry";

    FriendRequestEntry entry;

    [UnitySetUp]
    protected override IEnumerator SetUp()
    {
        GameObject go = Object.Instantiate((GameObject)Resources.Load(FRIEND_REQUEST_ENTRY_RESOURCE_NAME));
        entry = go.GetComponent<FriendRequestEntry>();
        yield break;
    }

    protected override IEnumerator TearDown()
    {
        Object.Destroy(entry.gameObject);
        yield break;
    }

    [Test]
    public void BePopulatedCorrectly()
    {
        Sprite testSprite1 = Sprite.Create(Texture2D.whiteTexture, Rect.zero, Vector2.zero);
        Sprite testSprite2 = Sprite.Create(Texture2D.blackTexture, Rect.zero, Vector2.zero);
        var model1 = new FriendEntry.Model() { userName = "test1", avatarImage = testSprite1 };
        var model2 = new FriendEntry.Model() { userName = "test2", avatarImage = testSprite2 };

        entry.Populate("userId1", model1, isReceived: true);

        Assert.AreEqual(model1.userName, entry.playerNameText.text);
        Assert.AreEqual(model1.avatarImage, entry.playerImage.sprite);

        Assert.IsFalse(entry.cancelButton.gameObject.activeSelf);
        Assert.IsTrue(entry.acceptButton.gameObject.activeSelf);
        Assert.IsTrue(entry.rejectButton.gameObject.activeSelf);

        entry.Populate("userId2", model2, isReceived: false);

        Assert.AreEqual(model2.userName, entry.playerNameText.text);
        Assert.AreEqual(model2.avatarImage, entry.playerImage.sprite);

        Assert.IsTrue(entry.cancelButton.gameObject.activeSelf);
        Assert.IsFalse(entry.acceptButton.gameObject.activeSelf);
        Assert.IsFalse(entry.rejectButton.gameObject.activeSelf);

        Object.Destroy(testSprite1);
        Object.Destroy(testSprite2);
    }

    [Test]
    public void AcceptRequestCorrectly()
    {
        entry.Populate("userId1", new FriendEntry.Model(), isReceived: true);
        entry.acceptButton.onClick.Invoke();
    }

    [Test]
    public void RejectRequestCorrectly()
    {
        var model1 = new FriendEntry.Model() { userName = "test1", avatarImage = null };
        entry.Populate("userId1", model1, isReceived: true);
        entry.rejectButton.onClick.Invoke();
    }

    [Test]
    public void CancelRequestCorrectly()
    {
        var model2 = new FriendEntry.Model() { userName = "test1", avatarImage = null };
        entry.Populate("userId1", model2, isReceived: false);
        entry.cancelButton.onClick.Invoke();
    }
}
