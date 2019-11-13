﻿using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MinimapHUDView : MonoBehaviour
{
    public const string UNNAMED_SCENE = "Unnamed";
    
    private const string VIEW_PATH = "MinimapHUD";
    private const string VIEW_OBJECT_NAME = "_MinimapHUD";

    [Header("Information")]
    [SerializeField] private TextMeshProUGUI sceneNameText;

    [SerializeField] private TextMeshProUGUI playerPositionText;

    [Header("Options")]
    [SerializeField] private Button optionsButton;

    [SerializeField] private GameObject optionsPanel;

    [SerializeField] private Button addBookmarkButton;

    [SerializeField] private Button reportSceneButton;
    
    [SerializeField] private MinimapZoom minimapZoom;

    private MinimapHUDController controller;

    private void Initialize(MinimapHUDController controller)
    {
        this.controller = controller;
        gameObject.name = VIEW_OBJECT_NAME;
        optionsPanel.SetActive(false);

        optionsButton.onClick.AddListener(controller.ToggleOptions);
        addBookmarkButton.onClick.AddListener(controller.AddBookmark);
        reportSceneButton.onClick.AddListener(controller.ReportScene);
        minimapZoom.OnZoom += (relativeZoom) => controller.AddZoomDelta(relativeZoom);
    }

    internal static MinimapHUDView Create(MinimapHUDController controller)
    {
        var view = Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<MinimapHUDView>();
        view.Initialize(controller);
        return view;
    }

    internal void UpdateData(MinimapHUDModel model)
    {
        sceneNameText.text = string.IsNullOrEmpty(model.sceneName) ? UNNAMED_SCENE : model.sceneName;
        playerPositionText.text = model.playerPosition;
    }

    public void ToggleOptions()
    {
        optionsPanel.SetActive(!optionsPanel.activeSelf);
    }

    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }
}
