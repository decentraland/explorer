using DCL;
using DCL.HelpAndSupportHUD;
using DCL.Helpers;
using DCL.Interface;
using DCL.SettingsHUD;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class TaskbarHUDController : IHUD
{
    public TaskbarHUDView view;
    public WorldChatWindowHUDController worldChatWindowHud;
    public PrivateChatWindowHUDController privateChatWindowHud;
    public FriendsHUDController friendsHud;
    public SettingsHUDController settingsHud;
    public AvatarEditorHUDController avatarEditorHud;
    public ExploreHUDController exploreHud;
    public HelpAndSupportHUDController helpAndSupportHud;

    IMouseCatcher mouseCatcher;
    IChatController chatController;

    private InputAction_Trigger toggleFriendsTrigger;
    private InputAction_Trigger closeWindowTrigger;
    private InputAction_Trigger toggleWorldChatTrigger;

    public event System.Action OnAnyTaskbarButtonClicked;

    public bool isNewTaskbar { get; private set; }
    public RectTransform tutorialTooltipReference { get => view.moreTooltipReference; }
    public RectTransform exploreTooltipReference { get => view.exploreTooltipReference; }

    public void Initialize(IMouseCatcher mouseCatcher, IChatController chatController,
        IFriendsController friendsController, bool newTaskbarIsEnabled)
    {
        this.mouseCatcher = mouseCatcher;
        this.chatController = chatController;

        isNewTaskbar = newTaskbarIsEnabled;
        view = TaskbarHUDView.Create(this, chatController, friendsController, newTaskbarIsEnabled);

        if (mouseCatcher != null)
        {
            mouseCatcher.OnMouseLock -= MouseCatcher_OnMouseLock;
            mouseCatcher.OnMouseUnlock -= MouseCatcher_OnMouseUnlock;
            mouseCatcher.OnMouseLock += MouseCatcher_OnMouseLock;
            mouseCatcher.OnMouseUnlock += MouseCatcher_OnMouseUnlock;
        }

        view.chatHeadsGroup.OnHeadToggleOn += ChatHeadsGroup_OnHeadOpen;
        view.chatHeadsGroup.OnHeadToggleOff += ChatHeadsGroup_OnHeadClose;

        view.leftWindowContainerLayout.enabled = false;

        view.OnChatToggleOff += View_OnChatToggleOff;
        view.OnChatToggleOn += View_OnChatToggleOn;
        view.OnFriendsToggleOff += View_OnFriendsToggleOff;
        view.OnFriendsToggleOn += View_OnFriendsToggleOn;
        view.OnSettingsToggleOff += View_OnSettingsToggleOff;
        view.OnSettingsToggleOn += View_OnSettingsToggleOn;
        view.OnBackpackToggleOff += View_OnBackpackToggleOff;
        view.OnBackpackToggleOn += View_OnBackpackToggleOn;
        view.OnExploreToggleOff += View_OnExploreToggleOff;
        view.OnExploreToggleOn += View_OnExploreToggleOn;
        view.OnHelpAndSupportToggleOff += View_OnHelpAndSupportToggleOff;
        view.OnHelpAndSupportToggleOn += View_OnHelpAndSupportToggleOn;

        toggleFriendsTrigger = Resources.Load<InputAction_Trigger>("ToggleFriends");
        toggleFriendsTrigger.OnTriggered -= ToggleFriendsTrigger_OnTriggered;
        toggleFriendsTrigger.OnTriggered += ToggleFriendsTrigger_OnTriggered;

        closeWindowTrigger = Resources.Load<InputAction_Trigger>("CloseWindow");
        closeWindowTrigger.OnTriggered -= CloseWindowTrigger_OnTriggered;
        closeWindowTrigger.OnTriggered += CloseWindowTrigger_OnTriggered;

        toggleWorldChatTrigger = Resources.Load<InputAction_Trigger>("ToggleWorldChat");
        toggleWorldChatTrigger.OnTriggered -= ToggleWorldChatTrigger_OnTriggered;
        toggleWorldChatTrigger.OnTriggered += ToggleWorldChatTrigger_OnTriggered;

        if (chatController != null)
        {
            chatController.OnAddMessage -= OnAddMessage;
            chatController.OnAddMessage += OnAddMessage;
        }

        view.leftWindowContainerAnimator.Show();
    }

    private void ChatHeadsGroup_OnHeadClose(TaskbarButton obj)
    {
        privateChatWindowHud.SetVisibility(false);
    }

    private void View_OnFriendsToggleOn()
    {
        friendsHud.SetVisibility(true);
        OnAnyTaskbarButtonClicked?.Invoke();
    }

    private void View_OnFriendsToggleOff()
    {
        friendsHud.SetVisibility(false);
    }

    private void ToggleFriendsTrigger_OnTriggered(DCLAction_Trigger action)
    {
        if (!view.friendsButton.gameObject.activeSelf) return;

        OnFriendsToggleInputPress();
    }

    private void ToggleWorldChatTrigger_OnTriggered(DCLAction_Trigger action)
    {
        OnWorldChatToggleInputPress();
    }

    private void CloseWindowTrigger_OnTriggered(DCLAction_Trigger action)
    {
        OnCloseWindowToggleInputPress();
    }

    private void View_OnChatToggleOn()
    {
        worldChatWindowHud.SetVisibility(true);
        worldChatWindowHud.MarkWorldChatMessagesAsRead();
        worldChatWindowHud.view.DeactivatePreview();
        worldChatWindowHud.OnPressReturn();
        OnAnyTaskbarButtonClicked?.Invoke();
    }

    private void View_OnChatToggleOff()
    {
        if (view.AllButtonsToggledOff())
        {
            worldChatWindowHud.SetVisibility(true);
            worldChatWindowHud.view.ActivatePreview();
        }
        else
        {
            worldChatWindowHud.SetVisibility(false);
        }
    }

    private void ChatHeadsGroup_OnHeadOpen(TaskbarButton taskbarBtn)
    {
        ChatHeadButton head = taskbarBtn as ChatHeadButton;

        if (taskbarBtn == null)
            return;

        OpenPrivateChatWindow(head.profile.userId);
    }

    private void View_OnSettingsToggleOn()
    {
        settingsHud.SetVisibility(true);
        OnAnyTaskbarButtonClicked?.Invoke();
    }

    private void View_OnSettingsToggleOff()
    {
        settingsHud.SetVisibility(false);
    }

    private void View_OnBackpackToggleOn()
    {
        avatarEditorHud.SetVisibility(true);
        OnAnyTaskbarButtonClicked?.Invoke();
    }

    private void View_OnBackpackToggleOff()
    {
        avatarEditorHud.SetVisibility(false);
    }

    private void View_OnExploreToggleOn()
    {
        exploreHud.SetVisibility(true);
        OnAnyTaskbarButtonClicked?.Invoke();
    }

    private void View_OnExploreToggleOff()
    {
        exploreHud.SetVisibility(false);
    }

    private void View_OnHelpAndSupportToggleOn()
    {
        helpAndSupportHud.SetVisibility(true);
        OnAnyTaskbarButtonClicked?.Invoke();
    }

    private void View_OnHelpAndSupportToggleOff()
    {
        helpAndSupportHud.SetVisibility(false);
    }

    private void MouseCatcher_OnMouseUnlock()
    {
        view.leftWindowContainerAnimator.Show();
    }

    private void MouseCatcher_OnMouseLock()
    {
        view.leftWindowContainerAnimator.Hide();
        view.moreMenu.ShowMoreMenu(false);

        foreach (var btn in view.GetButtonList())
        {
            btn.SetToggleState(false);
        }

        worldChatWindowHud.SetVisibility(true);
        worldChatWindowHud.view.ActivatePreview();

        if (!AnyWindowsDifferentThanChatIsOpen())
            worldChatWindowHud.MarkWorldChatMessagesAsRead();
    }

    public void AddWorldChatWindow(WorldChatWindowHUDController controller)
    {
        if (controller == null || controller.view == null)
        {
            Debug.LogWarning("AddChatWindow >>> World Chat Window doesn't exist yet!");
            return;
        }

        if (controller.view.transform.parent == view.leftWindowContainer)
            return;

        controller.view.transform.SetParent(view.leftWindowContainer, false);

        worldChatWindowHud = controller;

        view.OnAddChatWindow();
        worldChatWindowHud.view.OnClose += () => { view.friendsButton.SetToggleState(false, false); };

        view.chatButton.SetToggleState(true);
        view.chatButton.SetToggleState(false);
    }

    public void OpenFriendsWindow()
    {
        view.friendsButton.SetToggleState(true);
    }

    public void OpenPrivateChatTo(string userId)
    {
        var button = view.chatHeadsGroup.AddChatHead(userId, (ulong) System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        button.toggleButton.onClick.Invoke();
    }

    public void AddPrivateChatWindow(PrivateChatWindowHUDController controller)
    {
        if (controller == null || controller.view == null)
        {
            Debug.LogWarning("AddPrivateChatWindow >>> Private Chat Window doesn't exist yet!");
            return;
        }

        if (controller.view.transform.parent == view.leftWindowContainer)
            return;

        controller.view.transform.SetParent(view.leftWindowContainer, false);

        privateChatWindowHud = controller;

        privateChatWindowHud.view.OnMinimize += () =>
        {
            ChatHeadButton btn = view.GetButtonList().FirstOrDefault(
                    (x) => x is ChatHeadButton &&
                           (x as ChatHeadButton).profile.userId == privateChatWindowHud.conversationUserId) as
                ChatHeadButton;

            if (btn != null)
                btn.SetToggleState(false, false);

            if (!AnyWindowsDifferentThanChatIsOpen())
                worldChatWindowHud.MarkWorldChatMessagesAsRead();
        };

        privateChatWindowHud.view.OnClose += () =>
        {
            ChatHeadButton btn = view.GetButtonList().FirstOrDefault(
                    (x) => x is ChatHeadButton &&
                           (x as ChatHeadButton).profile.userId == privateChatWindowHud.conversationUserId) as
                ChatHeadButton;

            if (btn != null)
            {
                btn.SetToggleState(false, false);
                view.chatHeadsGroup.RemoveChatHead(btn);
            }

            if (!AnyWindowsDifferentThanChatIsOpen())
                worldChatWindowHud.MarkWorldChatMessagesAsRead();
        };
    }

    public void AddFriendsWindow(FriendsHUDController controller)
    {
        if (controller == null || controller.view == null)
        {
            Debug.LogWarning("AddFriendsWindow >>> Friends window doesn't exist yet!");
            return;
        }

        if (controller.view.transform.parent == view.leftWindowContainer)
            return;

        controller.view.transform.SetParent(view.leftWindowContainer, false);

        friendsHud = controller;
        view.OnAddFriendsWindow();
        friendsHud.view.OnClose += () =>
        {
            view.friendsButton.SetToggleState(false, false);

            if (!AnyWindowsDifferentThanChatIsOpen())
                worldChatWindowHud.MarkWorldChatMessagesAsRead();
        };

        friendsHud.view.friendsList.OnDeleteConfirmation += (userIdToRemove) => { view.chatHeadsGroup.RemoveChatHead(userIdToRemove); };
    }

    public void AddSettingsWindow(SettingsHUDController controller)
    {
        if (controller == null)
        {
            Debug.LogWarning("AddSettingsWindow >>> Settings window doesn't exist yet!");
            return;
        }

        settingsHud = controller;
        view.OnAddSettingsWindow();
        settingsHud.OnClose += () =>
        {
            view.settingsButton.SetToggleState(false, false);

            if (!AnyWindowsDifferentThanChatIsOpen())
                worldChatWindowHud.MarkWorldChatMessagesAsRead();
        };
    }

    public void AddBackpackWindow(AvatarEditorHUDController controller)
    {
        if (controller == null)
        {
            Debug.LogWarning("AddBackpackWindow >>> Backpack window doesn't exist yet!");
            return;
        }

        avatarEditorHud = controller;
        view.OnAddBackpackWindow();
        avatarEditorHud.OnClose += () =>
        {
            view.backpackButton.SetToggleState(false, false);

            if (!AnyWindowsDifferentThanChatIsOpen())
                worldChatWindowHud.MarkWorldChatMessagesAsRead();
        };
    }

    public void AddExploreWindow(ExploreHUDController controller)
    {
        if (controller == null)
        {
            Debug.LogWarning("AddExploreWindow >>> Explore window doesn't exist yet!");
            return;
        }

        exploreHud = controller;
        view.OnAddExploreWindow();
        exploreHud.OnClose += () =>
        {
            view.exploreButton.SetToggleState(false, false);

            if (!AnyWindowsDifferentThanChatIsOpen())
                worldChatWindowHud.MarkWorldChatMessagesAsRead();
        };
    }

    public void AddHelpAndSupportWindow(HelpAndSupportHUDController controller)
    {
        if (controller == null || controller.view == null)
        {
            Debug.LogWarning("AddHelpAndSupportWindow >>> Help and Support window doesn't exist yet!");
            return;
        }

        helpAndSupportHud = controller;
        view.OnAddHelpAndSupportWindow();
        helpAndSupportHud.view.OnClose += () =>
        {
            view.helpAndSupportButton.SetToggleState(false, false);

            if (!AnyWindowsDifferentThanChatIsOpen())
                worldChatWindowHud.MarkWorldChatMessagesAsRead();
        };
    }

    public void AddControlsMoreOption()
    {
        view.OnAddControlsMoreOption();
    }

    public void DisableFriendsWindow()
    {
        view.friendsButton.gameObject.SetActive(false);
        view.chatHeadsGroup.ClearChatHeads();
    }

    private void OpenPrivateChatWindow(string userId)
    {
        privateChatWindowHud.Configure(userId);
        privateChatWindowHud.SetVisibility(true);
        privateChatWindowHud.ForceFocus();
        OnAnyTaskbarButtonClicked?.Invoke();
    }

    public void Dispose()
    {
        if (view != null)
        {
            view.chatHeadsGroup.OnHeadToggleOn -= ChatHeadsGroup_OnHeadOpen;
            view.chatHeadsGroup.OnHeadToggleOff -= ChatHeadsGroup_OnHeadClose;

            view.OnChatToggleOff -= View_OnChatToggleOff;
            view.OnChatToggleOn -= View_OnChatToggleOn;
            view.OnFriendsToggleOff -= View_OnFriendsToggleOff;
            view.OnFriendsToggleOn -= View_OnFriendsToggleOn;
            view.OnSettingsToggleOff -= View_OnSettingsToggleOff;
            view.OnSettingsToggleOn -= View_OnSettingsToggleOn;
            view.OnBackpackToggleOff -= View_OnBackpackToggleOff;
            view.OnBackpackToggleOn -= View_OnBackpackToggleOn;
            view.OnExploreToggleOff -= View_OnExploreToggleOff;
            view.OnExploreToggleOn -= View_OnExploreToggleOn;
            view.OnHelpAndSupportToggleOff -= View_OnHelpAndSupportToggleOff;
            view.OnHelpAndSupportToggleOn -= View_OnHelpAndSupportToggleOn;

            UnityEngine.Object.Destroy(view.gameObject);
        }

        if (mouseCatcher != null)
        {
            mouseCatcher.OnMouseLock -= MouseCatcher_OnMouseLock;
            mouseCatcher.OnMouseUnlock -= MouseCatcher_OnMouseUnlock;
        }

        if (toggleFriendsTrigger != null)
            toggleFriendsTrigger.OnTriggered -= ToggleFriendsTrigger_OnTriggered;

        if (closeWindowTrigger != null)
            closeWindowTrigger.OnTriggered -= CloseWindowTrigger_OnTriggered;

        if (toggleWorldChatTrigger != null)
            toggleWorldChatTrigger.OnTriggered -= ToggleWorldChatTrigger_OnTriggered;

        if (chatController != null)
            chatController.OnAddMessage -= OnAddMessage;
    }

    public void SetVisibility(bool visible)
    {
        view.SetVisibility(visible);
    }

    public void OnWorldChatToggleInputPress()
    {
        bool anyInputFieldIsSelected = EventSystem.current != null &&
                                       EventSystem.current.currentSelectedGameObject != null &&
                                       EventSystem.current.currentSelectedGameObject.GetComponent<TMPro.TMP_InputField>() != null;

        if (anyInputFieldIsSelected)
            return;

        worldChatWindowHud.OnPressReturn();

        if (AnyWindowsDifferentThanChatIsOpen())
        {
            foreach (var btn in view.GetButtonList())
            {
                btn.SetToggleState(btn == view.chatButton);
            }
        }
    }

    public void OnCloseWindowToggleInputPress()
    {
        if (mouseCatcher.isLocked)
            return;

        view.chatButton.SetToggleState(true);
        view.chatButton.SetToggleState(false, false);
        worldChatWindowHud.view.chatHudView.ResetInputField();
        worldChatWindowHud.view.ActivatePreview();
    }

    private void OnFriendsToggleInputPress()
    {
        bool anyInputFieldIsSelected = EventSystem.current != null &&
                                       EventSystem.current.currentSelectedGameObject != null &&
                                       EventSystem.current.currentSelectedGameObject.GetComponent<TMPro.TMP_InputField>() != null &&
                                       (!worldChatWindowHud.view.chatHudView.inputField.isFocused || !worldChatWindowHud.view.isInPreview);

        if (anyInputFieldIsSelected)
            return;

        Utils.UnlockCursor();
        view.leftWindowContainerAnimator.Show();
        view.friendsButton.SetToggleState(!view.friendsButton.toggledOn);
    }

    void OnAddMessage(ChatMessage message)
    {
        if (!AnyWindowsDifferentThanChatIsOpen() && message.messageType == ChatMessage.Type.PUBLIC)
            worldChatWindowHud.MarkWorldChatMessagesAsRead((long) message.timestamp);
    }

    private bool AnyWindowsDifferentThanChatIsOpen()
    {
        return (friendsHud != null && friendsHud.view.gameObject.activeSelf) ||
               (privateChatWindowHud != null && privateChatWindowHud.view.gameObject.activeSelf);
    }
}