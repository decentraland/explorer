using System;

internal class SectionsHandler : IDisposable
{
    private readonly SectionsController sectionsController;
    private readonly ScenesViewController scenesViewController;
    private readonly SearchBarView searchBarView;

    public SectionsHandler(SectionsController sectionsController, ScenesViewController scenesViewController, SearchBarView searchBarView)
    {
        this.sectionsController = sectionsController;
        this.scenesViewController = scenesViewController;
        this.searchBarView = searchBarView;

        sectionsController.OnSectionShow += OnSectionShow;
        sectionsController.OnSectionHide += OnSectionHide;
        scenesViewController.OnSceneSelected += OnSelectScene;
    }

    public void Dispose()
    {
        sectionsController.OnSectionShow -= OnSectionShow;
        sectionsController.OnSectionHide -= OnSectionHide;
        scenesViewController.OnSceneSelected -= OnSelectScene;
    }

    void OnSectionShow(SectionBase sectionBase)
    {
        if (sectionBase is IDeployedSceneListener deployedSceneListener)
        {
            scenesViewController.OnDeployedSceneAdded += deployedSceneListener.OnSceneAdded;
            scenesViewController.OnDeployedSceneRemoved += deployedSceneListener.OnSceneRemoved;
            scenesViewController.OnDeployedScenesSet += deployedSceneListener.OnSetScenes;
            deployedSceneListener.OnSetScenes(scenesViewController.deployedScenes);
        }

        if (sectionBase is IProjectSceneListener projectSceneListener)
        {
            scenesViewController.OnProjectSceneAdded += projectSceneListener.OnSceneAdded;
            scenesViewController.OnProjectSceneRemoved += projectSceneListener.OnSceneRemoved;
            scenesViewController.OnProjectScenesSet += projectSceneListener.OnSetScenes;
            projectSceneListener.OnSetScenes(scenesViewController.projectScenes);
        }

        if (sectionBase is ISelectSceneListener selectSceneListener)
        {
            scenesViewController.OnSceneSelected += selectSceneListener.OnSelectScene;
            selectSceneListener.OnSelectScene(scenesViewController.selectedScene);
        }

        searchBarView.SetSearchBar(sectionBase.searchHandler, sectionBase.searchBarConfig);
    }

    void OnSectionHide(SectionBase sectionBase)
    {
        if (sectionBase is IDeployedSceneListener deployedSceneListener)
        {
            scenesViewController.OnDeployedSceneAdded -= deployedSceneListener.OnSceneAdded;
            scenesViewController.OnDeployedSceneRemoved -= deployedSceneListener.OnSceneRemoved;
            scenesViewController.OnDeployedScenesSet -= deployedSceneListener.OnSetScenes;
        }

        if (sectionBase is IProjectSceneListener projectSceneListener)
        {
            scenesViewController.OnProjectSceneAdded -= projectSceneListener.OnSceneAdded;
            scenesViewController.OnProjectSceneRemoved -= projectSceneListener.OnSceneRemoved;
            scenesViewController.OnProjectScenesSet -= projectSceneListener.OnSetScenes;
        }
        
        if (sectionBase is ISelectSceneListener selectSceneListener)
        {
            scenesViewController.OnSceneSelected -= selectSceneListener.OnSelectScene;
        }        

        searchBarView.SetSearchBar(null, null);
    }
    
    void OnSelectScene(ISceneData sceneData)
    {
        sectionsController.OpenSection(SectionsController.SectionId.SETTINGS_PROJECT_GENERAL);
    }
}
