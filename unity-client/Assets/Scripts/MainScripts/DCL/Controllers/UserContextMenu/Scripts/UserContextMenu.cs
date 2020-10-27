using DCL.Interface;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Contextual menu with different options about an user.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class UserContextMenu : MonoBehaviour
{
    const string BLOCK_BTN_BLOCK_TEXT = "Block";
    const string BLOCK_BTN_UNBLOCK_TEXT = "Unblock";
    const string CURRENT_PLAYER_ID = "CurrentPlayerInfoCardId";

    [System.Flags]
    public enum MenuConfigFlags
    {
        Name = 1,
        Friendship = 2,
        Message = 4,
        Passport = 8,
        Block = 16,
        Report = 32
    }

    const MenuConfigFlags headerFlags = MenuConfigFlags.Name | MenuConfigFlags.Friendship;

    [Header("Enable Actions")]
    [SerializeField] internal MenuConfigFlags menuConfigFlags = MenuConfigFlags.Passport | MenuConfigFlags.Block | MenuConfigFlags.Report;

    [Header("Containers")]
    [SerializeField] internal GameObject headerContainer;
    [SerializeField] internal GameObject bodyContainer;
    [SerializeField] internal GameObject friendshipContainer;
    [SerializeField] internal GameObject friendAddContainer;
    [SerializeField] internal GameObject friendRemoveContainer;
    [SerializeField] internal GameObject friendRequestedContainer;

    [Header("Texts")]
    [SerializeField] internal TextMeshProUGUI userName;
    [SerializeField] internal TextMeshProUGUI blockText;

    [Header("Buttons")]
    [SerializeField] internal Button passportButton;
    [SerializeField] internal Button blockButton;
    [SerializeField] internal Button reportButton;
    [SerializeField] internal Button addFriendButton;
    [SerializeField] internal Button cancelFriendButton;
    [SerializeField] internal Button deleteFriendButton;
    [SerializeField] internal Button messageButton;

    public bool isVisible => gameObject.activeSelf;

    public event System.Action OnShowMenu;
    public event System.Action<string> OnPassport;
    public event System.Action<string> OnReport;
    public event System.Action<string, bool> OnBlock;
    public event System.Action<string> OnDelete;

    private static StringVariable currentPlayerId = null;
    private RectTransform rectTransform;
    private string userId;
    private bool isBlocked;

    private void Awake()
    {
        if (!currentPlayerId)
        {
            currentPlayerId = Resources.Load<StringVariable>(CURRENT_PLAYER_ID);
        }

        rectTransform = GetComponent<RectTransform>();

        passportButton.onClick.AddListener(OnPassportButtonPressed);
        blockButton.onClick.AddListener(OnBlockUserButtonPressed);
        reportButton.onClick.AddListener(OnReportUserButtonPressed);
        if (deleteFriendButton != null)
            deleteFriendButton.onClick.AddListener(OnDeleteUserButtonPressed);
    }

    private void Update()
    {
        HideIfClickedOutside();
    }

    private void OnDestroy()
    {
        passportButton.onClick.RemoveListener(OnPassportButtonPressed);
        blockButton.onClick.RemoveListener(OnBlockUserButtonPressed);
        reportButton.onClick.RemoveListener(OnReportUserButtonPressed);
        if (deleteFriendButton != null)
            deleteFriendButton.onClick.RemoveListener(OnDeleteUserButtonPressed);
    }

    /// <summary>
    /// Configures the context menu with the needed imformation.
    /// </summary>
    /// <param name="userId">User Id</param>
    /// <param name="userName">User name</param>
    public void Initialize(string userId, string userName, bool isBlocked)
    {
        this.userId = userId;
        this.isBlocked = isBlocked;
        if (this.userName != null)
            this.userName.text = userName;
        UpdateBlockButton();
    }

    /// <summary>
    /// Shows the context menu.
    /// </summary>
    public void Show()
    {
        gameObject.SetActive(true);
        UpdateBlockButton();
        OnShowMenu?.Invoke();
    }

    /// <summary>
    /// Hides the context menu.
    /// </summary>
    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void OnPassportButtonPressed()
    {
        OnPassport?.Invoke(userId);
        currentPlayerId.Set(userId);
        Hide();

        AudioScriptableObjects.dialogOpen.Play(true);
    }

    private void OnReportUserButtonPressed()
    {
        OnReport?.Invoke(userId);
        WebInterface.SendReportPlayer(userId);
        Hide();
    }

    private void OnDeleteUserButtonPressed()
    {
        OnDelete?.Invoke(userId);
        Hide();
    }

    private void OnBlockUserButtonPressed()
    {
        bool blockUser = !isBlocked;
        OnBlock?.Invoke(userId, blockUser);
        if (blockUser)
        {
            WebInterface.SendBlockPlayer(userId);
        }
        else
        {
            WebInterface.SendUnblockPlayer(userId);
        }
        Hide();
    }

    private void UpdateBlockButton()
    {
        blockText.text = isBlocked ? BLOCK_BTN_UNBLOCK_TEXT : BLOCK_BTN_BLOCK_TEXT;
    }

    private void HideIfClickedOutside()
    {
        if (Input.GetMouseButtonDown(0) &&
            !RectTransformUtility.RectangleContainsScreenPoint(rectTransform, Input.mousePosition))
        {
            Hide();
        }
    }

    private void ProcessActiveElements(MenuConfigFlags flags)
    {
        bool hasHeader = (flags & headerFlags) != 0;
        headerContainer.SetActive(hasHeader);
        if (hasHeader)
        {
            userName.gameObject.SetActive((flags & MenuConfigFlags.Name) != 0);
            friendshipContainer.SetActive((flags & MenuConfigFlags.Friendship) != 0);
        }
        passportButton.gameObject.SetActive((flags & MenuConfigFlags.Passport) != 0);
        blockButton.gameObject.SetActive((flags & MenuConfigFlags.Block) != 0);
        reportButton.gameObject.SetActive((flags & MenuConfigFlags.Report) != 0);
        messageButton.gameObject.SetActive((flags & MenuConfigFlags.Message) != 0);
    }

#if UNITY_EDITOR
    //This is just to process buttons and container visibility on editor
    private void OnValidate()
    {
        if (headerContainer == null) return;
        ProcessActiveElements(menuConfigFlags);
    }
#endif
}
