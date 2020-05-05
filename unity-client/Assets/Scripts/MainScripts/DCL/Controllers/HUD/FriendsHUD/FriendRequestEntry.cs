using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FriendRequestEntry : MonoBehaviour, IFriendEntry, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] internal TextMeshProUGUI playerNameText;
    [SerializeField] internal Image playerImage;
    [SerializeField] internal Button menuButton;
    [SerializeField] internal Button acceptButton;
    [SerializeField] internal Button rejectButton;
    [SerializeField] internal Button cancelButton;
    [SerializeField] internal Image backgroundImage;
    [SerializeField] internal Sprite hoveredBackgroundSprite;
    public Transform menuPositionReference;
    public Image playerBlockedImage;

    public string userId { get; private set; }
    public bool isReceived { get; private set; }

    public FriendEntry.Model model { get; private set; }
    internal Sprite unhoveredBackgroundSprite;

    public event System.Action<FriendRequestEntry> OnMenuToggle;
    public event System.Action<FriendRequestEntry> OnAccepted;
    public event System.Action<FriendRequestEntry> OnRejected;
    public event System.Action<FriendRequestEntry> OnCancelled;

    public void Awake()
    {
        unhoveredBackgroundSprite = backgroundImage.sprite;

        menuButton.onClick.AddListener(() => OnMenuToggle?.Invoke(this));
        acceptButton.onClick.AddListener(() => OnAccepted?.Invoke(this));
        rejectButton.onClick.AddListener(() => OnRejected?.Invoke(this));
        cancelButton.onClick.AddListener(() => OnCancelled?.Invoke(this));
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        backgroundImage.sprite = hoveredBackgroundSprite;
        menuButton.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        backgroundImage.sprite = unhoveredBackgroundSprite;
        menuButton.gameObject.SetActive(false);
    }

    void OnDisable()
    {
        OnPointerExit(null);
    }

    public void Populate(string userId, FriendEntry.Model model, bool? isReceived = null)
    {
        this.userId = userId;
        this.model = model;
        playerNameText.text = model.userName;

        if (isReceived.HasValue)
        {
            if (isReceived.Value)
                PopulateReceived();
            else
                PopulateSent();
        }

        model.OnSpriteUpdateEvent -= OnAvatarImageChange;
        model.OnSpriteUpdateEvent += OnAvatarImageChange;
        playerImage.sprite = model.avatarImage;
    }

    void OnDestroy()
    {
        model.OnSpriteUpdateEvent -= OnAvatarImageChange;
    }

    private void OnAvatarImageChange(Sprite sprite)
    {
        playerImage.sprite = sprite;
    }

    void PopulateReceived()
    {
        isReceived = true;
        cancelButton.gameObject.SetActive(false);
        acceptButton.gameObject.SetActive(true);
        rejectButton.gameObject.SetActive(true);
    }

    void PopulateSent()
    {
        isReceived = false;
        cancelButton.gameObject.SetActive(true);
        acceptButton.gameObject.SetActive(false);
        rejectButton.gameObject.SetActive(false);
    }

    public void ToggleBlockedImage(bool targetState)
    {
        playerBlockedImage.enabled = targetState;
    }
}
