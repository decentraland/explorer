using TMPro;
using UnityEngine;
using UnityEngine.UI;

public interface IFriendEntry
{
    FriendEntry.Model model { get; }
}
public class FriendEntry : MonoBehaviour, IFriendEntry
{
    [SerializeField] internal TextMeshProUGUI playerNameText;
    [SerializeField] internal TextMeshProUGUI playerLocationText;
    [SerializeField] internal Image playerImage;
    [SerializeField] internal Button jumpInButton;
    [SerializeField] internal Button whisperButton;

    public struct Model
    {
        public FriendsController.PresenceStatus status;
        public string userName;
        public Vector2 coords;
        public string realm;
        public Sprite avatarImage;
    }

    public Model model { get; private set; }

    public event System.Action<FriendEntry> OnJumpInClick;
    public event System.Action<FriendEntry> OnWhisperClick;

    public void Awake()
    {
        jumpInButton.onClick.RemoveAllListeners();
        jumpInButton.onClick.AddListener(() => OnJumpInClick?.Invoke(this));

        whisperButton.onClick.RemoveAllListeners();
        whisperButton.onClick.AddListener(() => OnWhisperClick?.Invoke(this));
    }

    public void Populate(Model model)
    {
        this.model = model;

        playerNameText.text = model.userName;
        playerLocationText.text = $"{model.realm} {model.coords}";

        playerImage.sprite = model.avatarImage;
    }
}
