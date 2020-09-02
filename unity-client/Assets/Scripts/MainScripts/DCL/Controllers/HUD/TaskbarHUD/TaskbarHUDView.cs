using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TaskbarHUDView : MonoBehaviour
{
    const string VIEW_PATH = "Taskbar";

    [SerializeField] internal RectTransform windowContainer;
    [SerializeField] internal ShowHideAnimator windowContainerAnimator;
    [SerializeField] internal LayoutGroup windowContainerLayout;

    [SerializeField] internal TaskbarButton chatButton;
    [SerializeField] internal TaskbarButton friendsButton;
    [SerializeField] internal TaskbarButton settingsButton;
    [SerializeField] internal TaskbarButton backpackButton;
    [SerializeField] internal TaskbarButton exploreButton;

    [SerializeField] internal ChatHeadGroupView chatHeadsGroup;

    internal TaskbarHUDController controller;

    public event System.Action OnChatToggleOn;
    public event System.Action OnChatToggleOff;
    public event System.Action OnFriendsToggleOn;
    public event System.Action OnFriendsToggleOff;
    public event System.Action OnSettingsToggleOn;
    public event System.Action OnSettingsToggleOff;
    public event System.Action OnBackpackToggleOn;
    public event System.Action OnBackpackToggleOff;
    public event System.Action OnExploreToggleOn;
    public event System.Action OnExploreToggleOff;

    internal List<TaskbarButton> GetButtonList()
    {
        var taskbarButtonList = new List<TaskbarButton>();
        taskbarButtonList.Add(chatButton);
        taskbarButtonList.Add(friendsButton);
        taskbarButtonList.AddRange(chatHeadsGroup.chatHeads);
        taskbarButtonList.Add(settingsButton);
        taskbarButtonList.Add(backpackButton);
        taskbarButtonList.Add(exploreButton);
        return taskbarButtonList;
    }

    internal static TaskbarHUDView Create(TaskbarHUDController controller, IChatController chatController,
        IFriendsController friendsController)
    {
        var view = Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<TaskbarHUDView>();
        view.Initialize(controller, chatController, friendsController);
        return view;
    }

    public void Initialize(TaskbarHUDController controller, IChatController chatController,
        IFriendsController friendsController)
    {
        this.controller = controller;

        chatHeadsGroup.Initialize(chatController, friendsController);
        chatButton.Initialize();
        friendsButton.Initialize();
        settingsButton.Initialize();
        backpackButton.Initialize();
        exploreButton.Initialize();

        chatHeadsGroup.OnHeadToggleOn += OnWindowToggleOn;
        chatHeadsGroup.OnHeadToggleOff += OnWindowToggleOff;

        chatButton.OnToggleOn += OnWindowToggleOn;
        chatButton.OnToggleOff += OnWindowToggleOff;

        friendsButton.OnToggleOn += OnWindowToggleOn;
        friendsButton.OnToggleOff += OnWindowToggleOff;

        settingsButton.OnToggleOn += OnWindowToggleOn;
        settingsButton.OnToggleOff += OnWindowToggleOff;

        backpackButton.OnToggleOn += OnWindowToggleOn;
        backpackButton.OnToggleOff += OnWindowToggleOff;

        exploreButton.OnToggleOn += OnWindowToggleOn;
        exploreButton.OnToggleOff += OnWindowToggleOff;
    }

    private void OnWindowToggleOff(TaskbarButton obj)
    {
        if (obj == friendsButton)
            OnFriendsToggleOff?.Invoke();
        else if (obj == chatButton)
            OnChatToggleOff?.Invoke();
        else if (obj == settingsButton)
            OnSettingsToggleOff?.Invoke();
        else if (obj == backpackButton)
            OnBackpackToggleOff?.Invoke();
        else if (obj == exploreButton)
            OnExploreToggleOff?.Invoke();

        if (AllButtonsToggledOff())
        {
            chatButton.SetToggleState(false, useCallback: false);
            controller.worldChatWindowHud.SetVisibility(true);
        }
    }

    public bool AllButtonsToggledOff()
    {
        var btns = GetButtonList();

        bool allToggledOff = true;

        foreach (var btn in btns)
        {
            if (btn.toggledOn)
                allToggledOff = false;
        }

        return allToggledOff;
    }

    private void OnWindowToggleOn(TaskbarButton obj)
    {
        if (obj == friendsButton)
            OnFriendsToggleOn?.Invoke();
        else if (obj == chatButton)
            OnChatToggleOn?.Invoke();
        else if (obj == settingsButton)
            OnSettingsToggleOn?.Invoke();
        else if (obj == backpackButton)
            OnBackpackToggleOn?.Invoke();
        else if (obj == exploreButton)
            OnExploreToggleOn?.Invoke();

        SelectButton(obj);
    }

    void SelectButton(TaskbarButton obj)
    {
        var taskbarButtonList = GetButtonList();

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

    internal void OnAddSettingsWindow()
    {
        settingsButton.gameObject.SetActive(true);
    }

    internal void OnAddBackpackWindow()
    {
        backpackButton.gameObject.SetActive(true);
    }

    internal void OnAddExploreWindow()
    {
        exploreButton.gameObject.SetActive(true);
    }

    public void SetVisibility(bool visible)
    {
        gameObject.SetActive(visible);
    }

    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.Return))
    //    {
    //        controller.OnPressReturn();
    //    }

    //    if (Input.GetKeyDown(KeyCode.Escape))
    //    {
    //        controller.OnPressEsc();
    //    }
    //}

    private void OnDestroy()
    {
        if (chatHeadsGroup != null)
        {
            chatHeadsGroup.OnHeadToggleOn -= OnWindowToggleOn;
            chatHeadsGroup.OnHeadToggleOff -= OnWindowToggleOff;
        }

        if (chatButton != null)
        {
            chatButton.OnToggleOn -= OnWindowToggleOn;
            chatButton.OnToggleOff -= OnWindowToggleOff;
        }

        if (friendsButton != null)
        {
            friendsButton.OnToggleOn -= OnWindowToggleOn;
            friendsButton.OnToggleOff -= OnWindowToggleOff;
        }

        if (settingsButton != null)
        {
            settingsButton.OnToggleOn -= OnWindowToggleOn;
            settingsButton.OnToggleOff -= OnWindowToggleOff;
        }

        if (backpackButton != null)
        {
            backpackButton.OnToggleOn -= OnWindowToggleOn;
            backpackButton.OnToggleOff -= OnWindowToggleOff;
        }

        if (exploreButton != null)
        {
            exploreButton.OnToggleOn -= OnWindowToggleOn;
            exploreButton.OnToggleOff -= OnWindowToggleOff;
        }
    }
}