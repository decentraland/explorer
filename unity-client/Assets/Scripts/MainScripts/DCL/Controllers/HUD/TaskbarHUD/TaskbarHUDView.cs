using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TaskbarHUDView : MonoBehaviour
{
    const string VIEW_PATH = "Taskbar";

    [Header("Taskbar Config")]
    [SerializeField] internal ShowHideAnimator taskbarAnimator;

    [Header("Left Side Config")]
    [SerializeField] internal RectTransform leftWindowContainer;
    [SerializeField] internal ShowHideAnimator leftWindowContainerAnimator;
    [SerializeField] internal LayoutGroup leftWindowContainerLayout;
    [SerializeField] internal TaskbarButton chatButton;
    [SerializeField] internal TaskbarButton friendsButton;
    [SerializeField] internal ChatHeadGroupView chatHeadsGroup;

    [Header("Right Side Config")]
    [SerializeField] internal TaskbarButton settingsButton;
    [SerializeField] internal TaskbarButton backpackButton;
    [SerializeField] internal TaskbarButton exploreButton;
    [SerializeField] internal TaskbarButton helpAndSupportButton;
    [SerializeField] internal GameObject separatorMark;

    [Header("More Button Config")]
    [SerializeField] internal ShowHideAnimator moreWindowContainerAnimator;
    [SerializeField] internal Button moreButton;
    [SerializeField] internal Button collapseBarButton;
    [SerializeField] internal Button hideUIButton;
    [SerializeField] internal Button controlsButton;

    internal TaskbarHUDController controller;

    private bool isBarVisible = true;
    private bool isMoreMenuVisible = false;

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
    public event System.Action OnHelpAndSupportToggleOn;
    public event System.Action OnHelpAndSupportToggleOff;
    public event System.Action OnMoreToggleOn;
    public event System.Action OnMoreToggleOff;

    internal List<TaskbarButton> GetButtonList()
    {
        var taskbarButtonList = new List<TaskbarButton>();
        taskbarButtonList.Add(chatButton);
        taskbarButtonList.Add(friendsButton);
        taskbarButtonList.AddRange(chatHeadsGroup.chatHeads);
        taskbarButtonList.Add(settingsButton);
        taskbarButtonList.Add(backpackButton);
        taskbarButtonList.Add(exploreButton);
        taskbarButtonList.Add(helpAndSupportButton);
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

        ShowBar(true, true);
        ShowMoreMenu(false, true);
        chatButton.transform.parent.gameObject.SetActive(false);
        friendsButton.transform.parent.gameObject.SetActive(false);
        settingsButton.transform.parent.gameObject.SetActive(false);
        backpackButton.transform.parent.gameObject.SetActive(false);
        exploreButton.transform.parent.gameObject.SetActive(false);
        helpAndSupportButton.transform.parent.gameObject.SetActive(false);
        separatorMark.SetActive(false);

        collapseBarButton.gameObject.SetActive(true);

        chatHeadsGroup.Initialize(chatController, friendsController);
        chatButton.Initialize();
        friendsButton.Initialize();
        settingsButton.Initialize();
        backpackButton.Initialize();
        exploreButton.Initialize();
        helpAndSupportButton.Initialize();

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

        helpAndSupportButton.OnToggleOn += OnWindowToggleOn;
        helpAndSupportButton.OnToggleOff += OnWindowToggleOff;

        moreButton.onClick.AddListener(() =>
        {
            ShowMoreMenu(!isMoreMenuVisible);
        });

        collapseBarButton.onClick.AddListener(() =>
        {
            ShowBar(!isBarVisible);
            ShowMoreMenu(false);
        });
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
        else if (obj == helpAndSupportButton)
            OnHelpAndSupportToggleOff?.Invoke();

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
        else if (obj == helpAndSupportButton)
            OnHelpAndSupportToggleOn?.Invoke();

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
        chatButton.transform.parent.gameObject.SetActive(true);
    }

    internal void OnAddFriendsWindow()
    {
        friendsButton.transform.parent.gameObject.SetActive(true);
    }

    internal void OnAddSettingsWindow()
    {
        settingsButton.transform.parent.gameObject.SetActive(true);
        separatorMark.SetActive(true);
    }

    internal void OnAddBackpackWindow()
    {
        backpackButton.transform.parent.gameObject.SetActive(true);
    }

    internal void OnAddExploreWindow()
    {
        exploreButton.transform.parent.gameObject.SetActive(true);
    }

    internal void OnAddHelpAndSupportWindow()
    {
        helpAndSupportButton.transform.parent.gameObject.SetActive(true);
        separatorMark.SetActive(true);
    }

    internal void ShowMoreMenu(bool visible, bool instant = false)
    {
        if (visible)
        {
            moreWindowContainerAnimator.Show(instant);
            isMoreMenuVisible = true;
        }
        else
        {
            moreWindowContainerAnimator.Hide(instant);
            isMoreMenuVisible = false;
        }
    }

    internal void ShowBar(bool visible, bool instant = false)
    {
        if (visible)
        {
            taskbarAnimator.Show(instant);
            isBarVisible = true;
        }
        else
        {
            taskbarAnimator.Hide(instant);
            isBarVisible = false;
        }
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

        if (helpAndSupportButton != null)
        {
            helpAndSupportButton.OnToggleOn -= OnWindowToggleOn;
            helpAndSupportButton.OnToggleOff -= OnWindowToggleOff;
        }

        if (moreButton != null)
            moreButton.onClick.RemoveAllListeners();

        if (collapseBarButton != null)
            collapseBarButton.onClick.RemoveAllListeners();
    }
}