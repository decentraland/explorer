using DCL;
using UnityEngine;

public class TaskbarHUDController : IHUD
{
    public const bool WINDOW_STACKING_ENABLED = false;

    internal TaskbarHUDView view;
    public WorldChatWindowHUDController worldChatWindowHud;
    public PrivateChatWindowHUDController privateChatWindowHud;
    public FriendsHUDController friendsHud;
    public bool alreadyToggledOnForFirstTime { get; private set; } = false;

    IMouseCatcher mouseCatcher;
    IChatController chatController;

    public void Initialize(IMouseCatcher mouseCatcher, IChatController chatController)
    {
        this.mouseCatcher = mouseCatcher;
        this.chatController = chatController;

        view = TaskbarHUDView.Create(this, chatController);

        mouseCatcher.OnMouseLock -= MouseCatcher_OnMouseLock;
        mouseCatcher.OnMouseUnlock -= MouseCatcher_OnMouseUnlock;
        mouseCatcher.OnMouseLock += MouseCatcher_OnMouseLock;
        mouseCatcher.OnMouseUnlock += MouseCatcher_OnMouseUnlock;

        if (!WINDOW_STACKING_ENABLED)
        {
            view.windowContainerLayout.enabled = false;
        }
    }


    private void MouseCatcher_OnMouseUnlock()
    {
        view.windowContainerCanvasGroup.alpha = 1;
    }

    private void MouseCatcher_OnMouseLock()
    {
        view.windowContainerCanvasGroup.alpha = 0;
    }

    public void AddPrivateMessageButton(string userId)
    {
        view.OnAddPrivateMessageButton();
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

        view.OnAddChatWindow(ToggleChatWindow);
        worldChatWindowHud.view.DeactivatePreview();
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

        //Note(Pravus): We don't notify the view about this new window here because it is not toggled from a taskbar icon until we get a private conversation.
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
        view.OnAddFriendsWindow(ToggleFriendsWindow);
    }

    private void ToggleChatWindow()
    {
        if (worldChatWindowHud.view.isInPreview)
            worldChatWindowHud.view.DeactivatePreview();
        else
            worldChatWindowHud.view.ActivatePreview();
    }

    private void TogglePrivateChatWindow()
    {
        privateChatWindowHud.view.Toggle();
    }

    private void ToggleFriendsWindow()
    {
        friendsHud.view.Toggle();
    }

    public void Dispose()
    {
        if (view != null)
        {
            UnityEngine.Object.Destroy(view.gameObject);
        }

        if (mouseCatcher != null)
        {
            mouseCatcher.OnMouseLock -= MouseCatcher_OnMouseLock;
            mouseCatcher.OnMouseUnlock -= MouseCatcher_OnMouseUnlock;
        }
    }

    public void SetVisibility(bool visible)
    {
        view.SetVisibility(visible);
    }

    public void OnPressReturn()
    {
        worldChatWindowHud.OnPressReturn();
    }
}
