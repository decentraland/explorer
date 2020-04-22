using DCL;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WorldChatWindowHUDView : MonoBehaviour, IPointerClickHandler
{
    const string VIEW_PATH = "World Chat Window";

    public Button worldFilterButton;
    public Button pmFilterButton;
    public Button closeButton;

    public ChatHUDView chatHudView;

    public CanvasGroup group;
    public WorldChatWindowHUDController controller;

    public static WorldChatWindowHUDView Create()
    {
        var view = Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<WorldChatWindowHUDView>();
        view.Initialize();
        return view;
    }

    private void Initialize()
    {
        this.closeButton.onClick.AddListener(Toggle);
    }

    public void DeactivatePreview()
    {
        group.alpha = 1;
    }

    public void ActivatePreview()
    {
        group.alpha = 0;
    }

    public void Toggle()
    {
        if (gameObject.activeSelf)
            gameObject.SetActive(false);
        else
        {
            gameObject.SetActive(true);
            DeactivatePreview();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        DeactivatePreview();
    }

    public void Update()
    {
        if (Input.GetKey(KeyCode.Return) && !chatHudView.inputField.isFocused)
        {
            chatHudView.FocusInputField();
            DeactivatePreview();
            InitialSceneReferences.i.mouseCatcher.UnlockCursor();
        }
    }
}
