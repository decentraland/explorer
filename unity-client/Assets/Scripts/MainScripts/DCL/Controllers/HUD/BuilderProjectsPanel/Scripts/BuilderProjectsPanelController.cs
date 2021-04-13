using DCL;
using UnityEngine;
using Object = UnityEngine.Object;

public class BuilderProjectsPanelController : IHUD
{
    internal readonly IBuilderProjectsPanelView view;

    private ISectionsController sectionsController;
    private IScenesViewController scenesViewController;
    private ILandController landsController;

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

        leftMenuSettingsViewHandler?.Dispose();
        sectionsHandler?.Dispose();
        sceneContextMenuHandler?.Dispose();
        leftMenuHandler?.Dispose();
        bridgeHandler?.Dispose();

        sectionsController?.Dispose();
        scenesViewController?.Dispose();
        landsController?.Dispose();

        view.Dispose();
    }

    public void Initialize()
    {
        Initialize(BuilderProjectsPanelBridge.i,
            new SectionsController(view.GetSectionContainer()),
            new ScenesViewController(view.GetCardViewPrefab(), view.GetTransform()),
            new LandController(Environment.i.platform.serviceProviders.theGraph));
    }

    internal void Initialize(IBuilderProjectsPanelBridge bridge, ISectionsController sectionsController, 
        IScenesViewController scenesViewController, ILandController landController)
    {
        if (isInitialized)
            return;

        isInitialized = true;

        this.sectionsController = sectionsController;
        this.scenesViewController = scenesViewController;
        this.landsController = landController;

        // set listeners for sections, setup searchbar for section, handle request for opening a new section
        sectionsHandler = new SectionsHandler(sectionsController, scenesViewController, landsController, view.GetSearchBar());
        // handle if main panel or settings panel should be shown in current section
        leftMenuHandler = new LeftMenuHandler(view, sectionsController);
        // handle project scene info on the left menu panel
        leftMenuSettingsViewHandler = new LeftMenuSettingsViewHandler(view.GetSettingsViewReferences(), scenesViewController);
        // handle scene's context menu options
        sceneContextMenuHandler = new SceneContextMenuHandler(view.GetSceneCardViewContextMenu(), sectionsController, scenesViewController, bridge);
        // handle in and out bridge communications
        bridgeHandler = new BridgeHandler(bridge, scenesViewController, landsController, sectionsController);

        SetView();

        //sectionsController.OpenSection(SectionId.SCENES_MAIN);

        DataStore.i.HUDs.builderProjectsPanelVisible.OnChange += OnVisibilityChanged;
    }

    public void SetVisibility(bool visible)
    {
        DataStore.i.HUDs.builderProjectsPanelVisible.Set(visible);
    }

    private void OnVisibilityChanged(bool isVisible, bool prev)
    {
        if (isVisible == prev)
            return;

        view.SetVisible(isVisible);
        
        if (isVisible)
        {
            landsController.FetchLands();
            sectionsController.OpenSection(SectionId.SCENES_MAIN);
        }
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