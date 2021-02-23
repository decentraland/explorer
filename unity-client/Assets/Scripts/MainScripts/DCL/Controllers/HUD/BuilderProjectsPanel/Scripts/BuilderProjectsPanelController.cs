using DCL.Helpers;
using System;
using UnityEngine;
using Object = UnityEngine.Object;

public class BuilderProjectsPanelController : IDisposable
{
    internal readonly BuilderProjectsPanelView view;
    internal readonly SectionsController sectionsController;
    internal readonly ScenesViewController scenesViewController;

    internal BuilderProjectsPanelBridge bridge = null;

    internal readonly SectionsHandler sectionsHandler;
    internal readonly SceneContextMenuHandler sceneContextMenuHandler;
    internal readonly LeftMenuButtonsHandler leftMenuButtonsHandler;

    public BuilderProjectsPanelController() : this(
        Object.Instantiate(Resources.Load<BuilderProjectsPanelView>("BuilderProjectsPanel")))
    {
    }

    public void Dispose()
    {
        if (bridge != null)
        {
            bridge.OnProjectsSet -= OnProjectsUpdated;
        }

        sectionsHandler.Dispose();
        sceneContextMenuHandler.Dispose();
        leftMenuButtonsHandler.Dispose();

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

        SetView();

        if (bridge != null)
        {
            bridge.OnProjectsSet += OnProjectsUpdated;
            bridge.SendFetchProjects();
        }

        sectionsHandler = new SectionsHandler(sectionsController, scenesViewController, view.searchBarView);
        sceneContextMenuHandler = new SceneContextMenuHandler(view.contextMenu, sectionsController, bridge);
        leftMenuButtonsHandler = new LeftMenuButtonsHandler(view, sectionsController);
    }

    void SetView()
    {
        IDeployedSceneListener viewDeployedSceneListener = view;
        IProjectSceneListener viewProjectSceneListener = view;

        scenesViewController.OnDeployedSceneAdded += viewDeployedSceneListener.OnSceneAdded;
        scenesViewController.OnDeployedSceneRemoved += viewDeployedSceneListener.OnSceneRemoved;
        scenesViewController.OnDeployedScenesSet += viewDeployedSceneListener.OnSetScenes;
        scenesViewController.OnProjectSceneAdded += viewProjectSceneListener.OnSceneAdded;
        scenesViewController.OnProjectSceneRemoved += viewProjectSceneListener.OnSceneRemoved;
        scenesViewController.OnProjectScenesSet += viewProjectSceneListener.OnSetScenes;

        viewDeployedSceneListener.OnSetScenes(scenesViewController.deployedScenes);
        viewProjectSceneListener.OnSetScenes(scenesViewController.projectScenes);
    }

    void OnProjectsUpdated(string payload)
    {
        if (scenesViewController != null)
        {
            var scenes = Utils.ParseJsonArray<SceneData[]>(payload);
            scenesViewController.SetScenes(scenes);
        }
    }
}