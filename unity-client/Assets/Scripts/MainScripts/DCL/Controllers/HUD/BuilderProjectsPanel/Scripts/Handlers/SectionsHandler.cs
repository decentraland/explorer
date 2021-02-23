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
    }

    public void Dispose()
    {
        sectionsController.OnSectionShow -= OnSectionShow;
        sectionsController.OnSectionHide -= OnSectionHide;
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

        searchBarView.SetSearchBar(null, null);
    }
}
