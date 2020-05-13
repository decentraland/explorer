using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TaskbarHUDView : MonoBehaviour
{
    const string VIEW_PATH = "Taskbar";

    public RectTransform windowContainer;
    public CanvasGroup windowContainerCanvasGroup;
    public LayoutGroup windowContainerLayout;

    public TaskbarButton chatButton;
    public TaskbarButton friendsButton;

    public ChatHeadsGroupView chatHeadsGroup;
    public List<TaskbarButton> taskbarButtonList = new List<TaskbarButton>();
    internal TaskbarHUDController controller;

    void RefreshButtonList()
    {
        taskbarButtonList = new List<TaskbarButton>();
        taskbarButtonList.Add(chatButton);
        taskbarButtonList.Add(friendsButton);
        taskbarButtonList.AddRange(chatHeadsGroup.chatHeads);
    }

    internal static TaskbarHUDView Create(TaskbarHUDController controller, IChatController chatController)
    {
        var view = Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<TaskbarHUDView>();
        view.Initialize(controller, chatController);
        return view;
    }

    public void Initialize(TaskbarHUDController controller, IChatController chatController)
    {
        this.controller = controller;
        chatHeadsGroup.Initialize(chatController);
        RefreshButtonList();
    }

    internal void OnAddChatWindow(UnityAction onToggle)
    {
        chatButton.gameObject.SetActive(true);
        chatButton.openButton.onClick.AddListener(onToggle);
    }

    internal void OnAddFriendsWindow(UnityAction onToggle)
    {
        friendsButton.gameObject.SetActive(true);
        friendsButton.openButton.onClick.AddListener(onToggle);
    }

    internal void OnAddPrivateMessageButton()
    {
    }

    public void SetVisibility(bool visible)
    {
        gameObject.SetActive(visible);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            controller.OnPressReturn();
        }
    }
}
