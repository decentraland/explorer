using DCL;
using UnityEngine;
using Object = UnityEngine.Object;

public class BuilderProjectsPanelController : IHUD
{
    private readonly IBuilderProjectsPanelView view;

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

    internal BuilderProjectsPanelController(IBuilderProjectsPanelView view)
    {
        this.view = view;
        view.OnClosePressed += OnClose;
    }

    public void Dispose()
    {
        DataStore.i.HUDs.builderProjectsPanelVisible.OnChange -= OnVisibilityChanged;
        view.OnClosePressed -= OnClose;
        
        leftMenuSettingsViewHandler.Dispose();
        sectionsHandler.Dispose();
        sceneContextMenuHandler.Dispose();
        leftMenuHandler.Dispose();
        bridgeHandler.Dispose();

        sectionsController.Dispose();
        scenesViewController.Dispose();

        view.Dispose();
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
        
        sectionsController = new SectionsController(view.GetSectionContainer());
        scenesViewController = new ScenesViewController(view.GetCardViewPrefab());
        landsController = new LandController();

        sectionsHandler = new SectionsHandler(sectionsController, scenesViewController, landsController, view.GetSearchBar());
        leftMenuHandler = new LeftMenuHandler(view, sectionsController);
        leftMenuSettingsViewHandler = new LeftMenuSettingsViewHandler(view.GetSettingsViewReferences(), scenesViewController);
        sceneContextMenuHandler = new SceneContextMenuHandler(view.GetSceneCardViewContextMenu(), sectionsController, scenesViewController, bridge);
        bridgeHandler = new BridgeHandler(bridge, scenesViewController, landsController, sectionsController);

        SetView();

        sectionsController.OpenSection(SectionsController.SectionId.SCENES_MAIN);
        
        bridge.SendFetchProjects();
        bridge.SendFetchLands();

        DataStore.i.HUDs.builderProjectsPanelVisible.OnChange += OnVisibilityChanged;
    }
    
    public void SetVisibility(bool visible)
    {
        DataStore.i.HUDs.builderProjectsPanelVisible.Set(visible);
    }

    private void OnVisibilityChanged(bool current, bool prev)
    {
        if (current == prev)
            return;
        
        view.SetVisible(current);
    }

    private void OnClose()
    {
        SetVisibility(false);
    }

    private void SetView()
    {
        scenesViewController.AddListener((IDeployedSceneListener) view);
        scenesViewController.AddListener((IProjectSceneListener) view);
    }
}