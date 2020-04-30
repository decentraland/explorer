using System.Collections.Generic;
using UnityEngine;

public interface IFriendsController
{
    Dictionary<string, FriendsController.UserStatus> GetFriends();

    event System.Action<string, FriendsController.FriendshipAction> OnUpdateFriendship;
    event System.Action<string, FriendsController.UserStatus> OnUpdateUserStatus;
}
public class FriendsController : MonoBehaviour, IFriendsController
{
    public static FriendsController i { get; private set; }

    void Awake()
    {
        i = this;
    }

    public Dictionary<string, UserStatus> friends = new Dictionary<string, UserStatus>();

    [System.Serializable]
    public class UserStatus
    {
        [System.Serializable]
        public class Realm
        {
            public string serverName;
            public string layer;
        }

        public Realm realm;
        public string userId;
        public FriendshipStatus friendshipStatus;
        public PresenceStatus presenceStatus;
    }

    public enum PresenceStatus
    {
        NONE,
        OFFLINE,
        ONLINE,
        UNAVAILABLE,
    }

    public enum FriendshipStatus
    {
        NONE,
        FRIEND,
        REQUESTED_FROM,
        REQUESTED_TO
    }
    public enum FriendshipAction
    {
        NONE,
        APPROVED,
        REJECTED,
        CANCELLED,
        REQUESTED_FROM,
        REQUESTED_TO,
        DELETED
    }


    [System.Serializable]
    public class FriendshipInitializationMessage
    {
        public string[] currentFriends;
        public string[] requestedTo;
        public string[] requestedFrom;
    }

    [System.Serializable]
    public class FriendshipUpdateStatusMessage
    {
        public string userId;
        public FriendshipAction action;
    }

    public UserStatus GetUserStatus(string userId)
    {
        if (!friends.ContainsKey(userId))
            return new UserStatus() { userId = userId, friendshipStatus = FriendshipStatus.NONE };

        return friends[userId];
    }

    public event System.Action<string, UserStatus> OnUpdateUserStatus;
    public event System.Action<string, FriendshipAction> OnUpdateFriendship;

    public Dictionary<string, UserStatus> GetFriends()
    {
        return new Dictionary<string, UserStatus>(friends);
    }

    public void InitializeFriends(string json)
    {
        FriendshipInitializationMessage msg = JsonUtility.FromJson<FriendshipInitializationMessage>(json);

        foreach (var userId in msg.currentFriends)
        {
            UpdateFriendshipStatus(new FriendshipUpdateStatusMessage() { action = FriendshipAction.APPROVED, userId = userId });
        }

        foreach (var userId in msg.requestedFrom)
        {
            UpdateFriendshipStatus(new FriendshipUpdateStatusMessage() { action = FriendshipAction.REQUESTED_FROM, userId = userId });
        }

        foreach (var userId in msg.requestedTo)
        {
            UpdateFriendshipStatus(new FriendshipUpdateStatusMessage() { action = FriendshipAction.REQUESTED_TO, userId = userId });
        }
    }

    public void UpdateUserStatus(string json)
    {
        UserStatus newUserStatus = JsonUtility.FromJson<UserStatus>(json);

        if (!friends.ContainsKey(newUserStatus.userId))
        {
            friends.Add(newUserStatus.userId, newUserStatus);
        }
        else
        {
            friends[newUserStatus.userId] = newUserStatus;
        }

        OnUpdateUserStatus?.Invoke(newUserStatus.userId, newUserStatus);
    }

    public void UpdateFriendshipStatus(FriendshipUpdateStatusMessage msg)
    {
        if (!friends.ContainsKey(msg.userId))
        {
            friends.Add(msg.userId, new UserStatus() { });
        }

        switch (msg.action)
        {
            case FriendshipAction.NONE:
                break;
            case FriendshipAction.APPROVED:
                friends[msg.userId].friendshipStatus = FriendshipStatus.FRIEND;
                break;
            case FriendshipAction.REJECTED:
                friends[msg.userId].friendshipStatus = FriendshipStatus.NONE;
                break;
            case FriendshipAction.CANCELLED:
                friends[msg.userId].friendshipStatus = FriendshipStatus.NONE;
                break;
            case FriendshipAction.REQUESTED_FROM:
                friends[msg.userId].friendshipStatus = FriendshipStatus.REQUESTED_FROM;
                break;
            case FriendshipAction.REQUESTED_TO:
                friends[msg.userId].friendshipStatus = FriendshipStatus.REQUESTED_TO;
                break;
            case FriendshipAction.DELETED:
                friends[msg.userId].friendshipStatus = FriendshipStatus.NONE;
                break;
        }

        if (friends[msg.userId].friendshipStatus == FriendshipStatus.NONE)
        {
            friends.Remove(msg.userId);
        }

        OnUpdateFriendship?.Invoke(msg.userId, msg.action);
    }

    public void UpdateFriendshipStatus(string json)
    {
        FriendshipUpdateStatusMessage msg = JsonUtility.FromJson<FriendshipUpdateStatusMessage>(json);
        UpdateFriendshipStatus(msg);
    }

}
