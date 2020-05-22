using DCL;
using DCL.Helpers;
using DCL.Interface;
using System.Linq;
using UnityEngine;

public class TaskbarHUDController : IHUD
{
    public const bool WINDOW_STACKING_ENABLED = false;

    public TaskbarHUDView view;
    public WorldChatWindowHUDController worldChatWindowHud;
    public PrivateChatWindowHUDController privateChatWindowHud;
    public FriendsHUDController friendsHud;
    public bool alreadyToggledOnForFirstTime { get; private set; } = false;

    IMouseCatcher mouseCatcher;
    IChatController chatController;
    InputAction_Trigger toggleTrigger;

    public void Initialize(IMouseCatcher mouseCatcher, IChatController chatController)
    {
        this.mouseCatcher = mouseCatcher;
        this.chatController = chatController;

        view = TaskbarHUDView.Create(this, chatController);

        if (mouseCatcher != null)
        {
            mouseCatcher.OnMouseLock -= MouseCatcher_OnMouseLock;
            mouseCatcher.OnMouseUnlock -= MouseCatcher_OnMouseUnlock;
            mouseCatcher.OnMouseLock += MouseCatcher_OnMouseLock;
            mouseCatcher.OnMouseUnlock += MouseCatcher_OnMouseUnlock;
        }

        view.chatHeadsGroup.OnHeadToggleOn += ChatHeadsGroup_OnHeadOpen;
        view.chatHeadsGroup.OnHeadToggleOff += ChatHeadsGroup_OnHeadClose;

        view.windowContainerLayout.enabled = false;

        view.OnChatToggleOff += View_OnChatToggleOff;
        view.OnChatToggleOn += View_OnChatToggleOn;
        view.OnFriendsToggleOff += View_OnFriendsToggleOff;
        view.OnFriendsToggleOn += View_OnFriendsToggleOn;

        toggleTrigger = Resources.Load<InputAction_Trigger>("ToggleFriends");
        toggleTrigger.OnTriggered -= ToggleTrigger_OnTriggered;
        toggleTrigger.OnTriggered += ToggleTrigger_OnTriggered;

        if (chatController != null)
        {
            chatController.OnAddMessage -= OnAddMessage;
            chatController.OnAddMessage += OnAddMessage;
        }
    }

    private void ChatHeadsGroup_OnHeadClose(TaskbarButton obj)
    {
        privateChatWindowHud.SetVisibility(false);
    }

    private void View_OnFriendsToggleOn()
    {
        friendsHud.SetVisibility(true);
    }

    private void View_OnFriendsToggleOff()
    {
        friendsHud.SetVisibility(false);
    }

    private void ToggleTrigger_OnTriggered(DCLAction_Trigger action)
    {
        Utils.UnlockCursor();
        view.windowContainerAnimator.Show();
        view.friendsButton.SetToggleState(!view.friendsButton.toggledOn);
    }

    private void View_OnChatToggleOn()
    {
        worldChatWindowHud.SetVisibility(true);
        worldChatWindowHud.MarkWorldChatMessagesAsRead();
        worldChatWindowHud.view.DeactivatePreview();
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


    private void MouseCatcher_OnMouseUnlock()
    {
        view.windowContainerAnimator.Show();
    }

    private void MouseCatcher_OnMouseLock()
    {
        view.windowContainerAnimator.Hide();

        foreach (var btn in view.GetButtonList())
        {
            btn.SetToggleState(false);
        }

        worldChatWindowHud.SetVisibility(true);
        worldChatWindowHud.view.ActivatePreview();

        if (!AnyWindowsOpen())
            worldChatWindowHud.MarkWorldChatMessagesAsRead();
    }

    public void AddWorldChatWindow(WorldChatWindowHUDController controller)
    {
        if (controller == null || controller.view == null)
        {
            Debug.LogWarning("AddChatWindow >>> World Chat Window doesn't exist yet!");
            return;
        }

        if (controller.view.transform.parent == view.windowContainer)
            return;

        controller.view.transform.SetParent(view.windowContainer, false);

        worldChatWindowHud = controller;

        view.OnAddChatWindow();
        worldChatWindowHud.view.ActivatePreview();
        worldChatWindowHud.view.OnClose += () => { view.friendsButton.SetToggleState(false, false); };
    }

    public void OpenFriendsWindow()
    {
        view.friendsButton.SetToggleState(true);
    }

    public void OpenPrivateChatTo(string userId)
    {
        var button = view.chatHeadsGroup.AddChatHead(userId, ulong.MaxValue);
        button.toggleButton.onClick.Invoke();
    }

    public void AddPrivateChatWindow(PrivateChatWindowHUDController controller)
    {
        if (controller == null || controller.view == null)
        {
            Debug.LogWarning("AddPrivateChatWindow >>> Private Chat Window doesn't exist yet!");
            return;
        }

        if (controller.view.transform.parent == view.windowContainer)
            return;

        controller.view.transform.SetParent(view.windowContainer, false);

        privateChatWindowHud = controller;

        privateChatWindowHud.view.OnMinimize += () =>
        {
            ChatHeadButton btn = view.GetButtonList().FirstOrDefault(
                (x) => x is ChatHeadButton &&
                (x as ChatHeadButton).profile.userId == privateChatWindowHud.conversationUserId) as ChatHeadButton;

            if (btn != null)
                btn.SetToggleState(false, false);

            if (!AnyWindowsOpen())
                worldChatWindowHud.MarkWorldChatMessagesAsRead();
        };

        privateChatWindowHud.view.OnClose += () =>
        {
            ChatHeadButton btn = view.GetButtonList().FirstOrDefault(
                (x) => x is ChatHeadButton &&
                (x as ChatHeadButton).profile.userId == privateChatWindowHud.conversationUserId) as ChatHeadButton;

            if (btn != null)
            {
                btn.SetToggleState(false, false);
                view.chatHeadsGroup.RemoveChatHead(btn);
            }

            if (!AnyWindowsOpen())
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

        if (controller.view.transform.parent == view.windowContainer)
            return;

        controller.view.transform.SetParent(view.windowContainer, false);

        friendsHud = controller;
        view.OnAddFriendsWindow();
        friendsHud.view.OnClose += () =>
        {
            view.friendsButton.SetToggleState(false, false);

            if (!AnyWindowsOpen())
                worldChatWindowHud.MarkWorldChatMessagesAsRead();
        };
    }


    private void OpenPrivateChatWindow(string userId)
    {
        privateChatWindowHud.Configure(userId);
        privateChatWindowHud.SetVisibility(true);
        privateChatWindowHud.ForceFocus();
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

            UnityEngine.Object.Destroy(view.gameObject);
        }

        if (mouseCatcher != null)
        {
            mouseCatcher.OnMouseLock -= MouseCatcher_OnMouseLock;
            mouseCatcher.OnMouseUnlock -= MouseCatcher_OnMouseUnlock;
        }

        toggleTrigger.OnTriggered -= ToggleTrigger_OnTriggered;

        if (chatController != null)
            chatController.OnAddMessage -= OnAddMessage;
    }

    public void SetVisibility(bool visible)
    {
        view.SetVisibility(visible);
    }

    public void OnPressReturn()
    {
        worldChatWindowHud.OnPressReturn();
    }

    public void OnPressEsc()
    {
        if (mouseCatcher.isLocked)
            return;

        view.chatButton.SetToggleState(true);
        view.chatButton.SetToggleState(false, false);
        worldChatWindowHud.view.ActivatePreview();
    }

    void OnAddMessage(ChatMessage message)
    {
        if (!AnyWindowsOpen())
            worldChatWindowHud.MarkWorldChatMessagesAsRead();
    }

    private bool AnyWindowsOpen()
    {
        return (friendsHud != null && friendsHud.view.gameObject.activeSelf) ||
               (privateChatWindowHud != null && privateChatWindowHud.view.gameObject.activeSelf);
    }
}
