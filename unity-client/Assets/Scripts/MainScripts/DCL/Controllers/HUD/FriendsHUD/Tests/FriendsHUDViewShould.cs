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
        string id1 = "userId-1";
        string id2 = "userId-2";

        var entry1 = CreateFriendEntry(id1, "Pravus", FriendsController.PresenceStatus.ONLINE);
        var entry2 = CreateFriendEntry(id2, "Brian", FriendsController.PresenceStatus.OFFLINE);

        Assert.IsNotNull(entry1);
        Assert.AreEqual(entry1.model.userName, entry1.playerNameText.text);
        Assert.AreEqual(controller.view.friendsList.onlineFriendsContainer, entry1.transform.parent);

        Assert.IsNotNull(entry2);
        Assert.AreEqual(entry2.model.userName, entry2.playerNameText.text);
        Assert.AreEqual(controller.view.friendsList.offlineFriendsContainer, entry2.transform.parent);

        var model2 = entry2.model;
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
        string id1 = "userId-1";
        string id2 = "userId-2";

        var entry1 = CreateFriendRequestEntry(id1, "Pravus", true);
        var entry2 = CreateFriendRequestEntry(id2, "Brian", false);

        Assert.IsNotNull(entry1);
        Assert.AreEqual("Pravus", entry1.playerNameText.text);
        Assert.AreEqual(controller.view.friendRequestsList.receivedRequestsContainer, entry1.transform.parent);

        Assert.IsNotNull(entry2);
        Assert.AreEqual("Brian", entry2.playerNameText.text);
        Assert.AreEqual(controller.view.friendRequestsList.sentRequestsContainer, entry2.transform.parent);

        controller.view.friendRequestsList.UpdateEntry(id2, entry2.model, true);
        Assert.AreEqual(controller.view.friendRequestsList.receivedRequestsContainer, entry2.transform.parent);
    }

    [Test]
    public void CountProperlyStatus()
    {
        CreateFriendEntry("user1", "Armando Barreda", FriendsController.PresenceStatus.ONLINE);
        CreateFriendEntry("user2", "Neo", FriendsController.PresenceStatus.ONLINE);

        CreateFriendEntry("user3", "Wanda Nara", FriendsController.PresenceStatus.OFFLINE);
        CreateFriendEntry("user4", "Mirtha Legrand", FriendsController.PresenceStatus.OFFLINE);

        Assert.AreEqual(2, view.friendsList.onlineFriends);
        Assert.AreEqual(2, view.friendsList.offlineFriends);

        view.friendsList.RemoveEntry("user1");
        view.friendsList.RemoveEntry("user3");

        Assert.AreEqual(1, view.friendsList.onlineFriends);
        Assert.AreEqual(1, view.friendsList.offlineFriends);
    }


    [Test]
    public void OpenContextMenuProperly()
    {
        var entry = CreateFriendEntry("userId-1", "Pravus");
        entry.menuButton.onClick.Invoke();
        Assert.IsTrue(view.friendsList.friendMenuPanel.activeSelf);
        Assert.AreEqual(entry, view.friendsList.selectedFriendEntry);
    }

    [Test]
    public void DeleteFriendProperly()
    {
        string id1 = "userId-1";
        var entry = CreateFriendEntry(id1, "Ted Bundy");

        entry.menuButton.onClick.Invoke();
        Assert.IsTrue(view.friendsList.deleteFriendButton.gameObject.activeSelf);
        Assert.IsTrue(view.friendsList.deleteFriendButton.isActiveAndEnabled);

        view.friendsList.deleteFriendButton.onClick.Invoke();
        view.friendsList.deleteFriendDialogConfirmButton.onClick.Invoke();

        Assert.IsNull(view.friendsList.GetEntry(id1));
    }

    [Test]
    public void RejectIncomingFriendRequestsProperly()
    {
        //NOTE(Brian): Confirm cancellation
        var entry = CreateFriendRequestEntry("id1", "Padre Grassi", isReceived: true);

        entry.rejectButton.onClick.Invoke();

        Assert.IsTrue(view.friendRequestsList.rejectRequestDialog.activeSelf);

        view.friendRequestsList.rejectRequestDialogConfirmButton.onClick.Invoke();

        Assert.IsFalse(view.friendRequestsList.rejectRequestDialog.activeSelf);
        Assert.IsNull(view.friendRequestsList.GetEntry(entry.userId));

        //NOTE(Brian): Deny cancellation
        var entry2 = CreateFriendRequestEntry("id1", "Warren Buffet", isReceived: true);

        entry2.rejectButton.onClick.Invoke();

        Assert.IsTrue(view.friendRequestsList.rejectRequestDialog.activeSelf);

        view.friendRequestsList.rejectRequestDialogCancelButton.onClick.Invoke();

        Assert.IsFalse(view.friendRequestsList.rejectRequestDialog.activeSelf);
        Assert.IsNotNull(view.friendRequestsList.GetEntry(entry2.userId));
    }

    [Test]
    public void SendAndCancelFriendRequestsProperly()
    {
        //NOTE(Brian): Confirm cancellation
        var entry = CreateFriendRequestEntry("id1", "Padre Grassi", isReceived: false);

        entry.cancelButton.onClick.Invoke();

        Assert.IsTrue(view.friendRequestsList.cancelRequestDialog.activeSelf);

        view.friendRequestsList.cancelRequestDialogConfirmButton.onClick.Invoke();

        Assert.IsFalse(view.friendRequestsList.cancelRequestDialog.activeSelf);
        Assert.IsNull(view.friendRequestsList.GetEntry(entry.userId));

        //NOTE(Brian): Deny cancellation
        var entry2 = CreateFriendRequestEntry("id1", "Warren Buffet", isReceived: false);

        entry2.cancelButton.onClick.Invoke();

        Assert.IsTrue(view.friendRequestsList.cancelRequestDialog.activeSelf);

        view.friendRequestsList.cancelRequestDialogCancelButton.onClick.Invoke();

        Assert.IsFalse(view.friendRequestsList.cancelRequestDialog.activeSelf);
        Assert.IsNotNull(view.friendRequestsList.GetEntry(entry2.userId));
    }

    FriendEntry CreateFriendEntry(string id, string name, FriendsController.PresenceStatus status = FriendsController.PresenceStatus.ONLINE)
    {
        var model1 = new FriendEntry.Model()
        {
            status = status,
            userName = name,
        };

        controller.view.friendsList.CreateOrUpdateEntry(id, model1);

        return controller.view.friendsList.GetEntry(id);
    }

    FriendRequestEntry CreateFriendRequestEntry(string id, string name, bool isReceived)
    {
        var model1 = new FriendEntry.Model()
        {
            userName = name,
        };

        controller.view.friendRequestsList.CreateOrUpdateEntry(id, model1, isReceived);

        return controller.view.friendRequestsList.GetEntry(id);
    }

}
