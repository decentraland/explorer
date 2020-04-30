using DCL.Interface;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public interface IFriendEntry
{
    FriendEntry.Model model { get; }
}

public class FriendEntry : MonoBehaviour, IFriendEntry, IPointerEnterHandler, IPointerExitHandler
{
    public Model model { get; private set; }
    public event System.Action<FriendEntry> OnJumpIn;
    public event System.Action<FriendEntry> OnWhisper;
    public event System.Action<FriendEntry> OnPassport;
    public event System.Action<FriendEntry> OnBlock;
    public event System.Action<FriendEntry> OnReport;
    public event System.Action<FriendEntry> OnDelete;

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

    public void Populate(Model model)
    {
        this.model = model;

        playerNameText.text = model.userName;
        playerLocationText.text = $"{model.realm} {model.coords}";

        playerImage.sprite = model.avatarImage;

        jumpInButton.onClick.RemoveAllListeners();
        jumpInButton.onClick.AddListener(() => WebInterface.GoTo((int)model.coords.x, (int)model.coords.y));

        whisperButton.onClick.RemoveAllListeners();
        whisperButton.onClick.AddListener(() => OnWhisper?.Invoke(this));

        passportButton.onClick.RemoveAllListeners();
        passportButton.onClick.AddListener(() => OnPassport?.Invoke(this));

        blockButton.onClick.RemoveAllListeners();
        blockButton.onClick.AddListener(() => OnBlock?.Invoke(this));

        reportButton.onClick.RemoveAllListeners();
        reportButton.onClick.AddListener(() => OnReport?.Invoke(this));

        deleteButton.onClick.RemoveAllListeners();
        deleteButton.onClick.AddListener(() => OnDelete?.Invoke(this));
    }

    void ToggleMenuPanel()
    {
        menuPanel.SetActive(!menuPanel.activeSelf);
    }
}
