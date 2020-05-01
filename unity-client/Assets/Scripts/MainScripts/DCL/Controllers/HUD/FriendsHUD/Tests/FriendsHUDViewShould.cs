using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;

public class FriendsHUDViewShould : TestsBase
{
    FriendsHUDController controller;
    FriendsHUDView view;

    protected override bool justSceneSetUp => true;
    [UnitySetUp]
    protected override IEnumerator SetUp()
    {
        base.SetUp();

        controller = new FriendsHUDController();
        controller.Initialize(null);
        this.view = controller.view;

        Assert.IsTrue(view != null, "Friends hud view is null?");
        Assert.IsTrue(controller != null, "Friends hud controller is null?");
        yield break;
    }

    protected override IEnumerator TearDown()
    {
        yield return base.TearDown();
        UnityEngine.Object.Destroy(view);
    }

    [Test]
    public void ChangeContentWhenClickingTabs()
    {
        controller.view.friendsButton.onClick.Invoke();

        Assert.IsTrue(controller.view.friendsList.gameObject.activeSelf);
        Assert.IsFalse(controller.view.friendRequestsList.gameObject.activeSelf);

        controller.view.friendRequestsButton.onClick.Invoke();

        Assert.IsFalse(controller.view.friendsList.gameObject.activeSelf);
        Assert.IsTrue(controller.view.friendRequestsList.gameObject.activeSelf);
    }

    [Test]
    public void PopulateFriendListCorrectly()
    {
        var model1 = new FriendEntry.Model()
        {
            status = FriendsController.PresenceStatus.ONLINE,
            userName = "Pravus",
        };

        var model2 = new FriendEntry.Model()
        {
            status = FriendsController.PresenceStatus.OFFLINE,
            userName = "Brian",
        };

        string id1 = "userId-1";
        string id2 = "userId-2";

        controller.view.friendsList.CreateOrUpdateEntry(id1, model1);
        controller.view.friendsList.CreateOrUpdateEntry(id2, model2);

        var entry1 = controller.view.friendsList.GetEntry(id1);
        Assert.IsNotNull(entry1);
        Assert.AreEqual(model1.userName, entry1.playerNameText.text);
        Assert.AreEqual(controller.view.friendsList.onlineFriendsContainer, entry1.transform.parent);

        var entry2 = controller.view.friendsList.GetEntry(id2);
        Assert.IsNotNull(entry2);
        Assert.AreEqual(model2.userName, entry2.playerNameText.text);
        Assert.AreEqual(controller.view.friendsList.offlineFriendsContainer, entry2.transform.parent);

        model2.status = FriendsController.PresenceStatus.ONLINE;
        controller.view.friendsList.CreateOrUpdateEntry(id2, model2);

        Assert.AreEqual(controller.view.friendsList.onlineFriendsContainer, entry2.transform.parent);
    }


    [Test]
    public void RemoveFriendCorrectly()
    {
        string id1 = "userId-1";

        controller.view.friendRequestsList.CreateOrUpdateEntry(id1, new FriendEntry.Model(), isReceived: true);

        Assert.IsNotNull(controller.view.friendRequestsList.GetEntry(id1));

        controller.view.friendRequestsList.RemoveEntry(id1);

        Assert.IsNull(controller.view.friendRequestsList.GetEntry(id1));
    }

    [Test]
    public void PopulateFriendRequestCorrectly()
    {
        var model1 = new FriendEntry.Model()
        {
            status = FriendsController.PresenceStatus.ONLINE,
            userName = "Pravus",
        };

        var model2 = new FriendEntry.Model()
        {
            status = FriendsController.PresenceStatus.OFFLINE,
            userName = "Brian",
        };

        string id1 = "userId-1";
        string id2 = "userId-2";

        controller.view.friendRequestsList.CreateOrUpdateEntry(id1, model1, true);
        controller.view.friendRequestsList.CreateOrUpdateEntry(id2, model2, false);

        var entry1 = controller.view.friendRequestsList.GetEntry(id1);

        Assert.IsNotNull(entry1);
        Assert.AreEqual(model1.userName, entry1.playerNameText.text);
        Assert.AreEqual(controller.view.friendRequestsList.receivedRequestsContainer, entry1.transform.parent);

        var entry2 = controller.view.friendRequestsList.GetEntry(id2);
        Assert.IsNotNull(entry2);
        Assert.AreEqual(model2.userName, entry2.playerNameText.text);
        Assert.AreEqual(controller.view.friendRequestsList.sentRequestsContainer, entry2.transform.parent);

        controller.view.friendRequestsList.UpdateEntry(id2, model2, true);
        Assert.AreEqual(controller.view.friendRequestsList.receivedRequestsContainer, entry2.transform.parent);
    }

    [Test]
    void CountProperlyStatus()
    {

    }

    [Test]
    void DeleteFriendProperly()
    {
        var model1 = new FriendEntry.Model()
        {
            status = FriendsController.PresenceStatus.ONLINE,
            userName = "Pravus",
        };

        string id1 = "userId-1";

        controller.view.friendsList.CreateOrUpdateEntry(id1, model1);
    }

    [Test]
    void RejectFriendRequestsProperly()
    {
    }

    [Test]
    void CancelFriendRequestsProperly()
    {
    }

    [Test]
    void SendFriendRequestsProperly()
    {
    }
}
