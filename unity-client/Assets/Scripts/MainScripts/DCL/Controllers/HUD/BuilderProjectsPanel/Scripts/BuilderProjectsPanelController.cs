using DCL.Helpers;
using System;
using UnityEngine;
using Object = UnityEngine.Object;

public class BuilderProjectsPanelController : IDisposable
{
    internal readonly BuilderProjectsPanelView view;
    internal readonly SectionsController sectionsController;
    internal readonly ScenesViewController scenesViewController;
    internal readonly LandController landsController;

    internal BuilderProjectsPanelBridge bridge = null;

    internal readonly SectionsHandler sectionsHandler;
    internal readonly SceneContextMenuHandler sceneContextMenuHandler;
    internal readonly LeftMenuHandler leftMenuHandler;
    internal readonly LeftMenuSettingsViewHandler leftMenuSettingsViewHandler;


    public BuilderProjectsPanelController() : this(
        Object.Instantiate(Resources.Load<BuilderProjectsPanelView>("BuilderProjectsPanel")))
    {
    }

    public void Dispose()
    {
        if (bridge != null)
        {
            bridge.OnProjectsSet -= OnProjectsUpdated;
            bridge.OnLandsSet -= OnLandsUpdated;
        }
        sectionsController.OnRequestUpdateSceneData -= OnRequestUpdateSceneData;
        sectionsController.OnRequestUpdateSceneContributors -= OnRequestUpdateSceneContributors;
        sectionsController.OnRequestUpdateSceneAdmins -= OnRequestUpdateSceneAdmins;
        sectionsController.OnRequestUpdateSceneBannedUsers -= OnRequestUpdateSceneBannedUsers;

        leftMenuSettingsViewHandler.Dispose();
        sectionsHandler.Dispose();
        sceneContextMenuHandler.Dispose();
        leftMenuHandler.Dispose();

        sectionsController.Dispose();
        scenesViewController.Dispose();

        if (view != null)
            Object.Destroy(view.gameObject);
    }

    internal BuilderProjectsPanelController(BuilderProjectsPanelView view)
    {
        bridge = BuilderProjectsPanelBridge.i;
        if (BuilderProjectsPanelBridge.mockData && bridge == null)
        {
            bridge = new GameObject("_BuilderProjectsPanelBridge").AddComponent<BuilderProjectsPanelBridge>();
        }

        this.view = view;
        view.name = "_BuilderProjectsPanel";

        sectionsController = new SectionsController(view.sectionsContainer);
        scenesViewController = new ScenesViewController(view.sceneCardViewPrefab);
        landsController = new LandController();

        SetView();

        if (bridge != null)
        {
            bridge.OnProjectsSet += OnProjectsUpdated;
            bridge.OnLandsSet += OnLandsUpdated;
            bridge.SendFetchProjects();
            bridge.SendFetchLands();
            sectionsController.OnRequestUpdateSceneData += OnRequestUpdateSceneData;
            sectionsController.OnRequestUpdateSceneContributors += OnRequestUpdateSceneContributors;
            sectionsController.OnRequestUpdateSceneAdmins += OnRequestUpdateSceneAdmins;
            sectionsController.OnRequestUpdateSceneBannedUsers += OnRequestUpdateSceneBannedUsers;
        }

        leftMenuSettingsViewHandler = new LeftMenuSettingsViewHandler(scenesViewController, view.settingsViewReferences);
        sectionsHandler = new SectionsHandler(sectionsController, scenesViewController, view.searchBarView, landsController);
        leftMenuHandler = new LeftMenuHandler(view, sectionsController);
        sceneContextMenuHandler = new SceneContextMenuHandler(view.contextMenu, sectionsController,
            scenesViewController, bridge);

        sectionsController.OpenSection(SectionsController.SectionId.SCENES_MAIN);
    }

    void SetView()
    {
        scenesViewController.AddListener((IDeployedSceneListener) view);
        scenesViewController.AddListener((IProjectSceneListener) view);
    }

    void OnProjectsUpdated(string payload)
    {
        if (scenesViewController != null)
        {
            var scenes = Utils.ParseJsonArray<SceneData[]>(payload);
            scenesViewController.SetScenes(scenes);
        }
    }

    void OnLandsUpdated(string payload)
    {
        if (landsController != null)
        {
            var lands = Utils.ParseJsonArray<LandData[]>(payload);
            landsController.SetLands(lands);
        }
    }

    void OnRequestUpdateSceneData(string id, SceneDataUpdatePayload dataUpdatePayload)
    {
        bridge?.SendSceneDataUpdate(id, dataUpdatePayload);
    }

    void OnRequestUpdateSceneContributors(string id, SceneContributorsUpdatePayload payload)
    {
        bridge?.SendSceneContributorsUpdate(id, payload);
    }
    
    void OnRequestUpdateSceneAdmins(string id, SceneAdminsUpdatePayload payload)
    {
        bridge?.SendSceneAdminsUpdate(id, payload);
    }
    
    void OnRequestUpdateSceneBannedUsers(string id, SceneBannedUsersUpdatePayload payload)
    {
        bridge?.SendSceneBannedUsersUpdate(id, payload);
    }
}