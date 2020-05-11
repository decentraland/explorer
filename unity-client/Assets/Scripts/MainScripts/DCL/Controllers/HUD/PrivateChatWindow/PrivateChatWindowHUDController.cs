using DCL;
using DCL.Interface;
using System.Collections;
using UnityEngine;

public class PrivateChatWindowHUDController : IHUD
{
    private ChatHUDController chatHudController;
    public PrivateChatWindowHUDView view;

    private IChatController chatController;

    public void SetVisibility(bool visible)
    {
        view.gameObject.SetActive(visible);

        if (visible)
        {
            view.StartCoroutine(ForceLayoutDelayed());
        }
    }

    public void Dispose()
    {
        if (chatController != null)
            chatController.OnAddMessage -= OnAddMessage;

        Object.Destroy(view);
    }

    void OnAddMessage(ChatMessage message)
    {
        view.chatHudView.controller.AddChatMessage(ChatHUDController.ChatMessageToChatEntry(message));
    }

    IEnumerator ForceLayoutDelayed()
    {
        yield return null;
        view.chatHudView.ForceUpdateLayout();
    }
}
