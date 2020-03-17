using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DCL.Helpers;

public class MinimapHUDView : MonoBehaviour
{
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

    [Header("NavMap")]
    [SerializeField] internal InputAction_Trigger toggleNavMapAction;
    [SerializeField] private GameObject navMapGameObject;
    internal InputAction_Trigger.Triggered toggleNavMapDelegate;

    private MinimapHUDController controller;

    void Awake()
    {
        toggleNavMapDelegate = (x) => { ToggleNavMap(); };
        toggleNavMapAction.OnTriggered += toggleNavMapDelegate;
    }

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
        sceneNameText.text = string.IsNullOrEmpty(model.sceneName) ? "Unnamed" : model.sceneName;
        playerPositionText.text = model.playerPosition;
    }

    internal void ToggleNavMap()
    {
        navMapGameObject.SetActive(!navMapGameObject.activeSelf);

        if (navMapGameObject.activeSelf)
            DCL.Helpers.Utils.UnlockCursor();
        else
            DCL.Helpers.Utils.LockCursor();
    }

    public void ToggleOptions()
    {
        optionsPanel.SetActive(!optionsPanel.activeSelf);
    }

    public void SetVisibility(bool visible)
    {
        gameObject.SetActive(visible);
    }
}