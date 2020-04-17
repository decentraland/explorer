using UnityEngine;
using UnityEngine.UI;

public class TaskbarHUDView : MonoBehaviour
{
    const string VIEW_PATH = "Taskbar";

    public RectTransform windowContainer;
    public Button chatButton;
    public Button friendsButton;

    TaskbarHUDController controller;

    internal static TaskbarHUDView Create(TaskbarHUDController controller)
    {
        var view = Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<TaskbarHUDView>();
        view.Initialize(controller);
        return view;
    }

    public void Initialize(TaskbarHUDController controller)
    {
        this.controller = controller;
    }

    internal void OnAddChatWindow()
    {
        chatButton.gameObject.SetActive(true);
    }

    public void SetVisibility(bool visible)
    {
        gameObject.SetActive(visible);
    }

}
