using DCL.Helpers;
using System;
using UnityEngine;
using Object = UnityEngine.Object;

public class BuilderProjectsPanelController : IDisposable
{
    private readonly BuilderProjectsPanelView view;

    private SectionsController sectionsController;
    private ScenesViewController scenesViewController;
    private LandController landsController;

    private SectionsHandler sectionsHandler;
    private SceneContextMenuHandler sceneContextMenuHandler;
    private LeftMenuHandler leftMenuHandler;
    private LeftMenuSettingsViewHandler leftMenuSettingsViewHandler;
    private BridgeHandler bridgeHandler;

    private bool isInitialized = false;

    public BuilderProjectsPanelController() : this(
        Object.Instantiate(Resources.Load<BuilderProjectsPanelView>("BuilderProjectsPanel"))) { }

    internal BuilderProjectsPanelController(BuilderProjectsPanelView view)
    {
        this.view = view;
        view.name = "_BuilderProjectsPanel";
    }

    public void Dispose()
    {
        leftMenuSettingsViewHandler.Dispose();
        sectionsHandler.Dispose();
        sceneContextMenuHandler.Dispose();
        leftMenuHandler.Dispose();
        bridgeHandler.Dispose();

        sectionsController.Dispose();
        scenesViewController.Dispose();

        if (view != null)
            Object.Destroy(view.gameObject);
    }

    public void Initialize()
    {
        Initialize(BuilderProjectsPanelBridge.i);
    }

    public void Initialize(IBuilderProjectsPanelBridge bridge)
    {
        if (isInitialized)
            return;
        
        isInitialized = true;
        
        sectionsController = new SectionsController(view.sectionsContainer);
        scenesViewController = new ScenesViewController(view.sceneCardViewPrefab);
        landsController = new LandController();

        sectionsHandler = new SectionsHandler(sectionsController, scenesViewController, landsController, view.searchBarView);
        leftMenuHandler = new LeftMenuHandler(view, sectionsController);
        leftMenuSettingsViewHandler = new LeftMenuSettingsViewHandler(view.settingsViewReferences, scenesViewController);
        sceneContextMenuHandler = new SceneContextMenuHandler(view.contextMenu, sectionsController, scenesViewController, bridge);
        bridgeHandler = new BridgeHandler(bridge, scenesViewController, landsController, sectionsController);

        SetView();

        sectionsController.OpenSection(SectionsController.SectionId.SCENES_MAIN);
        
        bridge.SendFetchProjects();
        bridge.SendFetchLands();
    }

    private void SetView()
    {
        scenesViewController.AddListener((IDeployedSceneListener) view);
        scenesViewController.AddListener((IProjectSceneListener) view);
    }
}