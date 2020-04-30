using DCL.Interface;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class FriendEntry : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
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
        public enum Status
        {
            NONE,
            OFFLINE,
            ONLINE,
            LOADING,
            AFK
        }

        public Status status;
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
        playerNameText.text = model.userName;
        playerLocationText.text = $"{model.realm} {model.coords}";

        playerImage.sprite = model.avatarImage;

        jumpInButton.onClick.RemoveAllListeners();
        jumpInButton.onClick.AddListener(() => WebInterface.GoTo((int)model.coords.x, (int)model.coords.y));

        whisperButton.onClick.RemoveAllListeners();
        whisperButton.onClick.AddListener(WhisperFriend);

        passportButton.onClick.RemoveAllListeners();
        passportButton.onClick.AddListener(WhisperFriend);

        blockButton.onClick.RemoveAllListeners();
        blockButton.onClick.AddListener(WhisperFriend);

        reportButton.onClick.RemoveAllListeners();
        reportButton.onClick.AddListener(WhisperFriend);

        deleteButton.onClick.RemoveAllListeners();
        deleteButton.onClick.AddListener(WhisperFriend);
    }

    void WhisperFriend()
    {
        // TODO: trigger private message to the user
    }

    void OpenFriendPassport()
    {
        // TODO:
    }

    void BlockFriend()
    {
        // TODO:
    }

    void ReportFriend()
    {
        // TODO:
    }

    void DeleteFriend()
    {
        // TODO:
    }

    void ToggleMenuPanel()
    {
        menuPanel.SetActive(!menuPanel.activeSelf);
    }
}
