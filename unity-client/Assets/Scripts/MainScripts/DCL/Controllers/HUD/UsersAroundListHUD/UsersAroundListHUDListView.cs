﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

internal class UsersAroundListHUDListView : MonoBehaviour, IUsersAroundListHUDListView
{
    public event Action<string, bool> OnRequestMuteUser;
    public event Action<bool> OnRequestMuteGlobal;

    [SerializeField] private UsersAroundListHUDListElementView listElementView;
    [SerializeField] private ShowHideAnimator showHideAnimator;
    [SerializeField] internal TMPro.TextMeshProUGUI textFriendsTitle;
    [SerializeField] internal TMPro.TextMeshProUGUI textPlayersTitle;
    [SerializeField] internal Transform contentFriends;
    [SerializeField] internal Transform contentPlayers;
    [SerializeField] internal Toggle muteAllToggle;

    internal Queue<UsersAroundListHUDListElementView> availableElements;
    internal Dictionary<string, UsersAroundListHUDListElementView> userElementDictionary;

    private string friendsTextPattern;
    private string playersTextPattern;
    private int friendsCount = 0;
    private int playersCount = 0;

    private void Awake()
    {
        availableElements = new Queue<UsersAroundListHUDListElementView>();
        userElementDictionary = new Dictionary<string, UsersAroundListHUDListElementView>();

        friendsTextPattern = textFriendsTitle.text;
        playersTextPattern = textPlayersTitle.text;
        textFriendsTitle.text = string.Format(friendsTextPattern, friendsCount);
        textPlayersTitle.text = string.Format(playersTextPattern, playersCount);

        muteAllToggle.onValueChanged.AddListener(OnMuteGlobal);

        listElementView.OnMuteUser += OnMuteUser;
        listElementView.OnPoolRelease();
        availableElements.Enqueue(listElementView);
    }

    void IUsersAroundListHUDListView.AddOrUpdateUser(MinimapMetadata.MinimapUserInfo userInfo)
    {
        if (userElementDictionary.ContainsKey(userInfo.userId))
        {
            return;
        }

        var profile = UserProfileController.userProfilesCatalog.Get(userInfo.userId);

        if (profile == null)
            return;

        bool isFriend = FriendsController.i?.friends.ContainsKey(userInfo.userId) ?? false;

        UsersAroundListHUDListElementView view = null;
        if (availableElements.Count > 0)
        {
            view = availableElements.Dequeue();
            view.transform.SetParent(isFriend ? contentFriends : contentPlayers);
        }
        else
        {
            view = Instantiate(listElementView, isFriend ? contentFriends : contentPlayers);
            view.OnMuteUser += OnMuteUser;
        }

        view.OnPoolGet();
        view.SetUserProfile(profile);
        userElementDictionary.Add(userInfo.userId, view);
        ModifyListCount(isFriend, 1);
    }

    void IUsersAroundListHUDListView.RemoveUser(string userId)
    {
        if (!userElementDictionary.TryGetValue(userId, out UsersAroundListHUDListElementView elementView))
        {
            return;
        }
        if (!elementView)
        {
            return;
        }

        ModifyListCount(elementView.transform.parent == contentFriends, -1);
        PoolElementView(elementView);
        userElementDictionary.Remove(userId);
    }

    void IUsersAroundListHUDListView.SetUserRecording(string userId, bool isRecording)
    {
        if (!userElementDictionary.TryGetValue(userId, out UsersAroundListHUDListElementView elementView))
        {
            return;
        }
        elementView.SetRecording(isRecording);
    }

    void IUsersAroundListHUDListView.SetUserMuted(string userId, bool isMuted)
    {
        if (!userElementDictionary.TryGetValue(userId, out UsersAroundListHUDListElementView elementView))
        {
            return;
        }
        elementView.SetMuted(isMuted);
    }

    void IUsersAroundListHUDListView.SetVisibility(bool visible)
    {
        if (visible)
        {
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }

            showHideAnimator.Show();
        }
        else
        {
            showHideAnimator.Hide();
        }
    }

    void IUsersAroundListHUDListView.Dispose()
    {
        userElementDictionary.Clear();
        availableElements.Clear();
        Destroy(gameObject);
    }

    void OnMuteUser(string userId, bool mute)
    {
        OnRequestMuteUser?.Invoke(userId, mute);
    }

    void OnMuteGlobal(bool mute)
    {
        OnRequestMuteGlobal?.Invoke(mute);
    }

    void PoolElementView(UsersAroundListHUDListElementView element)
    {
        element.OnPoolRelease();
        availableElements.Enqueue(element);
    }

    void ModifyListCount(bool isFriend, int delta)
    {
        if (isFriend)
        {
            friendsCount += delta;
            textFriendsTitle.text = string.Format(friendsTextPattern, friendsCount);
        }
        else
        {
            playersCount += delta;
            textPlayersTitle.text = string.Format(playersTextPattern, playersCount);
        }
    }
}
