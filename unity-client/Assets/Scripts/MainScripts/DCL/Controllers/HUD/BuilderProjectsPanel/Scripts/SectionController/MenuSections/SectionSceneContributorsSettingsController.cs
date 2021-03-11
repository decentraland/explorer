using System;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

internal class SectionSceneContributorsSettingsController : SectionBase, ISelectSceneListener, ISectionUpdateSceneDataRequester
{
    public const string VIEW_PREFAB_PATH = "BuilderProjectsPanelMenuSections/SectionSceneContributorsSettingsView";
    
    public event Action<string, SceneUpdatePayload> OnRequestUpdateSceneData;

    private readonly SectionSceneContributorsSettingsView view;
    private readonly UsersSearchPromptController usersSearchPromptController;

    private ISceneData sceneData;

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

        view.OnAddUserPressed += () => usersSearchPromptController.Show();
    }

    public override void Dispose()
    {
        Object.Destroy(view.gameObject);
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
        if (sceneCardView.sceneData.contributors == null || sceneCardView.sceneData.contributors.Length == 0)
        {
            view.SetEmptyList(true);
            view.SetContributorsCount(0);
            return;
        }

        string userId;
        for (int i = 0; i < sceneCardView.sceneData.contributors.Length; i++)
        {
            userId = sceneCardView.sceneData.contributors[i];
            
            if (string.IsNullOrEmpty(userId))
                continue;
            
            UserProfile profile = UserProfileController.userProfilesCatalog.Get(userId);
            if (profile != null)
            {
                view.AddUser(profile);
            }
            else
            {
                Debug.Log($"PROFILE NOT FOUND!!!! {userId}");
            }
        }
        usersSearchPromptController.SetUsersInRolList(sceneCardView.sceneData.contributors);
        view.SetEmptyList(false);
        view.SetContributorsCount(sceneCardView.sceneData.contributors.Length);
    }
}
