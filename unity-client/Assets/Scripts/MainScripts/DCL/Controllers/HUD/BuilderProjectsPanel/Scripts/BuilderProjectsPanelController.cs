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

    public BuilderProjectsPanelController() : this(
        Object.Instantiate(Resources.Load<BuilderProjectsPanelView>("BuilderProjectsPanel")))
    {
    }

    public void Dispose()
    {
        SceneCardView.OnContextMenuPressed -= OnContextMenuOpen;
        view.contextMenu.OnSettingsPressed -= OnContextMenuSettingsPressed;
        view.contextMenu.OnDuplicatePressed -= OnContextMenuDuplicatePressed;
        view.contextMenu.OnDownloadPressed -= OnContextMenuDownloadPressed;
        view.contextMenu.OnSharePressed -= OnContextMenuSharePressed;
        view.contextMenu.OnUnpublishPressed -= OnContextMenuUnpublishPressed;
        view.contextMenu.OnDeletePressed -= OnContextMenuDeletePressed;
        view.contextMenu.OnQuitContributorPressed -= OnContextMenuQuitContributorPressed;

        if (bridge != null)
        {
            bridge.OnProjectsSet -= OnProjectsUpdated;
        }

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

        view.OnScenesToggleChanged += OnSceneToggleChanged;
        view.OnInWorldScenesToggleChanged += OnInWorldScenesToggleChanged;
        view.OnProjectsToggleChanged += OnProjectsToggleChanged;
        view.OnLandToggleChanged += OnLandToggleChanged;

        sectionsController.OnSectionShow += OnSectionShow;
        sectionsController.OnSectionHide += OnSectionHide;
        sectionsController.OnRequestContextMenuHide += () => view.contextMenu.Hide();
        sectionsController.OnRequestOpenSection += OnRequestOpenSection;

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

        SceneCardView.OnContextMenuPressed += OnContextMenuOpen;

        view.contextMenu.OnSettingsPressed += OnContextMenuSettingsPressed;
        view.contextMenu.OnDuplicatePressed += OnContextMenuDuplicatePressed;
        view.contextMenu.OnDownloadPressed += OnContextMenuDownloadPressed;
        view.contextMenu.OnSharePressed += OnContextMenuSharePressed;
        view.contextMenu.OnUnpublishPressed += OnContextMenuUnpublishPressed;
        view.contextMenu.OnDeletePressed += OnContextMenuDeletePressed;
        view.contextMenu.OnQuitContributorPressed += OnContextMenuQuitContributorPressed;

        if (bridge != null)
        {
            bridge.OnProjectsSet += OnProjectsUpdated;
            bridge.SendFetchProjects();
        }
    }

    void OnProjectsUpdated(string payload)
    {
        if (scenesViewController != null)
        {
            var scenes = Utils.ParseJsonArray<SceneData[]>(payload);
            scenesViewController.SetScenes(scenes);
        }
    }

    void OnContextMenuOpen(ISceneData sceneData, SceneCardView sceneCard)
    {
        view.contextMenu.transform.position = sceneCard.contextMenuButton.transform.position;
        view.contextMenu.Show(sceneData.id, sceneData.isDeployed,
            sceneData.isOwner || sceneData.isOperator, sceneData.isContributor);
    }

    void OnContextMenuSettingsPressed(string id)
    {
    }

    void OnContextMenuDuplicatePressed(string id)
    {
        bridge?.SendDuplicateProject(id);
    }

    void OnContextMenuDownloadPressed(string id)
    {
        bridge?.SendDownload(id);
    }

    void OnContextMenuSharePressed(string id)
    {
        bridge?.SendShare(id);
    }

    void OnContextMenuUnpublishPressed(string id)
    {
        bridge?.SendUnPublish(id);
    }

    void OnContextMenuDeletePressed(string id)
    {
        bridge?.SendDelete(id);
    }

    void OnContextMenuQuitContributorPressed(string id)
    {
        bridge?.SendQuitContributor(id);
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