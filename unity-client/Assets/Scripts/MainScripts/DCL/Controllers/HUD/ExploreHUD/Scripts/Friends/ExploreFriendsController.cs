using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

internal class ExploreFriendsController : IDisposable
{
    Dictionary<IExploreViewWithFriends, ListenerWrapper> listeners = new Dictionary<IExploreViewWithFriends, ListenerWrapper>();
    Dictionary<string, FriendWrapper> friends = new Dictionary<string, FriendWrapper>();

    IFriendsController friendsController;

    public ExploreFriendsController(IFriendsController friendsController)
    {
        this.friendsController = friendsController;

        if (!friendsController.isInitialized)
        {
            friendsController.OnInitialized += OnFriendsInitialized;
        }

        friendsController.OnUpdateUserStatus += OnUpdateUserStatus;
    }

    public void Dispose()
    {
        friendsController.OnInitialized -= OnFriendsInitialized;
        friendsController.OnUpdateUserStatus -= OnUpdateUserStatus;
        listeners.Clear();
        friends.Clear();
    }

    public void AddListener(IExploreViewWithFriends listener)
    {
        ListenerWrapper wrapper;
        if (listeners.TryGetValue(listener, out wrapper))
        {
            return;
        }

        wrapper = new ListenerWrapper(listener);

        if (friendsController.isInitialized)
        {
            ProcessNewListener(wrapper);
        }
        listeners.Add(listener, wrapper);
    }

    public void RemoveListener(IExploreViewWithFriends listener)
    {
        ListenerWrapper wrapper;
        if (listeners.TryGetValue(listener, out wrapper))
        {
            wrapper.Dispose();
            listeners.Remove(listener);
        }
    }

    void OnUpdateUserStatus(string userId, FriendsController.UserStatus status)
    {
        if (!friendsController.isInitialized)
            return;

        FriendWrapper friend;
        if (!friends.TryGetValue(userId, out friend))
        {
            friend = new FriendWrapper(userId);
            friends.Add(userId, friend);
        }

        friend.SetStatus(status);

        if (!friend.IsOnline())
        {
            friend.RemoveFromAllListeners();
        }
        else
        {
            ProcessFriendLocation(friend, new Vector2Int((int)status.position.x, (int)status.position.y));
        }
    }

    void OnFriendsInitialized()
    {
        friendsController.OnInitialized -= OnFriendsInitialized;

        using (var friendsIterator = friendsController.GetFriends().GetEnumerator())
        {
            while (friendsIterator.MoveNext())
            {
                FriendWrapper friend = new FriendWrapper(friendsIterator.Current.Key);
                friend.SetStatus(friendsIterator.Current.Value);
                friends.Add(friendsIterator.Current.Key, friend);
            }
        }

        using (var listenersIterator = listeners.GetEnumerator())
        {
            while (listenersIterator.MoveNext())
            {
                ProcessNewListener(listenersIterator.Current.Value);
            }
        }
    }

    void ProcessFriendLocation(FriendWrapper friend, Vector2Int coords)
    {
        if (!friend.HasChangeLocation(coords))
            return;

        friend.RemoveFromAllListeners();

        using (var listenersIterator = listeners.GetEnumerator())
        {
            while (listenersIterator.MoveNext())
            {
                if (listenersIterator.Current.Value.ContainCoords(coords))
                {
                    friend.AddListener(listenersIterator.Current.Value);
                }
            }
        }
    }

    void ProcessNewListener(ListenerWrapper listener)
    {
        Vector2Int friendCoords = new Vector2Int();
        using (var friendIterator = friends.GetEnumerator())
        {
            while (friendIterator.MoveNext())
            {
                if (!friendIterator.Current.Value.IsOnline())
                {
                    continue;
                }

                friendCoords.x = (int)friendIterator.Current.Value.status.position.x;
                friendCoords.y = (int)friendIterator.Current.Value.status.position.y;

                if (listener.ContainCoords(friendCoords))
                {
                    friendIterator.Current.Value.AddListener(listener);
                }
            }
        }
    }
}

class FriendWrapper
{
    HashSet<ListenerWrapper> listeners = new HashSet<ListenerWrapper>();

    public UserProfile profile { private set; get; }
    public FriendsController.UserStatus status { private set; get; }

    public FriendWrapper(string userId)
    {
        profile = UserProfileController.userProfilesCatalog.Get(userId);
    }

    public void SetStatus(FriendsController.UserStatus newStatus)
    {
        status = newStatus;
    }

    public void AddListener(ListenerWrapper listener)
    {
        listener.OnListenerDisposed += OnListenerDisposed;
        listeners.Add(listener);
        listener.OnFriendAdded(profile);
    }

    public void RemoveListener(ListenerWrapper listener)
    {
        OnListenerRemoved(listener);
        listeners.Remove(listener);
    }

    public bool HasListeners()
    {
        return listeners.Count > 0;
    }

    public bool HasChangeLocation(Vector2Int coords)
    {
        if (listeners.Count > 0 && listeners.First().ContainCoords(coords))
        {
            return false;
        }
        return true;
    }

    public void RemoveFromAllListeners()
    {
        if (!HasListeners())
        {
            return;
        }

        using (var listenerIterator = listeners.GetEnumerator())
        {
            while (listenerIterator.MoveNext())
            {
                OnListenerRemoved(listenerIterator.Current);
            }
        }
        listeners.Clear();
    }

    public bool IsOnline()
    {
        if (status.presence != PresenceStatus.ONLINE)
            return false;
        if (status.realm == null)
            return false;
        return !string.IsNullOrEmpty(status.realm.serverName) && !string.IsNullOrEmpty(status.realm.layer);
    }

    void OnListenerDisposed(ListenerWrapper listener)
    {
        listener.OnListenerDisposed -= OnListenerDisposed;
        listeners.Remove(listener);
    }

    void OnListenerRemoved(ListenerWrapper listener)
    {
        listener.OnListenerDisposed -= OnListenerDisposed;
        listener.OnFriendRemoved(profile);
    }
}

class ListenerWrapper : IDisposable
{
    IExploreViewWithFriends listener;

    public event Action<ListenerWrapper> OnListenerDisposed;

    public ListenerWrapper(IExploreViewWithFriends listener)
    {
        this.listener = listener;
    }

    public void OnFriendAdded(UserProfile profile)
    {
        listener.OnFriendAdded(profile);
    }

    public void OnFriendRemoved(UserProfile profile)
    {
        listener.OnFriendRemoved(profile);
    }

    public bool ContainCoords(Vector2Int coords)
    {
        return listener.ContainCoords(coords);
    }

    public void Dispose()
    {
        OnListenerDisposed?.Invoke(this);
    }
}