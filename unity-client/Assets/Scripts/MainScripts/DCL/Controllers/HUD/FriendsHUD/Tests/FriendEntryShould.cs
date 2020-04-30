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
    [Explicit("Disabling until feature is complete")]
    public void BePopulatedCorrectly()
    {
        Sprite testSprite = Sprite.Create(Texture2D.whiteTexture, Rect.zero, Vector2.zero);

        var model = new FriendEntry.Model() { };
        entry.Populate("userId-1", model);
        Object.Destroy(testSprite);
    }

    [Test]
    [Explicit("Disabling until feature is complete")]
    public void SendProperMessageWhenGoToButtonIsPressed()
    {
        var model = new FriendEntry.Model() { };
        entry.Populate("userId-1", model);
        entry.jumpInButton.onClick.Invoke();

    }

    [Test]
    [Explicit("Disabling until feature is complete")]
    public void WhisperPlayerCorrectlyWhenWhisperButtonIsPressed()
    {
        var model = new FriendEntry.Model() { };
        entry.Populate("userId-1", model);
        entry.whisperButton.onClick.Invoke();
    }
}
