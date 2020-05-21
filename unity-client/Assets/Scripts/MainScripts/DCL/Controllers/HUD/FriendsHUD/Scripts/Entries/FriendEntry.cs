using UnityEngine;
using UnityEngine.UI;

public class FriendEntry : FriendEntryBase
{
    public event System.Action<FriendEntry> OnJumpInClick;
    public event System.Action<FriendEntry> OnWhisperClick;

    [SerializeField] internal JumpInButton jumpInButton;
    [SerializeField] internal Button whisperButton;
    [SerializeField] internal UnreadNotificationBadge unreadNotificationBadge;

    public override void Awake()
    {
        base.Awake();

        jumpInButton.button.onClick.RemoveAllListeners();
        jumpInButton.button.onClick.AddListener(() => OnJumpInClick?.Invoke(this));

        whisperButton.onClick.RemoveAllListeners();
        whisperButton.onClick.AddListener(() => OnWhisperClick?.Invoke(this));
    }

    private void Start()
    {
        unreadNotificationBadge.Initialize(ChatController.i, userId);
    }

    public override void Populate(Model model)
    {
        base.Populate(model);
        jumpInButton.UpdateInfo(model);

        if (model.status == PresenceStatus.ONLINE)
            jumpInButton.gameObject.SetActive(true);
        else
            jumpInButton.gameObject.SetActive(false);
    }
}
