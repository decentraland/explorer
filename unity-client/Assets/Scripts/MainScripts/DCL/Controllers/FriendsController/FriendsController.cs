using System.Collections.Generic;
using UnityEngine;

public interface IFriendsController
{
    void InitializeFriends(string json);
    void UpdateFriendshipStatus(string json);
}
public class FriendsController : MonoBehaviour, IFriendsController
{
    public static FriendsController i { get; private set; }

    void Awake()
    {
        i = this;
    }

    public Dictionary<string, FriendshipStatus> friends = new Dictionary<string, FriendshipStatus>();

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
        CANCELED,
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

    public FriendshipStatus GetFriendStatus(string userId)
    {
        if (!friends.ContainsKey(userId))
            return FriendshipStatus.NONE;

        return friends[userId];
    }

    public event System.Action<string, FriendshipAction> OnUpdateFriendship;

    public Dictionary<string, FriendshipStatus> GetFriends()
    {
        return new Dictionary<string, FriendshipStatus>(friends);
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

    public void UpdateFriendshipStatus(FriendshipUpdateStatusMessage msg)
    {
        switch (msg.action)
        {
            case FriendshipAction.NONE:
                break;
            case FriendshipAction.APPROVED:
                friends[msg.userId] = FriendshipStatus.FRIEND;
                break;
            case FriendshipAction.REJECTED:
                friends[msg.userId] = FriendshipStatus.NONE;
                break;
            case FriendshipAction.CANCELED:
                friends[msg.userId] = FriendshipStatus.NONE;
                break;
            case FriendshipAction.REQUESTED_FROM:
                friends[msg.userId] = FriendshipStatus.REQUESTED_FROM;
                break;
            case FriendshipAction.REQUESTED_TO:
                friends[msg.userId] = FriendshipStatus.REQUESTED_TO;
                break;
            case FriendshipAction.DELETED:
                friends[msg.userId] = FriendshipStatus.NONE;
                break;
        }

        if (friends[msg.userId] == FriendshipStatus.NONE)
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
