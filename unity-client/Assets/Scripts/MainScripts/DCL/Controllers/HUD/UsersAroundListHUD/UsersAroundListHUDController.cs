using System.Collections;
using System.Collections.Generic;
using DCL.Interface;
using UnityEngine;

public class UsersAroundListHUDController : IHUD
{
    const float MUTE_STATUS_UPDATE_INTERVAL = 1;

    internal IUsersAroundListHUDButtonView usersButtonView;
    internal IUsersAroundListHUDListView usersListView;

    private bool isVisible = false;
    private readonly HashSet<string> trackedUsersHashSet = new HashSet<string>();
    private UserProfile profile => UserProfile.GetOwnUserProfile();

    private readonly List<string> usersToMute = new List<string>();
    private readonly List<string> usersToUnmute = new List<string>();
    private bool isMuteAll = false;
    private Coroutine updateMuteStatusRoutine = null;

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
        ReportMuteStatuses();

        if (updateMuteStatusRoutine != null)
        {
            CoroutineStarter.Stop(updateMuteStatusRoutine);
        }

        MinimapMetadata.GetMetadata().OnUserInfoUpdated -= MapRenderer_OnUserInfoUpdated;
        MinimapMetadata.GetMetadata().OnUserInfoRemoved -= MapRenderer_OnUserInfoRemoved;

        usersListView?.Dispose();

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

    public void SetUsersMuted(string[] usersId, bool isMuted)
    {
        for (int i = 0; i < usersId.Length; i++)
        {
            usersListView.SetUserMuted(usersId[i], isMuted);
        }
    }

    public void SetUserRecording(string userId, bool isRecording)
    {
        usersListView.SetUserRecording(userId, isRecording);
    }

    void Initialize(IUsersAroundListHUDListView view)
    {
        usersListView = view;

        usersListView.OnRequestMuteUser += OnMuteUser;
        usersListView.OnRequestMuteGlobal += OnMuteAll;

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

            if (isMuteAll && !isMuted)
            {
                OnMuteUser(userInfo.userId, true);
            }
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

    void OnMuteUser(string userId, bool mute)
    {
        var list = mute ? usersToMute : usersToUnmute;
        list.Add(userId);

        if (updateMuteStatusRoutine == null)
        {
            updateMuteStatusRoutine = CoroutineStarter.Start(MuteStateUpdateRoutine());
        }
    }

    void OnMuteUsers(IEnumerable<string> usersId, bool mute)
    {
        using (var iterator = usersId.GetEnumerator())
        {
            while (iterator.MoveNext())
            {
                OnMuteUser(iterator.Current, mute);
            }
        }
    }

    void OnMuteAll(bool mute)
    {
        isMuteAll = mute;

        if (mute)
        {
            usersToUnmute.Clear();
        }
        else
        {
            usersToMute.Clear();
        }
        OnMuteUsers(trackedUsersHashSet, mute);
    }

    void ReportMuteStatuses()
    {
        if (usersToUnmute.Count > 0)
        {
            WebInterface.SetMuteUsers(usersToUnmute.ToArray(), false);
        }
        if (usersToMute.Count > 0)
        {
            WebInterface.SetMuteUsers(usersToMute.ToArray(), true);
        }
        usersToUnmute.Clear();
        usersToMute.Clear();
    }

    IEnumerator MuteStateUpdateRoutine()
    {
        yield return WaitForSecondsCache.Get(MUTE_STATUS_UPDATE_INTERVAL);
        ReportMuteStatuses();
        updateMuteStatusRoutine = null;
    }
}
