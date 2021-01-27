using System;
using UnityEngine;
using UnityEngine.UI;

internal class BuilderProjectsPanelView : MonoBehaviour
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

    public void SetScenesSubMenu(bool hasDeployedScenes, bool hasProjectScenes)
    {
        inWorldScenesToggle.gameObject.SetActive(hasDeployedScenes);
        projectsToggle.gameObject.SetActive(hasProjectScenes);
    }

    private void MOCKUP()
    {
        SectionsController controller = new SectionsController(sectionViewFactory, sectionsContainer);

        OnScenesToggleChanged += (isOn) =>
        {
            if (isOn) controller.OpenSection(SectionsController.SectionId.SCENES_MAIN);
        };
        OnInWorldScenesToggleChanged += (isOn) =>
        {
            if (isOn) controller.OpenSection(SectionsController.SectionId.SCENES_DEPLOYED);
        };
        OnProjectsToggleChanged += (isOn) =>
        {
            if (isOn) controller.OpenSection(SectionsController.SectionId.SCENES_PROJECT);
        };
        OnLandToggleChanged += (isOn) =>
        {
            if (isOn) controller.OpenSection(SectionsController.SectionId.LAND);
        };
    }
}
