using System;
using System.Collections.Generic;
using UnityEngine;

internal class UsersAroundListHUDListView : MonoBehaviour, IUsersAroundListHUDListView
{
    public event Action<string, bool> OnRequestMuteUser;

    [SerializeField] private UsersAroundListHUDListElementView listElementView;
    [SerializeField] private ShowHideAnimator showHideAnimator;
    [SerializeField] private Transform content;

    private Queue<UsersAroundListHUDListElementView> availableElements;
    private Dictionary<string, UsersAroundListHUDListElementView> userElementDictionary;

    private void Awake()
    {
        availableElements = new Queue<UsersAroundListHUDListElementView>();
        userElementDictionary = new Dictionary<string, UsersAroundListHUDListElementView>();

        listElementView.OnMuteUser += OnMuteUser;
        listElementView.gameObject.SetActive(false);
        availableElements.Enqueue(listElementView);
    }

    void IUsersAroundListHUDListView.AddOrUpdateUser(MinimapMetadata.MinimapUserInfo userInfo)
    {
        if (userElementDictionary.ContainsKey(userInfo.userId))
        {
            return;
        }

        UsersAroundListHUDListElementView view = null;
        if (availableElements.Count > 0)
        {
            view = availableElements.Dequeue();
            view.OnPoolGet();
        }
        else
        {
            view = Instantiate(listElementView, content);
            view.gameObject.SetActive(true);
            view.OnMuteUser += OnMuteUser;
        }

        view.SetUserProfile(userInfo.userId);
        userElementDictionary.Add(userInfo.userId, view);
    }

    void IUsersAroundListHUDListView.RemoveUser(string userId)
    {
        if (!userElementDictionary.TryGetValue(userId, out UsersAroundListHUDListElementView elementView))
        {
            return;
        }

        elementView.OnPoolRelease();
        availableElements.Enqueue(elementView);
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
}
