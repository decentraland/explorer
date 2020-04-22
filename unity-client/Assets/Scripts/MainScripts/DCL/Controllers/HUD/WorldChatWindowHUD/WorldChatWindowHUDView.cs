using UnityEngine;
using UnityEngine.UI;

public class WorldChatWindowHUDView : MonoBehaviour
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

    public void OnMouseUnlock()
    {
        group.alpha = 1;
    }

    public void OnMouseLock()
    {
        group.alpha = 0;
    }

    public void Toggle()
    {
        if (gameObject.activeSelf)
            gameObject.SetActive(false);
        else
            gameObject.SetActive(true);
    }
}
