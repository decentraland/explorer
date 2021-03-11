using System;
using System.Collections.Generic;
using DCL.Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

internal class SectionSceneContributorsSettingsView : MonoBehaviour
{
    [SerializeField] internal UsersSearchPromptView usersSearchPromptView;
    [SerializeField] internal Button addUserButton;
    [SerializeField] internal UserElementView userElementView;
    [SerializeField] internal Transform usersContainer;
    [SerializeField] internal GameObject emptyListContainer;
    [SerializeField] internal TextMeshProUGUI labelContributor;

    public event Action OnAddUserPressed;

    private readonly Dictionary<string, UserElementView> userElementViews = new Dictionary<string, UserElementView>();
    private readonly Queue<UserElementView> userElementViewsPool = new Queue<UserElementView>();

    private string contributorLabelFormat;

    private void Awake()
    {
        addUserButton.onClick.AddListener(()=> OnAddUserPressed?.Invoke());
        PoolView(userElementView);
        contributorLabelFormat = labelContributor.text;
    }
    
    public void SetParent(Transform parent)
    {
        transform.SetParent(parent);
        transform.ResetLocalTRS();
    }

    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }

    public UsersSearchPromptView GetSearchPromptView()
    {
        return usersSearchPromptView;
    }
    
    public void SetEmptyList(bool isEmpty)
    {
        usersContainer.gameObject.SetActive(!isEmpty);
        emptyListContainer.SetActive(isEmpty);
    }

    public void SetContributorsCount(int count)
    {
        labelContributor.text = string.Format(contributorLabelFormat, count);
    }

    public void AddUser(UserProfile profile)
    {
        if (!userElementViews.TryGetValue(profile.userId, out UserElementView view))
        {
            view = GetView();
            view.SetUserProfile(profile);
            view.SetAlwaysHighlighted(false);
            view.SetIsAdded(true);
            view.SetActive(true);
        }
        
        bool isBlocked = UserProfile.GetOwnUserProfile().blocked.Contains(profile.userId);
        view.SetBlocked(isBlocked);
    }

    void PoolView(UserElementView view)
    {
        view.SetActive(false);
        userElementViewsPool.Enqueue(view);
    }

    UserElementView GetView()
    {
        UserElementView userView;

        if (userElementViewsPool.Count > 0)
        {
            userView = userElementViewsPool.Dequeue();
        }
        else
        {
            userView = Instantiate(userElementView, usersContainer);
        }

        return userView;
    }
}
