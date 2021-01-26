using System;
using UnityEngine;
using UnityEngine.UI;

internal class BuilderProjectsPanelView : MonoBehaviour
{
    [SerializeField] internal Button closeButton;
    [SerializeField] internal Button createSceneButton;
    [SerializeField] internal Button importSceneButton;

    [SerializeField] internal LeftMenuButtonToggleView scenesToggle;
    [SerializeField] internal LeftMenuButtonToggleView inWorldScenesToggle;
    [SerializeField] internal LeftMenuButtonToggleView projectsToggle;
    [SerializeField] internal LeftMenuButtonToggleView landToggle;

    private void Awake()
    {
        closeButton.onClick.AddListener(OnClosePressed);
        createSceneButton.onClick.AddListener(OnCreateScenePressed);
        importSceneButton.onClick.AddListener(OnImportScenePressed);

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

    private void OnClosePressed()
    {
    }

    private void OnCreateScenePressed()
    {
    }

    private void OnImportScenePressed()
    {
    }

    private void OnScenesToggleChanged(bool isOn)
    {
    }

    private void OnInWorldScenesToggleChanged(bool isOn)
    {
    }

    private void OnProjectsToggleChanged(bool isOn)
    {
    }

    private void OnLandToggleChanged(bool isOn)
    {
    }
}
