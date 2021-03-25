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
            scenesViewController.AddListener(deployedSceneListener);
        }

        if (sectionBase is IProjectSceneListener projectSceneListener)
        {
            scenesViewController.AddListener(projectSceneListener);
        }

        if (sectionBase is ISelectSceneListener selectSceneListener)
        {
            scenesViewController.AddListener(selectSceneListener);
        }

        searchBarView.SetSearchBar(sectionBase.searchHandler, sectionBase.searchBarConfig);
    }

    void OnSectionHide(SectionBase sectionBase)
    {
        if (sectionBase is IDeployedSceneListener deployedSceneListener)
        {
            scenesViewController.RemoveListener(deployedSceneListener);
        }

        if (sectionBase is IProjectSceneListener projectSceneListener)
        {
            scenesViewController.RemoveListener(projectSceneListener);
        }
        
        if (sectionBase is ISelectSceneListener selectSceneListener)
        {
            scenesViewController.RemoveListener(selectSceneListener);
        }        

        searchBarView.SetSearchBar(null, null);
    }
    
    void OnSelectScene(SceneCardView sceneCardView)
    {
        sectionsController.OpenSection(SectionsController.SectionId.SETTINGS_PROJECT_GENERAL);
    }
}
