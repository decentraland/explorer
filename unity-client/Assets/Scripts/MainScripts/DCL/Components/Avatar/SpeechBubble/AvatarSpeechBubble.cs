using DCL;
using DCL.Interface;
using TMPro;
using UnityEngine;

public class AvatarSpeechBubble : MonoBehaviour
{
    const float VANISHING_ALPHA_THRESHOLD = 0.7f;
    const float SECONDS_PER_CHARACTER = 0.04f; //25 char/secs

    public AvatarShape avatarShape;
    public CanvasGroup uiContainer;
    public TextMeshProUGUI chatText;
    public Color privateColor;
    public Color defaultColor;

    public int maxCharacters;
    public float minVisibleTime;

    RectTransform avatarNameRT;
    RectTransform thisRT;

    float yOffsetFromName;
    float lastMessageTime;
    float visibleTime;

    AvatarName avatarName;

    private void Awake()
    {
        avatarName = avatarShape.avatarName;
        avatarNameRT = (RectTransform)avatarName.transform;
        thisRT = (RectTransform)transform;
        yOffsetFromName = thisRT.anchoredPosition.y - avatarNameRT.anchoredPosition.y;
        HideBubble();

        if (ChatController.i) ChatController.i.OnAddMessage += OnChatMessage;
    }

    private void OnDestroy()
    {
        if (ChatController.i) ChatController.i.OnAddMessage -= OnChatMessage;
    }

    void LateUpdate()
    {
        if (string.IsNullOrEmpty(chatText.text))
            return;

        if (Time.unscaledTime - lastMessageTime >= visibleTime)
        {
            HideBubble();
            return;
        }

        RefreshTextPosition();
    }

    private void RefreshTextPosition()
    {
        Vector2 newPositon = avatarNameRT.anchoredPosition;
        newPositon.y += yOffsetFromName;

        thisRT.anchoredPosition = newPositon;

        float nameAlpha = avatarName.uiContainer.alpha;
        uiContainer.alpha = nameAlpha >= VANISHING_ALPHA_THRESHOLD ? nameAlpha : 0;

        if (uiContainer.gameObject.activeSelf != avatarName.uiContainer.gameObject.activeSelf)
        {
            uiContainer.gameObject.SetActive(avatarName.uiContainer.gameObject.activeSelf);
        }
    }

    private void OnChatMessage(ChatMessage message)
    {
        if (message.sender != avatarShape.model?.id)
        {
            return;
        }
        if (IsOldPrivateMessage(message))
        {
            return;
        }

        string messageText = message.body;

        var senderProfile = UserProfileController.userProfilesCatalog.Get(message.sender);
        if (!string.IsNullOrEmpty(senderProfile?.userName))
        {
            messageText = string.Format("{0}: {1}", senderProfile.userName, messageText);
        }

        ShowBubble(messageText, message.messageType == ChatMessage.Type.PRIVATE ? privateColor : defaultColor);
    }

    private void HideBubble()
    {
        chatText.text = string.Empty;
        uiContainer.alpha = 0;
        uiContainer.gameObject.SetActive(false);
    }

    private void ShowBubble(string text, Color color)
    {
        chatText.text = FormatText(text);
        chatText.color = color;
        lastMessageTime = Time.unscaledTime;
        visibleTime = minVisibleTime + chatText.text.Length * SECONDS_PER_CHARACTER;
        uiContainer.gameObject.SetActive(true);
    }

    private string FormatText(string text)
    {
        if (text.Length <= maxCharacters)
        {
            return text;
        }
        else
        {
            return string.Format("{0}...", text.Substring(0, maxCharacters));
        }
    }

    private bool IsOldPrivateMessage(ChatMessage message)
    {
        if (!ChatController.i)
        {
            return true;
        }

        if (message.messageType != ChatMessage.Type.PRIVATE)
            return false;

        double timestampAsSeconds = message.timestamp * 0.001f;

        if (timestampAsSeconds < ChatController.i.initTime)
            return true;

        return false;
    }
}
