using DCL;
using DCL.Interface;
using TMPro;
using UnityEngine;

public class AvatarSpeechBubble : MonoBehaviour
{
    const float VANISHING_ALPHA_THRESHOLD = 0.7f;

    public AvatarShape avatarShape;
    public CanvasGroup uiContainer;
    public TextMeshProUGUI chatText;
    [SerializeField] int maxCharacters;
    [SerializeField] float visibleTime;

    RectTransform avatarNameRT;
    RectTransform thisRT;

    float yOffsetFromName;
    float lastMessageTime;

    AvatarName avatarName;

    private void Awake()
    {
        avatarName = avatarShape.avatarName;
        avatarNameRT = (RectTransform)avatarName.transform;
        thisRT = (RectTransform)transform;
        yOffsetFromName = thisRT.anchoredPosition.y - avatarNameRT.anchoredPosition.y;
        HideBubble();

        ChatController.i.OnAddMessage += OnChatMessage;
    }

    private void OnDestroy()
    {
        ChatController.i.OnAddMessage -= OnChatMessage;
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
        uiContainer.alpha = nameAlpha > VANISHING_ALPHA_THRESHOLD ? nameAlpha : 0;

        if (uiContainer.gameObject.activeSelf != avatarName.uiContainer.gameObject.activeSelf)
        {
            uiContainer.gameObject.SetActive(avatarName.uiContainer.gameObject.activeSelf);
        }
    }

    private void OnChatMessage(ChatMessage message)
    {
        if (message.sender != avatarShape.model.id)
        {
            return;
        }

        ShowBubble(message.body);
    }

    private void HideBubble()
    {
        chatText.text = string.Empty;
        uiContainer.alpha = 0;
        uiContainer.gameObject.SetActive(false);
        Debug.Log($"{avatarShape.model.name} BUBBLE HIDDEN!");
    }

    private void ShowBubble(string text)
    {
        chatText.text = FormatText(text);
        lastMessageTime = Time.unscaledTime;
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
}
