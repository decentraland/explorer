using System;
using System.Collections.Generic;

internal class UsersSearchPromptController: IDisposable
{
    private readonly UsersSearchPromptView view;
    
    private readonly SearchHandler<UserElementView> searchHandler = new SearchHandler<UserElementView>();
    private readonly UsersSearchFriendsHandler friendsHandler;
    private readonly UsersSearchUserViewsHandler userViewsHandler;
    
    public UsersSearchPromptController(UsersSearchPromptView promptView, IFriendsController friendsController)
    {
        view = promptView;
        
        friendsHandler = new UsersSearchFriendsHandler(friendsController);
        userViewsHandler = new UsersSearchUserViewsHandler(view.GetUsersBaseElement(), view.GetUserElementsParent());

        friendsHandler.OnFriendRemoved += OnFriendRemoved;
        searchHandler.OnSearchChanged += OnSearchResult;
        view.OnSearchText += OnSearchText;
        view.OnShouldHide += OnShouldHidePrompt;
    }
    
    public void Dispose()
    {
        friendsHandler.OnFriendRemoved -= OnFriendRemoved;
        searchHandler.OnSearchChanged -= OnSearchResult;
        view.OnSearchText -= OnSearchText;
        view.OnShouldHide -= OnShouldHidePrompt;

        friendsHandler.Dispose();
        userViewsHandler.Dispose();
        view.Dispose();
    }

    public void Show()
    {
        view.ClearSearch();
        view.SetFriendListEmpty(userViewsHandler.userElementViewCount == 0);
        
        if (friendsHandler.isFriendlistDirty)
        {
            friendsHandler.GetFriendList().Then(OnFriendList);
        }
        
        view.Show();
    }

    public void Hide()
    {
        view.Hide();
    }

    public void SetUsersInRolList(List<string> usersId)
    {
        userViewsHandler.SetUsersInRolList(usersId);
    }

    private void OnSearchText(string searchText)
    {
        searchHandler.NotifySearchChanged(searchText);
    }

    private void OnShouldHidePrompt()
    {
        Hide();
    }

    private void OnSearchResult(List<UserElementView> viewsList)
    {
        userViewsHandler.SetVisibleList(viewsList);
    }

    private void OnFriendRemoved(string userId)
    {
        userViewsHandler.RemoveUserView(userId);
    }

    private void OnFriendList(Dictionary<string, FriendsController.UserStatus> friendsDictionary)
    {
        List<UserProfile> profiles = new List<UserProfile>();
        
        foreach (KeyValuePair<string, FriendsController.UserStatus> keyValuePair in friendsDictionary)
        {
            if (keyValuePair.Value.friendshipStatus != FriendshipStatus.FRIEND)
                continue;
                    
            UserProfile profile = UserProfileController.userProfilesCatalog.Get(keyValuePair.Key);
            if (profile)
            {
                profiles.Add(profile);
            }
        }
        
        userViewsHandler.SetUserViewsList(profiles);
        view.SetFriendListEmpty(userViewsHandler.userElementViewCount == 0);
        searchHandler.SetSearchableList(userViewsHandler.GetUserElementViews());
    }
}
