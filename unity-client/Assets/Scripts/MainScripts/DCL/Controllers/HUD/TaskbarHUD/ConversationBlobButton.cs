using UnityEngine;
using UnityEngine.UI;

public class ConversationBlobButton : MonoBehaviour
{
    public NotificationBadge badge;
    public Button closeButton;
    public TMPro.TextMeshProUGUI label;
    public Sprite portrait;

    public event System.Action OnClose;
    public void Initialize(UserProfile profile)
    {
        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(OnCloseButtonPressed);
    }

    private void OnCloseButtonPressed()
    {
        OnClose?.Invoke();
    }
}
