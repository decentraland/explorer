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

    protected override void Awake()
    {
        base.Awake();

        jumpInButton.onClick.RemoveAllListeners();
        jumpInButton.onClick.AddListener(() => OnJumpInClick?.Invoke(this));

        whisperButton.onClick.RemoveAllListeners();
        whisperButton.onClick.AddListener(() => OnWhisperClick?.Invoke(this));
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

    public override void Populate(string userId, Model model)
    {
        base.Populate(userId, model);

        if (model.status == FriendsController.PresenceStatus.ONLINE || model.status == FriendsController.PresenceStatus.UNAVAILABLE)
            playerLocationText.text = $"{model.realm} {model.coords.x}, {model.coords.y}";
        else
            playerLocationText.text = $"";
    }

    private void OnAvatarImageChange(Sprite sprite)
    {
        playerImage.sprite = sprite;
    }
}
