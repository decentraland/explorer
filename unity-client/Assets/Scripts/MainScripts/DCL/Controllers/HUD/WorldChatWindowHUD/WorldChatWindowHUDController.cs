using DCL;
using DCL.Interface;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class WorldChatWindowHUDController : IHUD
{
    internal const string PLAYER_PREFS_LAST_READ_WORLD_CHAT_MESSAGES = "LastReadWorldChatMessages";

    private ChatHUDController chatHudController;
    public WorldChatWindowHUDView view;

    private IChatController chatController;
    private IMouseCatcher mouseCatcher;

    internal bool resetInputFieldOnSubmit = true;
    private int invalidSubmitLastFrame = 0;
    UserProfile ownProfile => UserProfile.GetOwnUserProfile();
    public string lastPrivateMessageReceivedSender = string.Empty;

    public event UnityAction<string> OnPressPrivateMessage;

    public void Initialize(IChatController chatController, IMouseCatcher mouseCatcher)
    {
        view = WorldChatWindowHUDView.Create();
        view.controller = this;

        view.chatHudView.inputField.onSelect.RemoveListener(ChatHUDViewInputField_OnSelect);
        view.chatHudView.inputField.onSelect.AddListener(ChatHUDViewInputField_OnSelect);

        chatHudController = new ChatHUDController();
        chatHudController.Initialize(view.chatHudView, SendChatMessage);
        chatHudController.OnPressPrivateMessage -= ChatHUDController_OnPressPrivateMessage;
        chatHudController.OnPressPrivateMessage += ChatHUDController_OnPressPrivateMessage;
        LoadLatestReadWorldChatMessagesStatus();

        this.chatController = chatController;
        this.mouseCatcher = mouseCatcher;

        if (chatController != null)
        {
            chatController.OnAddMessage -= OnAddMessage;
            chatController.OnAddMessage += OnAddMessage;
        }

        if (mouseCatcher != null)
        {
            mouseCatcher.OnMouseLock += view.ActivatePreview;
        }

        if (chatController != null)
        {
            view.worldFilterButton.onClick.Invoke();
        }
    }

    void ChatHUDController_OnPressPrivateMessage(string friendUserId)
    {
        OnPressPrivateMessage?.Invoke(friendUserId);
    }

    public void Dispose()
    {
        view.chatHudView.inputField.onSelect.RemoveListener(ChatHUDViewInputField_OnSelect);

        if (chatController != null)
            chatController.OnAddMessage -= OnAddMessage;

        if (chatHudController != null)
            chatHudController.OnPressPrivateMessage -= ChatHUDController_OnPressPrivateMessage;

        if (mouseCatcher != null)
        {
            mouseCatcher.OnMouseLock -= view.ActivatePreview;
        }

        Object.Destroy(view);
    }

    bool IsOldPrivateMessage(ChatMessage message)
    {
        if (message.messageType != ChatMessage.Type.PRIVATE)
            return false;

        double timestampAsSeconds = message.timestamp / 1000.0f;

        if (timestampAsSeconds < chatController.initTime)
            return true;

        return false;
    }

    void OnAddMessage(ChatMessage message)
    {
        if (IsOldPrivateMessage(message))
            return;

        view.chatHudView.controller.AddChatMessage(ChatHUDController.ChatMessageToChatEntry(message));

        if (message.messageType == ChatMessage.Type.PRIVATE && message.recipient == ownProfile.userId)
            lastPrivateMessageReceivedSender = UserProfileController.userProfilesCatalog.Get(message.sender).userName;

        if (view.chatHudView.inputField.isFocused)
        {
            // The messages are marked as read if the player was already focused on the input field of the world chat
            MarkWorldChatMessagesAsRead();
        }
    }

    //NOTE(Brian): Send chat responsibilities must be on the chatHud containing window like this one, this way we ensure
    //             it can be reused by the private messaging windows down the road.
    public void SendChatMessage(string msgBody)
    {
        bool validString = !string.IsNullOrEmpty(msgBody);

        if (msgBody.Length == 1 && (byte)msgBody[0] == 11) //NOTE(Brian): Trim doesn't work. neither IsNullOrWhitespace.
            validString = false;

        if (!validString)
        {
            view.ActivatePreview();
            InitialSceneReferences.i.mouseCatcher.LockCursor();
            invalidSubmitLastFrame = Time.frameCount;
            return;
        }

        if (resetInputFieldOnSubmit)
        {
            view.chatHudView.ResetInputField();
            view.chatHudView.FocusInputField();
        }

        var data = new ChatMessage()
        {
            body = msgBody,
            sender = UserProfile.GetOwnUserProfile().userId,
        };

        WebInterface.SendChatMessage(data);
    }

    public void SetVisibility(bool visible)
    {
        view.gameObject.SetActive(visible);

        if (visible)
        {
            // The messages are marked as read once the world chat is opened
            MarkWorldChatMessagesAsRead();
        }
    }

    public bool OnPressReturn()
    {
        if (EventSystem.current.currentSelectedGameObject != null &&
            EventSystem.current.currentSelectedGameObject.GetComponent<TMPro.TMP_InputField>() != null)
            return false;

        if ((Time.frameCount - invalidSubmitLastFrame) < 2)
            return false;

        ForceFocus();
        return true;
    }

    public void ForceFocus(string setInputText = null)
    {
        SetVisibility(true);
        view.chatHudView.FocusInputField();
        view.DeactivatePreview();
        InitialSceneReferences.i.mouseCatcher.UnlockCursor();

        if (!string.IsNullOrEmpty(setInputText))
        {
            view.chatHudView.inputField.text = setInputText;
            view.chatHudView.inputField.caretPosition = setInputText.Length;
        }
    }

    private void MarkWorldChatMessagesAsRead()
    {
        CommonScriptableObjects.lastReadWorldChatMessages.Set(System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        SaveLatestReadWorldChatMessagesStatus();
    }

    private void SaveLatestReadWorldChatMessagesStatus()
    {
        PlayerPrefs.SetString(PLAYER_PREFS_LAST_READ_WORLD_CHAT_MESSAGES, CommonScriptableObjects.lastReadWorldChatMessages.Get().ToString());
        PlayerPrefs.Save();
    }

    private void LoadLatestReadWorldChatMessagesStatus()
    {
        CommonScriptableObjects.lastReadWorldChatMessages.Set(0);
        string storedLastReadWorldChatMessagesString = PlayerPrefs.GetString(PLAYER_PREFS_LAST_READ_WORLD_CHAT_MESSAGES);
        CommonScriptableObjects.lastReadWorldChatMessages.Set(System.Convert.ToInt64(string.IsNullOrEmpty(storedLastReadWorldChatMessagesString) ? 0 : System.Convert.ToInt64(storedLastReadWorldChatMessagesString)));
    }

    private void ChatHUDViewInputField_OnSelect(string message)
    {
        // The messages are marked as read if the player clicks on the input field of the world chat
        MarkWorldChatMessagesAsRead();
    }
}
