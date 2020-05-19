using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FriendEntry : FriendEntryBase
{
    public event System.Action<FriendEntry> OnJumpInClick;
    public event System.Action<FriendEntry> OnWhisperClick;

    [SerializeField] internal TextMeshProUGUI playerLocationText;
    [SerializeField] internal Button jumpInButton;
    [SerializeField] internal Button whisperButton;
    [SerializeField] internal GameObject whisperLabel;
    [SerializeField] internal UnreadNotificationBadge unreadNotificationBadge;

    public override void Awake()
    {
        base.Awake();

        jumpInButton.onClick.RemoveAllListeners();
        jumpInButton.onClick.AddListener(() => OnJumpInClick?.Invoke(this));

        whisperButton.onClick.RemoveAllListeners();
        whisperButton.onClick.AddListener(() => OnWhisperClick?.Invoke(this));
    }

    private void Start()
    {
        unreadNotificationBadge.Initialize(ChatController.i, userId);
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);

        whisperLabel.SetActive(true);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);

        whisperLabel.SetActive(false);
    }

    public override void Populate(Model model)
    {
        base.Populate(model);

        if (model.status == PresenceStatus.ONLINE)
        {
            playerLocationText.text = $"{model.realm} {(int)model.coords.x}, {(int)model.coords.y}";
            jumpInButton.gameObject.SetActive(true);
        }
        else
        {
            jumpInButton.gameObject.SetActive(false);
            playerLocationText.text = string.Empty;
        }
    }
}
