using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    public RectTransform mapRenderContainer;

    private void Initialize(MinimapHUDController controller)
    {
        gameObject.name = VIEW_OBJECT_NAME;
        optionsPanel.SetActive(false);

        optionsButton.onClick.AddListener(controller.ToggleOptions);
        addBookmarkButton.onClick.AddListener(controller.AddBookmark);
        reportSceneButton.onClick.AddListener(controller.ReportScene);

        var renderer = FindObjectOfType<MinimapParcelsView>();
        renderer.atlas.SetViewport(mapRenderContainer);
        renderer.transform.SetParent(mapRenderContainer);
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

    public void ToggleOptions()
    {
        optionsPanel.SetActive(!optionsPanel.activeSelf);
    }

    public void SetVisibility(bool visible)
    {
        gameObject.SetActive(visible);
    }
}
