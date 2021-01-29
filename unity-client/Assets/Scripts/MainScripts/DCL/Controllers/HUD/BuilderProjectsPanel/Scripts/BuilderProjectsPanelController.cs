using UnityEngine;

public class BuilderProjectsPanelController
{
    internal readonly BuilderProjectsPanelView view;
    internal readonly SectionsController sectionsController;
    internal readonly ScenesViewController scenesViewController;

    public BuilderProjectsPanelController() : this(
        Object.Instantiate(Resources.Load<BuilderProjectsPanelView>("BuilderProjectsPanel")))
    {
    }

    internal BuilderProjectsPanelController(BuilderProjectsPanelView view)
    {
        this.view = view;
        view.name = "_BuilderProjectsPanel";

        sectionsController = new SectionsController(view.sectionViewFactory, view.sectionsContainer);
        scenesViewController = new ScenesViewController(view.sceneCardViewPrefab);

        view.OnScenesToggleChanged += OnSceneToggleChanged;
        view.OnInWorldScenesToggleChanged += OnInWorldScenesToggleChanged;
        view.OnProjectsToggleChanged += OnProjectsToggleChanged;
        view.OnLandToggleChanged += OnLandToggleChanged;

        sectionsController.OnSectionShow += OnSectionShow;
        sectionsController.OnSectionHide += OnSectionHide;

        IDeployedSceneListener viewDeployedSceneListener = view;
        IProjectSceneListener viewProjectSceneListener = view;
        scenesViewController.OnDeployedSceneAdded += viewDeployedSceneListener.OnSceneAdded;
        scenesViewController.OnDeployedSceneRemoved += viewDeployedSceneListener.OnSceneRemoved;
        scenesViewController.OnDeployedScenesSet += viewDeployedSceneListener.OnSetScenes;
        scenesViewController.OnProjectSceneAdded += viewProjectSceneListener.OnSceneAdded;
        scenesViewController.OnProjectSceneRemoved += viewProjectSceneListener.OnSceneRemoved;
        scenesViewController.OnProjectScenesSet += viewProjectSceneListener.OnSetScenes;
        scenesViewController.NotifySet(viewDeployedSceneListener);
        scenesViewController.NotifySet(viewProjectSceneListener);
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
            scenesViewController.NotifySet(deployedSceneListener);
        }
        if (sectionBase is IProjectSceneListener projectSceneListener)
        {
            scenesViewController.OnProjectSceneAdded += projectSceneListener.OnSceneAdded;
            scenesViewController.OnProjectSceneRemoved += projectSceneListener.OnSceneRemoved;
            scenesViewController.OnProjectScenesSet += projectSceneListener.OnSetScenes;
            scenesViewController.NotifySet(projectSceneListener);
        }
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
    }
}