using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

internal class SectionSceneContributorsSettingsController : SectionBase, ISelectSceneListener, 
                                                            ISectionUpdateSceneDataRequester, ISectionUpdateSceneContributorsRequester
{
    public const string VIEW_PREFAB_PATH = "BuilderProjectsPanelMenuSections/SectionSceneContributorsSettingsView";
    
    public event Action<string, SceneDataUpdatePayload> OnRequestUpdateSceneData;
    public event Action<string, SceneContributorsUpdatePayload> OnRequestUpdateSceneContributors;

    private readonly SectionSceneContributorsSettingsView view;
    private readonly UsersSearchPromptController usersSearchPromptController;
    private readonly UserProfileFetcher profileFetcher = new UserProfileFetcher();
    private readonly SceneContributorsUpdatePayload contributorsUpdatePayload = new SceneContributorsUpdatePayload();

    private List<string> contributors = new List<string>();
    private string sceneId;

    public SectionSceneContributorsSettingsController() : this(
        Object.Instantiate(Resources.Load<SectionSceneContributorsSettingsView>(VIEW_PREFAB_PATH)),
        FriendsController.i
    )
    {
    }
    
    public SectionSceneContributorsSettingsController(SectionSceneContributorsSettingsView view, IFriendsController friendsController)
    {
        this.view = view;
        usersSearchPromptController = new UsersSearchPromptController(view.GetSearchPromptView(), friendsController);

        view.OnSearchUserButtonPressed += () => usersSearchPromptController.Show();
        usersSearchPromptController.OnAddUser += OnAddUserPressed;
        usersSearchPromptController.OnRemoveUser += OnRemoveUserPressed;
    }

    public override void Dispose()
    {
        Object.Destroy(view.gameObject);
        profileFetcher.Dispose();
        usersSearchPromptController.Dispose();
    }

    public override void SetViewContainer(Transform viewContainer)
    {
        view.SetParent(viewContainer);
    }

    protected override void OnShow()
    {
        view.SetActive(true);
    }

    protected override void OnHide()
    {
        view.SetActive(false);
    }
    
    void ISelectSceneListener.OnSelectScene(SceneCardView sceneCardView)
    {
        sceneId = sceneCardView.sceneData.id;
        
        if (sceneCardView.sceneData.contributors == null || sceneCardView.sceneData.contributors.Length == 0)
        {
            if (contributors.Count > 0)
                contributors.Clear();
            
            view.SetEmptyList(true);
            view.SetContributorsCount(0);
            return;
        }

        var newContributors = new List<string>(sceneCardView.sceneData.contributors);
        for (int i = 0; i < newContributors.Count; i++)
        {
            AddContributor(newContributors[i]);
            contributors.Remove(newContributors[i]);
        }
        
        for (int i = 0; i < contributors.Count; i++)
        {
            view.RemoveUser(contributors[i]);
        }
        
        contributors = newContributors;

        usersSearchPromptController.SetUsersInRolList(contributors);
        view.SetEmptyList(false);
        view.SetContributorsCount(contributors.Count);
    }

    void AddContributor(string userId)
    {
        if (string.IsNullOrEmpty(userId))
            return;

        var userView = view.AddUser(userId);
        profileFetcher.FetchProfile(userId)
                      .Then(userProfile => userView.SetUserProfile(userProfile));

        userView.OnAddPressed -= OnAddUserPressed;
        userView.OnRemovePressed -= OnRemoveUserPressed;
        userView.OnAddPressed += OnAddUserPressed;
        userView.OnRemovePressed += OnRemoveUserPressed;
    }

    void OnAddUserPressed(string userId)
    {
        if (contributors.Contains(userId))
            return;

        contributors.Add(userId);
        AddContributor(userId);
        usersSearchPromptController.SetUsersInRolList(contributors);
        view.SetEmptyList(false);
        view.SetContributorsCount(contributors.Count);
        contributorsUpdatePayload.contributors = contributors.ToArray();
        OnRequestUpdateSceneContributors?.Invoke(sceneId, contributorsUpdatePayload);
    }
    
    void OnRemoveUserPressed(string userId)
    {
        if (!contributors.Remove(userId))
            return;

        view.RemoveUser(userId);
        usersSearchPromptController.SetUsersInRolList(contributors);
        view.SetEmptyList(contributors.Count == 0);
        view.SetContributorsCount(contributors.Count);
        contributorsUpdatePayload.contributors = contributors.ToArray();
        OnRequestUpdateSceneContributors?.Invoke(sceneId, contributorsUpdatePayload);
    }
}
