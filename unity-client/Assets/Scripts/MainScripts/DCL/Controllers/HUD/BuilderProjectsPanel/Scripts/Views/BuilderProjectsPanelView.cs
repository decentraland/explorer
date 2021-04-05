using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

internal class BuilderProjectsPanelView : MonoBehaviour, IDeployedSceneListener, IProjectSceneListener
{
    [Header("General")]
    [SerializeField] internal Button closeButton;
    [SerializeField] internal Transform sectionsContainer;
    [SerializeField] internal SceneCardViewContextMenu contextMenu;
    [SerializeField] internal SearchBarView searchBarView;
    [SerializeField] internal ShowHideAnimator showHideAnimator;
    [SerializeField] internal InputAction_Trigger closeTrigger;

    [Header("Left-Panel Section Buttons")]
    [SerializeField] internal LeftMenuButtonToggleView[] sectionToggles;
    [SerializeField] internal LeftMenuButtonToggleView inWorldScenesToggle;
    [SerializeField] internal LeftMenuButtonToggleView projectsToggle;

    [Header("Left-Panel")]
    [SerializeField] internal GameObject leftPanelMain;
    [SerializeField] internal GameObject leftPanelProjectSettings;
    [SerializeField] internal Button createSceneButton;
    [SerializeField] internal Button importSceneButton;
    [SerializeField] internal Button backToMainPanelButton;
    [SerializeField] internal LeftMenuSettingsViewReferences settingsViewReferences;

    [Header("Assets")]
    [SerializeField] internal SceneCardView sceneCardViewPrefab;

    public event Action OnClosePressed;
    public event Action OnCreateScenePressed;
    public event Action OnImportScenePressed;

    private int deployedScenesCount = 0;
    private int projectScenesCount = 0;

    public void SetVisible(bool visible)
    {
        if (visible)
        {
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }
            showHideAnimator.Show();
        }
        else
        {
            showHideAnimator.Hide();
        }
    }

    private void Awake()
    {
        closeButton.onClick.AddListener(() => OnClosePressed?.Invoke());
        createSceneButton.onClick.AddListener(() => OnCreateScenePressed?.Invoke());
        importSceneButton.onClick.AddListener(() => OnImportScenePressed?.Invoke());
        closeTrigger.OnTriggered += CloseTriggerOnOnTriggered;

        contextMenu.Hide();
        gameObject.SetActive(false);
    }

    private void CloseTriggerOnOnTriggered(DCLAction_Trigger action)
    {
        OnClosePressed?.Invoke();
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
}
