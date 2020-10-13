using System.Collections.Generic;
using DCL.Interface;
using UnityEngine;

public class UsersAroundListHUDController : IHUD
{
    internal IUsersAroundListHUDButtonView usersButtonView;
    internal IUsersAroundListHUDListView usersListView;

    private bool isVisible = false;
    private readonly HashSet<string> trackedUsersHashSet = new HashSet<string>();
    private UserProfile profile => UserProfile.GetOwnUserProfile();

    public UsersAroundListHUDController()
    {
        UsersAroundListHUDListView view = Object.Instantiate(Resources.Load<GameObject>("UsersAroundListHUD")).GetComponent<UsersAroundListHUDListView>();
        view.name = "_UsersAroundListHUD";
        view.gameObject.SetActive(false);
        Initialize(view);
    }

    public UsersAroundListHUDController(IUsersAroundListHUDListView usersListView)
    {
        Initialize(usersListView);
    }

    public void Dispose()
    {
        MinimapMetadata.GetMetadata().OnUserInfoUpdated -= MapRenderer_OnUserInfoUpdated;
        MinimapMetadata.GetMetadata().OnUserInfoRemoved -= MapRenderer_OnUserInfoRemoved;
        usersListView.Dispose();
        if (usersButtonView != null)
        {
            usersButtonView.OnClick -= ToggleVisibility;
        }
    }

    public void SetVisibility(bool visible)
    {
        isVisible = visible;
        usersListView.SetVisibility(visible);
    }

    public void SetButtonView(IUsersAroundListHUDButtonView view)
    {
        usersButtonView = view;
        usersButtonView.OnClick += ToggleVisibility;
    }

    public void SetUserMuted(string userId, bool isMuted)
    {
        usersListView.SetUserMuted(userId, isMuted);
    }

    public void SetUserRecording(string userId, bool isRecording)
    {
        usersListView.SetUserRecording(userId, isRecording);
    }

    void Initialize(IUsersAroundListHUDListView view)
    {
        usersListView = view;
        usersListView.OnRequestMuteUser += ((userId, mute) =>
        {
            WebInterface.SetMuteUser(userId, mute);
        });
        MinimapMetadata.GetMetadata().OnUserInfoUpdated += MapRenderer_OnUserInfoUpdated;
        MinimapMetadata.GetMetadata().OnUserInfoRemoved += MapRenderer_OnUserInfoRemoved;
    }

    void MapRenderer_OnUserInfoUpdated(MinimapMetadata.MinimapUserInfo userInfo)
    {
        usersListView.AddOrUpdateUser(userInfo);

        if (!trackedUsersHashSet.Contains(userInfo.userId))
        {
            trackedUsersHashSet.Add(userInfo.userId);
            bool isMuted = profile.muted.Contains(userInfo.userId);
            usersListView.SetUserMuted(userInfo.userId, isMuted);
        }

        usersButtonView?.SetUsersCount(trackedUsersHashSet.Count);
    }

    void MapRenderer_OnUserInfoRemoved(string userId)
    {
        if (trackedUsersHashSet.Contains(userId))
        {
            trackedUsersHashSet.Remove(userId);
            usersButtonView?.SetUsersCount(trackedUsersHashSet.Count);
        }
        usersListView.RemoveUser(userId);

        if (isVisible && trackedUsersHashSet.Count == 0)
        {
            ToggleVisibility();
        }
    }

    void ToggleVisibility()
    {
        bool setVisible = !isVisible;
        if (trackedUsersHashSet.Count == 0 && setVisible)
        {
            return;
        }

        SetVisibility(setVisible);
    }
}
