using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

internal class BuilderProjectsPanelView : MonoBehaviour, IDeployedSceneListener, IProjectSceneListener
{
    [Header("References")]
    [SerializeField] internal Button closeButton;
    [SerializeField] internal Button createSceneButton;
    [SerializeField] internal Button importSceneButton;

    [SerializeField] internal Transform sectionsContainer;

    [SerializeField] internal LeftMenuButtonToggleView scenesToggle;
    [SerializeField] internal LeftMenuButtonToggleView inWorldScenesToggle;
    [SerializeField] internal LeftMenuButtonToggleView projectsToggle;
    [SerializeField] internal LeftMenuButtonToggleView landToggle;

    [Header("Prefabs")]
    [SerializeField] internal SceneCardView sceneCardViewPrefab;
    [SerializeField] internal SectionViewFactory sectionViewFactory;

    public event Action OnClosePressed;
    public event Action OnCreateScenePressed;
    public event Action OnImportScenePressed;
    public event Action<bool> OnScenesToggleChanged;
    public event Action<bool> OnInWorldScenesToggleChanged;
    public event Action<bool> OnProjectsToggleChanged;
    public event Action<bool> OnLandToggleChanged;

    private int deployedScenesCount = 0;
    private int projectScenesCount = 0;

    private void Awake()
    {
        MOCKUP();

        closeButton.onClick.AddListener(() => OnClosePressed?.Invoke());
        createSceneButton.onClick.AddListener(() => OnCreateScenePressed?.Invoke());
        importSceneButton.onClick.AddListener(() => OnImportScenePressed?.Invoke());

        scenesToggle.OnToggleValueChanged += OnScenesToggleChanged;
        inWorldScenesToggle.OnToggleValueChanged += OnInWorldScenesToggleChanged;
        projectsToggle.OnToggleValueChanged += OnProjectsToggleChanged;
        landToggle.OnToggleValueChanged += OnLandToggleChanged;
    }

    private void SubmenuScenesDirty()
    {
        inWorldScenesToggle.gameObject.SetActive(deployedScenesCount > 0);
        projectsToggle.gameObject.SetActive(projectScenesCount > 0);
    }

    void IDeployedSceneListener.OnSetScenes(Dictionary<string, SceneCardView> scenes)
    {
        deployedScenesCount = scenes.Count;
        SubmenuScenesDirty();
    }

    void IProjectSceneListener.OnSetScenes(Dictionary<string, SceneCardView> scenes)
    {
        projectScenesCount = scenes.Count;
        SubmenuScenesDirty();
    }

    void IDeployedSceneListener.OnSceneAdded(SceneCardView scene)
    {
        deployedScenesCount++;
        SubmenuScenesDirty();
    }

    void IProjectSceneListener.OnSceneAdded(SceneCardView scene)
    {
        projectScenesCount++;
        SubmenuScenesDirty();
    }

    void IDeployedSceneListener.OnSceneRemoved(SceneCardView scene)
    {
        deployedScenesCount--;
        SubmenuScenesDirty();
    }

    void IProjectSceneListener.OnSceneRemoved(SceneCardView scene)
    {
        projectScenesCount--;
        SubmenuScenesDirty();
    }

    private void MOCKUP()
    {
        SectionsController sectionsController = new SectionsController(sectionViewFactory, sectionsContainer);
        ScenesViewController scenesViewController = new ScenesViewController(sceneCardViewPrefab);

        OnScenesToggleChanged += (isOn) =>
        {
            if (isOn) sectionsController.OpenSection(SectionsController.SectionId.SCENES_MAIN);
        };
        OnInWorldScenesToggleChanged += (isOn) =>
        {
            if (isOn) sectionsController.OpenSection(SectionsController.SectionId.SCENES_DEPLOYED);
        };
        OnProjectsToggleChanged += (isOn) =>
        {
            if (isOn) sectionsController.OpenSection(SectionsController.SectionId.SCENES_PROJECT);
        };
        OnLandToggleChanged += (isOn) =>
        {
            if (isOn) sectionsController.OpenSection(SectionsController.SectionId.LAND);
        };

        sectionsController.OnSectionShow += sectionBase =>
        {
            if (sectionBase is IDeployedSceneListener deployedSceneListener)
            {
                scenesViewController.OnDeployedSceneAdded += deployedSceneListener.OnSceneAdded;
                scenesViewController.OnDeployedSceneRemoved += deployedSceneListener.OnSceneRemoved;
                scenesViewController.OnDeployedScenesSet += deployedSceneListener.OnSetScenes;
                scenesViewController.SetListener(deployedSceneListener);
            }
            if (sectionBase is IProjectSceneListener projectSceneListener)
            {
                scenesViewController.OnProjectSceneAdded += projectSceneListener.OnSceneAdded;
                scenesViewController.OnProjectSceneRemoved += projectSceneListener.OnSceneRemoved;
                scenesViewController.OnProjectScenesSet += projectSceneListener.OnSetScenes;
                scenesViewController.SetListener(projectSceneListener);
            }
        };

        sectionsController.OnSectionHide += sectionBase =>
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
        };

        IDeployedSceneListener thisDeployedSceneListener = this;
        IProjectSceneListener thisProjectSceneListener = this;
        scenesViewController.OnDeployedSceneAdded += thisDeployedSceneListener.OnSceneAdded;
        scenesViewController.OnDeployedSceneRemoved += thisDeployedSceneListener.OnSceneRemoved;
        scenesViewController.OnDeployedScenesSet += thisDeployedSceneListener.OnSetScenes;
        scenesViewController.OnProjectSceneAdded += thisProjectSceneListener.OnSceneAdded;
        scenesViewController.OnProjectSceneRemoved += thisProjectSceneListener.OnSceneRemoved;
        scenesViewController.OnProjectScenesSet += thisProjectSceneListener.OnSetScenes;
        scenesViewController.SetListener(thisDeployedSceneListener);
        scenesViewController.SetListener(thisProjectSceneListener);
    }
}
