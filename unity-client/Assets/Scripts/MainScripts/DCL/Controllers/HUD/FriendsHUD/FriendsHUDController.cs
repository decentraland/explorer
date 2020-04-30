using DCL.Interface;
using UnityEngine;

public class FriendsHUDController : IHUD
{
    internal const string CURRENT_PLAYER_ID = "CurrentPlayerInfoCardId";
    public FriendsHUDView view
    {
        get;
        private set;
    }

    IFriendsController friendsController;
    public event System.Action<string> OnPressWhisper;
    public void Initialize(IFriendsController friendsController)
    {
        view = FriendsHUDView.Create();
        this.friendsController = friendsController;

        if (this.friendsController != null)
        {
            this.friendsController.OnUpdateFriendship += OnUpdateFriendship;
            this.friendsController.OnUpdateUserStatus += OnUpdateUserStatus;
        }

        view.friendRequestsList.OnFriendRequestApproved += Entry_OnRequestAccepted;
        view.friendRequestsList.OnFriendRequestCancelled += Entry_OnRequestCancelled;
        view.friendRequestsList.OnFriendRequestRejected += Entry_OnRequestRejected;
        view.friendRequestsList.OnFriendRequestSent += Entry_OnRequestSent;
        view.friendRequestsList.OnBlock += Entry_OnBlock;
        view.friendRequestsList.OnPassport += Entry_OnPassport;

        view.friendsList.OnJumpIn += Entry_OnJumpIn;
        view.friendsList.OnWhisper += Entry_OnWhisper;
        view.friendsList.OnBlock += Entry_OnBlock;
        view.friendsList.OnDelete += Entry_OnDelete;
        view.friendsList.OnPassport += Entry_OnPassport;
        view.friendsList.OnReport += Entry_OnReport;
    }

    private void Entry_OnRequestSent(string userId)
    {
        WebInterface.UpdateFriendshipStatus(new FriendsController.FriendshipUpdateStatusMessage() { userId = userId, action = FriendsController.FriendshipAction.REQUESTED_TO });
    }

    private void OnUpdateUserStatus(string userId, FriendsController.UserStatus newStatus)
    {
        var model = new FriendEntry.Model();

        IFriendEntry entry = view.friendsList.GetEntry(userId) as IFriendEntry ?? view.friendRequestsList.GetEntry(userId) as IFriendEntry;

        if (entry != null)
            model = entry.model;

        model.status = newStatus.presenceStatus;

        view.friendsList.UpdateEntry(userId, model);
        view.friendRequestsList.UpdateEntry(userId, model);
    }

    private void OnUpdateFriendship(string userId, FriendsController.FriendshipAction friendshipAction)
    {
        var userProfile = UserProfileController.userProfilesCatalog.Get(userId);

        if (userProfile == null)
        {
            Debug.LogError($"UserProfile is null for {userId}! ... friendshipAction {friendshipAction}");
            return;
        }

        var friendEntryModel = new FriendEntry.Model();

        IFriendEntry entry = view.friendsList.GetEntry(userId) as IFriendEntry ?? view.friendRequestsList.GetEntry(userId) as IFriendEntry;

        if (entry != null)
            friendEntryModel = entry.model;

        friendEntryModel.userName = userProfile.userName;
        friendEntryModel.avatarImage = userProfile.faceSnapshot;

        switch (friendshipAction)
        {
            case FriendsController.FriendshipAction.NONE:
                break;
            case FriendsController.FriendshipAction.APPROVED:
                view.friendRequestsList.RemoveEntry(userId);
                view.friendsList.CreateOrUpdateEntry(userId, friendEntryModel);
                break;
            case FriendsController.FriendshipAction.REJECTED:
                view.friendRequestsList.RemoveEntry(userId);
                break;
            case FriendsController.FriendshipAction.CANCELLED:
                view.friendRequestsList.RemoveEntry(userId);
                break;
            case FriendsController.FriendshipAction.REQUESTED_FROM:
                view.friendRequestsList.CreateOrUpdateEntry(userId, friendEntryModel, true);
                break;
            case FriendsController.FriendshipAction.REQUESTED_TO:
                view.friendRequestsList.CreateOrUpdateEntry(userId, friendEntryModel, false);
                break;
            case FriendsController.FriendshipAction.DELETED:
                view.friendRequestsList.RemoveEntry(userId);
                view.friendsList.RemoveEntry(userId);
                break;
        }

        var pendingFriendRequestsSO = Resources.Load<FloatVariable>("ScriptableObjects/PendingFriendRequests");

        if (pendingFriendRequestsSO != null)
            pendingFriendRequestsSO.Set(view.friendRequestsList.entriesCount);
    }

    private void Entry_OnWhisper(FriendEntry entry)
    {
        OnPressWhisper?.Invoke(entry.model.userName);
    }

    private void Entry_OnReport(string userId)
    {
        WebInterface.SendReportPlayer(userId);
    }

    private void Entry_OnPassport(string userId)
    {
        var currentPlayerId = Resources.Load<StringVariable>(CURRENT_PLAYER_ID);
        currentPlayerId.Set(userId);
    }

    private void Entry_OnBlock(string userId)
    {
        WebInterface.SendBlockPlayer(userId);
    }

    private void Entry_OnJumpIn(FriendEntry entry)
    {
        WebInterface.GoTo((int)entry.model.coords.x, (int)entry.model.coords.y);
    }

    private void Entry_OnDelete(FriendEntry entry)
    {
        WebInterface.UpdateFriendshipStatus(
            new FriendsController.FriendshipUpdateStatusMessage()
            {
                action = FriendsController.FriendshipAction.DELETED,
                userId = entry.userId
            });
    }

    private void Entry_OnRequestRejected(FriendRequestEntry entry)
    {
        WebInterface.UpdateFriendshipStatus(
            new FriendsController.FriendshipUpdateStatusMessage()
            {
                action = FriendsController.FriendshipAction.REJECTED,
                userId = entry.userId
            });
    }

    private void Entry_OnRequestCancelled(FriendRequestEntry entry)
    {
        WebInterface.UpdateFriendshipStatus(
            new FriendsController.FriendshipUpdateStatusMessage()
            {
                action = FriendsController.FriendshipAction.CANCELLED,
                userId = entry.userId
            });
    }

    private void Entry_OnRequestAccepted(FriendRequestEntry entry)
    {
        WebInterface.UpdateFriendshipStatus(
            new FriendsController.FriendshipUpdateStatusMessage()
            {
                action = FriendsController.FriendshipAction.APPROVED,
                userId = entry.userId
            });
    }

    public void Dispose()
    {
        if (this.friendsController != null)
        {
            this.friendsController.OnUpdateFriendship -= OnUpdateFriendship;
            this.friendsController.OnUpdateUserStatus -= OnUpdateUserStatus;
        }

        if (view != null)
        {
            UnityEngine.Object.Destroy(view.gameObject);
        }
    }

    public void SetVisibility(bool visible)
    {
        view.gameObject.SetActive(visible);
    }

}
