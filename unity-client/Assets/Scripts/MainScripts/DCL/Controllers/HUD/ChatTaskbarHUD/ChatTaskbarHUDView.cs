using UnityEngine;
using UnityEngine.UI;

public class ChatTaskbarHUDView : MonoBehaviour
{
    const string VIEW_PATH = "Taskbar";

    public Button chatButton;
    public Button friendsButton;

    public ChatHUDView chatHUDView;

    ChatTaskbarHUDController controller;

    internal static ChatTaskbarHUDView Create(ChatTaskbarHUDController controller)
    {
        var view = Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<ChatTaskbarHUDView>();
        view.Initialize(controller);
        return view;
    }
    private void Start()
    {
        Initialize(null);
    }

    public void Initialize(ChatTaskbarHUDController controller)
    {
        this.controller = controller;

        var chatHUDController = new ChatHUDController();
        chatHUDController.view = chatHUDView;
        chatHUDView.Initialize(chatHUDController);
    }

    public void SetVisibility(bool visible)
    {
        gameObject.SetActive(visible);
    }

}
