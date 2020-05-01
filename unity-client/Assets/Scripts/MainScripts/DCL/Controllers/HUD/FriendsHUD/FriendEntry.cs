using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public interface IFriendEntry
{
    FriendEntry.Model model { get; }
}

public class FriendEntry : MonoBehaviour, IFriendEntry, IPointerEnterHandler, IPointerExitHandler
{
    public Model model { get; private set; }
    public string userId { get; private set; }
    public Transform menuPositionReference;

    public event System.Action<FriendEntry> OnMenuToggle;
    public event System.Action<FriendEntry> OnJumpInClick;
    public event System.Action<FriendEntry> OnWhisperClick;

    [SerializeField] internal TextMeshProUGUI playerNameText;
    [SerializeField] internal TextMeshProUGUI playerLocationText;
    [SerializeField] internal Image playerImage;
    [SerializeField] internal Button jumpInButton;
    [SerializeField] internal Button whisperButton;
    [SerializeField] internal Button menuButton;
    [SerializeField] internal Image backgroundImage;
    [SerializeField] internal GameObject whisperLabel;
    [SerializeField] internal Sprite hoveredBackgroundSprite;

    internal Sprite unhoveredBackgroundSprite;

    public struct Model
    {
        public FriendsController.PresenceStatus status;
        public string userName;
        public Vector2 coords;
        public string realm;
        public Sprite avatarImage;
    }

    void Awake()
    {
        unhoveredBackgroundSprite = backgroundImage.sprite;

        menuButton.onClick.RemoveAllListeners();
        menuButton.onClick.AddListener(() => OnMenuToggle?.Invoke(this));

        jumpInButton.onClick.RemoveAllListeners();
        jumpInButton.onClick.AddListener(() => OnJumpInClick?.Invoke(this));

        whisperButton.onClick.RemoveAllListeners();
        whisperButton.onClick.AddListener(() => OnWhisperClick?.Invoke(this));
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        backgroundImage.sprite = hoveredBackgroundSprite;
        menuButton.gameObject.SetActive(true);
        whisperLabel.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        backgroundImage.sprite = unhoveredBackgroundSprite;
        menuButton.gameObject.SetActive(false);
        whisperLabel.SetActive(false);
    }

    void OnDisable()
    {
        OnPointerExit(null);
    }

    public void Populate(string userId, Model model)
    {
        this.model = model;
        this.userId = userId;

        playerNameText.text = model.userName;
        playerLocationText.text = $"{model.realm} {model.coords}";

        playerImage.sprite = model.avatarImage;
    }
}
