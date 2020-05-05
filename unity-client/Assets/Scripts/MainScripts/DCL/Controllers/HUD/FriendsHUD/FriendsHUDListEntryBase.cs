using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FriendsHUDListEntry : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public class Model
    {
        public FriendsController.PresenceStatus status;
        public string userName;
        public Vector2 coords;
        public string realm;
        public Sprite avatarImage;

        public event System.Action<Sprite> OnSpriteUpdateEvent;
        public void OnSpriteUpdate(Sprite sprite)
        {
            OnSpriteUpdateEvent?.Invoke(sprite);
        }
    }

    public Model model { get; private set; } = new Model();
    public string userId { get; private set; }
    public Image playerBlockedImage;
    public Transform menuPositionReference;

    [SerializeField] protected internal TextMeshProUGUI playerNameText;
    [SerializeField] protected internal Image playerImage;
    [SerializeField] protected internal Button menuButton;
    [SerializeField] protected internal Image backgroundImage;
    [SerializeField] protected internal Sprite hoveredBackgroundSprite;
    protected internal Sprite unhoveredBackgroundSprite;

    public event System.Action<FriendsHUDListEntry> OnMenuToggle;

    protected virtual void Awake()
    {
        unhoveredBackgroundSprite = backgroundImage.sprite;

        menuButton.onClick.AddListener(() => OnMenuToggle?.Invoke(this));
    }

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        backgroundImage.sprite = hoveredBackgroundSprite;
        menuButton.gameObject.SetActive(true);
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        backgroundImage.sprite = unhoveredBackgroundSprite;
        menuButton.gameObject.SetActive(false);
    }

    protected virtual void OnDisable()
    {
        OnPointerExit(null);

        model.OnSpriteUpdateEvent -= OnAvatarImageChange;
    }

    public virtual void Populate(string userId, Model model)
    {
        this.userId = userId;
        this.model = model;
        playerNameText.text = model.userName;

        model.OnSpriteUpdateEvent -= OnAvatarImageChange;
        model.OnSpriteUpdateEvent += OnAvatarImageChange;
        playerImage.sprite = model.avatarImage;
    }

    private void OnAvatarImageChange(Sprite sprite)
    {
        playerImage.sprite = sprite;
    }

    public void ToggleBlockedImage(bool targetState)
    {
        playerBlockedImage.enabled = targetState;
    }
}
