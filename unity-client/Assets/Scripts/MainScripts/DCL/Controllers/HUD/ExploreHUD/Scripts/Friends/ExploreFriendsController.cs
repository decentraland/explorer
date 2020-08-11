using UnityEngine;
using System;
using System.Collections.Generic;

internal class ExploreFriendsController : IDisposable
{
    List<IExploreViewWithFriends> listeners = new List<IExploreViewWithFriends>();
    Dictionary<string, FriendData> friends = new Dictionary<string, FriendData>();

    bool friendsInitialized = false;

    public ExploreFriendsController()
    {
        FriendsController.i.OnInitialized += OnFriendsInitialized;
        FriendsController.i.OnUpdateUserStatus += OnUpdateUserStatus;
    }

    public void Dispose()
    {
        FriendsController.i.OnInitialized -= OnFriendsInitialized;
        FriendsController.i.OnUpdateUserStatus -= OnUpdateUserStatus;
        listeners.Clear();
    }

    public void AddListener(IExploreViewWithFriends listener)
    {
        if (friendsInitialized)
        {
            ProcessNewListener(listener);
        }
        listeners.Add(listener);
    }

    void OnUpdateUserStatus(string userId, FriendsController.UserStatus status)
    {
        if (!friendsInitialized)
            return;

        FriendData friend;
        if (!friends.TryGetValue(userId, out friend))
            return;

        friend.SetStatus(status);

        if (!IsOnline(status))
        {
            ProcessFriendOffline(friend);
        }
        else
        {
            ProcessFriendLocation(friend, new Vector2Int((int)status.position.x, (int)status.position.y));
        }
    }

    void OnFriendsInitialized()
    {
        FriendsController.i.OnInitialized -= OnFriendsInitialized;

        if (friendsInitialized)
        {
            return;
        }

        using (var iterator = FriendsController.i.friends.GetEnumerator())
        {
            while (iterator.MoveNext())
            {
                FriendData friend = new FriendData(iterator.Current.Key);
                friend.SetStatus(iterator.Current.Value);
                friends.Add(iterator.Current.Key, friend);
            }
        }

        for (int i = 0; i < listeners.Count; i++)
        {
            ProcessNewListener(listeners[i]);
        }
        friendsInitialized = true;
    }

    void ProcessFriendOffline(FriendData friend)
    {
        if (friend.listeners.Count == 0)
            return;

        for (int i = 0; i < friend.listeners.Count; i++)
        {
            friend.listeners[i].OnFriendRemoved(friend.profile);
        }
        friend.listeners.Clear();
    }

    void ProcessFriendLocation(FriendData friend, Vector2Int coords)
    {
        if (friend.listeners.Count == 0)
            return;

        if (friend.listeners[0].ContainCoords(coords))
            return;

        for (int i = 0; i < friend.listeners.Count; i++)
        {
            friend.listeners[i].OnFriendRemoved(friend.profile);
        }
        friend.listeners.Clear();

        for (int i = 0; i < listeners.Count; i++)
        {
            if (listeners[i].ContainCoords(coords))
            {
                listeners[i].OnFriendAdded(friend.profile);
                friend.listeners.Add(listeners[i]);
            }
        }
    }

    void ProcessNewListener(IExploreViewWithFriends listener)
    {
        using (var iterator = friends.GetEnumerator())
        {
            while (iterator.MoveNext())
            {
                if (!IsOnline(iterator.Current.Value.status))
                {
                    continue;
                }
                int x = (int)iterator.Current.Value.status.position.x;
                int y = (int)iterator.Current.Value.status.position.y;

                if (listener.ContainCoords(new Vector2Int(x, y)))
                {
                    listener.OnFriendAdded(iterator.Current.Value.profile);
                    iterator.Current.Value.listeners.Add(listener);
                }
            }
        }
    }

    bool IsOnline(FriendsController.UserStatus status)
    {
        if (status.presence != PresenceStatus.ONLINE)
            return false;
        if (status.realm == null)
            return false;
        return !string.IsNullOrEmpty(status.realm.serverName) && !string.IsNullOrEmpty(status.realm.layer);
    }
}

class FriendData
{
    public List<IExploreViewWithFriends> listeners = new List<IExploreViewWithFriends>();

    public UserProfile profile { private set; get; }
    public FriendsController.UserStatus status { private set; get; }

    public FriendData(string userId)
    {
        profile = UserProfileController.userProfilesCatalog.Get(userId);
    }

    public void SetStatus(FriendsController.UserStatus newStatus)
    {
        status = newStatus;
    }
}