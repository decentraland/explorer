using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ChatHeadGroupView : MonoBehaviour
{
    const int MAX_GROUP_SIZE = 5;
    const string CHAT_HEAD_PATH = "ChatHead";

    public event System.Action<TaskbarButton> OnHeadOpen;
    public event System.Action<TaskbarButton> OnHeadClose;

    public Transform container;
    [System.NonSerialized] public List<ChatHeadButton> chatHeads = new List<ChatHeadButton>();
    private IChatController chatController;

    public void Initialize(IChatController chatController)
    {
        this.chatController = chatController;

        if (chatController != null)
            chatController.OnAddMessage += ChatController_OnAddMessage;
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
            AddChatHead(userId, obj.timestamp);
        }
    }

    private void OnOpen(TaskbarButton head)
    {
        OnHeadOpen?.Invoke(head);
    }

    private void OnClose(ChatHeadButton head)
    {
        RemoveHead(head);
        OnHeadClose?.Invoke(head);
    }

    private void AddChatHead(string userId, ulong timestamp)
    {
        GameObject prefab = Resources.Load(CHAT_HEAD_PATH) as GameObject;
        GameObject instance = Instantiate(prefab, container);
        ChatHeadButton chatHead = instance.GetComponent<ChatHeadButton>();

        chatHead.Initialize(UserProfileController.userProfilesCatalog.Get(userId));
        chatHead.lastTimestamp = timestamp;
        chatHead.OnOpen += OnOpen;
        chatHead.OnClose += OnClose;

        chatHeads.Add(chatHead);
        chatHeads.OrderBy((x) => x.lastTimestamp);

        if (chatHeads.Count > MAX_GROUP_SIZE)
        {
            var lastChatHead = chatHeads[chatHeads.Count - 1];
            RemoveHead(lastChatHead);
        }
    }

    void RemoveHead(ChatHeadButton lastChatHead)
    {
        Destroy(lastChatHead.gameObject);
        chatHeads.Remove(lastChatHead);
    }

}
