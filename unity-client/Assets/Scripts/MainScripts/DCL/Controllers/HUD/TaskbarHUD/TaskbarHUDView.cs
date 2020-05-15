using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TaskbarHUDView : MonoBehaviour
{
    const string VIEW_PATH = "Taskbar";

    [SerializeField] internal RectTransform windowContainer;
    [SerializeField] internal CanvasGroup windowContainerCanvasGroup;
    [SerializeField] internal LayoutGroup windowContainerLayout;

    [SerializeField] internal TaskbarButton chatButton;
    [SerializeField] internal TaskbarButton friendsButton;

    [SerializeField] internal ChatHeadGroupView chatHeadsGroup;
    internal List<TaskbarButton> taskbarButtonList = new List<TaskbarButton>();
    internal TaskbarHUDController controller;

    internal void RefreshButtonList()
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
        chatButton.Initialize();
        friendsButton.Initialize();

        RefreshButtonList();

        chatHeadsGroup.OnHeadToggleOn += OnWindowToggleOn;
        chatHeadsGroup.OnHeadToggleOff += OnWindowToggleOff;

        chatButton.OnToggleOn += OnWindowToggleOn;
        chatButton.OnToggleOff += OnWindowToggleOff;

        friendsButton.OnToggleOn += OnWindowToggleOn;
        friendsButton.OnToggleOff += OnWindowToggleOff;

        chatButton.SetToggleState(true);
    }

    public event System.Action OnChatToggleOn;
    public event System.Action OnChatToggleOff;
    public event System.Action OnFriendsToggleOn;
    public event System.Action OnFriendsToggleOff;

    private void OnWindowToggleOff(TaskbarButton obj)
    {
        if (obj == friendsButton)
            OnFriendsToggleOff?.Invoke();
        else if (obj == chatButton)
            OnChatToggleOff?.Invoke();

        obj.SetToggleState(false, useCallback: true);


        RefreshButtonList();

        bool anyVisible = false;

        foreach (var btn in taskbarButtonList)
        {
            if (btn.toggledOn)
                anyVisible = true;
        }

        if (!anyVisible)
        {
            chatButton.SetToggleState(true);
        }
    }

    private void OnWindowToggleOn(TaskbarButton obj)
    {
        if (obj == friendsButton)
            OnFriendsToggleOn?.Invoke();
        else if (obj == chatButton)
            OnChatToggleOn?.Invoke();

        SelectButton(obj);
    }

    void SelectButton(TaskbarButton obj)
    {
        RefreshButtonList();

        foreach (var btn in taskbarButtonList)
        {
            if (btn != obj)
            {
                btn.SetToggleState(false, useCallback: true);
            }
        }
    }

    internal void OnAddChatWindow()
    {
        chatButton.gameObject.SetActive(true);
    }

    internal void OnAddFriendsWindow()
    {
        friendsButton.gameObject.SetActive(true);
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

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            controller.OnPressEsc();
        }
    }
}
