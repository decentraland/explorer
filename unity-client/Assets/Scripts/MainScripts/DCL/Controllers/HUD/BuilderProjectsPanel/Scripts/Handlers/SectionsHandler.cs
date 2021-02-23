using System;

internal class SectionsHandler : IDisposable
{
    private readonly SectionsController sectionsController;
    private readonly ScenesViewController scenesViewController;
    private readonly BuilderProjectsPanelView view;

    public SectionsHandler(SectionsController sectionsController, ScenesViewController scenesViewController, BuilderProjectsPanelView view)
    {
        this.sectionsController = sectionsController;
        this.scenesViewController = scenesViewController;
        this.view = view;

        view.OnScenesToggleChanged += OnSceneToggleChanged;
        view.OnInWorldScenesToggleChanged += OnInWorldScenesToggleChanged;
        view.OnProjectsToggleChanged += OnProjectsToggleChanged;
        view.OnLandToggleChanged += OnLandToggleChanged;

        sectionsController.OnSectionShow += OnSectionShow;
        sectionsController.OnSectionHide += OnSectionHide;
        sectionsController.OnRequestContextMenuHide += OnRequestContextMenuHide;
        sectionsController.OnRequestOpenSection += OnRequestOpenSection;
    }

    public void Dispose()
    {
        view.OnScenesToggleChanged -= OnSceneToggleChanged;
        view.OnInWorldScenesToggleChanged -= OnInWorldScenesToggleChanged;
        view.OnProjectsToggleChanged -= OnProjectsToggleChanged;
        view.OnLandToggleChanged -= OnLandToggleChanged;

        sectionsController.OnSectionShow -= OnSectionShow;
        sectionsController.OnSectionHide -= OnSectionHide;
        sectionsController.OnRequestContextMenuHide -= OnRequestContextMenuHide;
        sectionsController.OnRequestOpenSection -= OnRequestOpenSection;
    }

    void OnSceneToggleChanged(bool isOn)
    {
        if (isOn) sectionsController.OpenSection(SectionsController.SectionId.SCENES_MAIN);
    }

    void OnInWorldScenesToggleChanged(bool isOn)
    {
        if (isOn) sectionsController.OpenSection(SectionsController.SectionId.SCENES_DEPLOYED);
    }

    void OnProjectsToggleChanged(bool isOn)
    {
        if (isOn) sectionsController.OpenSection(SectionsController.SectionId.SCENES_PROJECT);
    }

    void OnLandToggleChanged(bool isOn)
    {
        if (isOn) sectionsController.OpenSection(SectionsController.SectionId.LAND);
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

        view.searchBarView.SetSearchBar(sectionBase.searchHandler, sectionBase.searchBarConfig);
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

        view.searchBarView.SetSearchBar(null, null);
    }

    void OnRequestContextMenuHide()
    {
        view.contextMenu.Hide();
    }

    void OnRequestOpenSection(SectionsController.SectionId id)
    {
        switch (id)
        {
            case SectionsController.SectionId.SCENES_MAIN:
                view.scenesToggle.isOn = true;
                break;
            case SectionsController.SectionId.SCENES_DEPLOYED:
                view.inWorldScenesToggle.isOn = true;
                break;
            case SectionsController.SectionId.SCENES_PROJECT:
                view.projectsToggle.isOn = true;
                break;
            case SectionsController.SectionId.LAND:
                break;
        }
    }
}
