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

    public event System.Action<FriendEntry> OnPassportClick;
    public event System.Action<FriendEntry> OnBlockClick;
    public event System.Action<FriendEntry> OnReportClick;
    public event System.Action<FriendEntry> OnDeleteClick;
    public event System.Action<FriendEntry> OnJumpInClick;
    public event System.Action<FriendEntry> OnWhisperClick;

    [SerializeField] internal TextMeshProUGUI playerNameText;
    [SerializeField] internal TextMeshProUGUI playerLocationText;
    [SerializeField] internal Image playerImage;
    [SerializeField] internal Button jumpInButton;
    [SerializeField] internal Button whisperButton;
    [SerializeField] internal Button menuButton;
    [SerializeField] internal Button passportButton;
    [SerializeField] internal Button blockButton;
    [SerializeField] internal Button reportButton;
    [SerializeField] internal Button deleteButton;
    [SerializeField] internal Image backgroundImage;
    [SerializeField] internal GameObject menuPanel;
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

        menuButton.onClick.AddListener(ToggleMenuPanel);

        jumpInButton.onClick.RemoveAllListeners();
        jumpInButton.onClick.AddListener(() => OnJumpInClick?.Invoke(this));

        whisperButton.onClick.RemoveAllListeners();
        whisperButton.onClick.AddListener(() => OnWhisperClick?.Invoke(this));

        passportButton.onClick.RemoveAllListeners();
        passportButton.onClick.AddListener(() => OnPassportClick?.Invoke(this));

        blockButton.onClick.RemoveAllListeners();
        blockButton.onClick.AddListener(() => OnBlockClick?.Invoke(this));

        reportButton.onClick.RemoveAllListeners();
        reportButton.onClick.AddListener(() => OnReportClick?.Invoke(this));

        deleteButton.onClick.RemoveAllListeners();
        deleteButton.onClick.AddListener(() => OnDeleteClick?.Invoke(this));

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
        menuPanel.SetActive(false);
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

    void ToggleMenuPanel()
    {
        menuPanel.SetActive(!menuPanel.activeSelf);
    }
}
