using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

public class FriendEntryShould : TestsBase
{
    static string FRIEND_ENTRY_RESOURCE_NAME = "FriendEntry";

    FriendEntry entry;

    [UnitySetUp]
    protected override IEnumerator SetUp()
    {
        GameObject go = Object.Instantiate((GameObject)Resources.Load(FRIEND_ENTRY_RESOURCE_NAME));
        entry = go.GetComponent<FriendEntry>();
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
        Sprite testSprite = Sprite.Create(Texture2D.whiteTexture, Rect.zero, Vector2.zero);

        FriendEntry.Model model = new FriendEntry.Model()
        {
            coords = Vector2.one,
            avatarImage = testSprite,
            realm = "realm-test",
            status = FriendsController.PresenceStatus.ONLINE,
            userName = "test-name"
        };

        entry.Populate("userId-1", model);

        Assert.AreEqual(entry.playerNameText.text, entry.playerNameText.text);
        Assert.AreEqual(entry.playerLocationText.text, $"{model.realm} {model.coords}");
        Assert.AreEqual(entry.playerImage.sprite, testSprite);

        Object.Destroy(testSprite);
    }

    [Test]
    public void SendProperEventWhenJumpInButtonIsPressed()
    {
        var model = new FriendEntry.Model() { };
        entry.Populate("userId-1", model);
        bool buttonPressed = false;
        entry.OnJumpInClick += (x) => { if (x == entry) buttonPressed = true; };
        entry.jumpInButton.onClick.Invoke();
        Assert.IsTrue(buttonPressed);
    }

    [Test]
    public void SendProperEventWhenWhisperButtonIsPressed()
    {
        var model = new FriendEntry.Model() { };
        entry.Populate("userId-1", model);
        bool buttonPressed = false;
        entry.OnWhisperClick += (x) => { if (x == entry) buttonPressed = true; };
        entry.whisperButton.onClick.Invoke();
        Assert.IsTrue(buttonPressed);
    }

    [Test]
    public void SendProperEventWhenOnMenuToggleIsPressed()
    {
        var model = new FriendEntry.Model() { };
        entry.Populate("userId-1", model);
        bool buttonPressed = false;
        entry.OnMenuToggle += (x) => { if (x == entry) buttonPressed = true; };
        entry.menuButton.onClick.Invoke();
        Assert.IsTrue(buttonPressed);
    }
}
