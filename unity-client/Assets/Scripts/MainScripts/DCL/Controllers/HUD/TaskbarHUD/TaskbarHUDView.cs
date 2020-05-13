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

    public GameObject chatTooltip;

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
        chatTooltip.SetActive(false);
        chatHeadsGroup.Initialize(chatController);
        CommonScriptableObjects.rendererState.OnChange -= RendererState_OnChange;
        CommonScriptableObjects.rendererState.OnChange += RendererState_OnChange;
        RefreshButtonList();
    }

    private void OnDestroy()
    {
        CommonScriptableObjects.rendererState.OnChange -= RendererState_OnChange;
    }

    private void RendererState_OnChange(bool current, bool previous)
    {
        if (current == previous)
            return;

        if (current && !controller.alreadyToggledOnForFirstTime)
        {
            chatTooltip.SetActive(true);
        }
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

    public void OnToggleForFirstTime()
    {
        //TODO(Brian): Toggle an animator trigger/bool instead of doing this.
        chatTooltip.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            controller.OnPressReturn();
        }
    }
}
