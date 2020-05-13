using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConversationBlobGroupView : MonoBehaviour
{
    const int MAX_BUTTONS = 5;
    const string PRIVATE_MSG_BTN_PATH = "ConversationBlob";

    public Transform container;
    public List<Button> privateMessageButtons;
    private IChatController chatController;

    public void Initialize(IChatController chatController)
    {
        this.chatController = chatController;

        if (chatController != null)
            chatController.OnAddMessage += ChatController_OnAddMessage;
    }

    private void Awake()
    {
    }

    private void OnDestroy()
    {
        if (chatController != null)
            chatController.OnAddMessage -= ChatController_OnAddMessage;

    }

    private void ChatController_OnAddMessage(DCL.Interface.ChatMessage obj)
    {
        if (obj.messageType != DCL.Interface.ChatMessage.Type.PRIVATE)
            return;

        var ownProfile = UserProfile.GetOwnUserProfile();

        string userId = string.Empty;

        if (obj.sender != ownProfile.userId)
            userId = obj.sender;
        else if (obj.recipient != ownProfile.userId)
            userId = obj.recipient;

        if (!string.IsNullOrEmpty(userId))
        {
            AddPrivateMessageButton(userId);
        }
    }

    private void AddPrivateMessageButton(string userId)
    {
        GameObject prefab = Resources.Load(PRIVATE_MSG_BTN_PATH) as GameObject;
        GameObject instance = Instantiate(prefab, container);
        privateMessageButtons.Add(instance.GetComponent<Button>());
    }

}
