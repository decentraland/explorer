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
    public event System.Action<string> OnUnfriend;
    public event System.Action<string> OnAddFriend;
    public event System.Action<string> OnCancelFriend;
    public event System.Action<string> OnMessage;

    private static StringVariable currentPlayerId = null;
    private RectTransform rectTransform;
    private string userId;
    private bool isBlocked;
    private MenuConfigFlags currentConfigFlags;

    public void Show(string userId, MenuConfigFlags configFlags)
    {
        this.userId = userId;
        ProcessActiveElements(configFlags);
        Setup(userId, configFlags);
        gameObject.SetActive(true);
        OnShowMenu?.Invoke();
    }

    public void Show(string userId)
    {
        Show(userId, menuConfigFlags);
    }

    /// <summary>
    /// Hides the context menu.
    /// </summary>
    public void Hide()
    {
        gameObject.SetActive(false);
    }

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
        deleteFriendButton.onClick.AddListener(OnDeleteUserButtonPressed);
        addFriendButton.onClick.AddListener(OnAddFriendButtonPressed);
        cancelFriendButton.onClick.AddListener(OnCancelFriendRequestButtonPressed);
        messageButton.onClick.AddListener(OnMessageButtonPressed);
    }

    private void Update()
    {
        HideIfClickedOutside();
    }

    private void OnDisable()
    {
        if (FriendsController.i)
            FriendsController.i.OnUpdateUserStatus -= OnFriendStatusUpdate;
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
        OnUnfriend?.Invoke(userId);
        Hide();
    }

    private void OnAddFriendButtonPressed()
    {
        OnAddFriend?.Invoke(userId);
        Hide();
    }

    private void OnCancelFriendRequestButtonPressed()
    {
        OnCancelFriend?.Invoke(userId);
        Hide();
    }

    private void OnMessageButtonPressed()
    {
        OnMessage?.Invoke(userId);
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
        headerContainer.SetActive((flags & headerFlags) != 0);
        userName.gameObject.SetActive((flags & MenuConfigFlags.Name) != 0);
        friendshipContainer.SetActive((flags & MenuConfigFlags.Friendship) != 0);
        deleteFriendButton.gameObject.SetActive((flags & MenuConfigFlags.Friendship) != 0);
        passportButton.gameObject.SetActive((flags & MenuConfigFlags.Passport) != 0);
        blockButton.gameObject.SetActive((flags & MenuConfigFlags.Block) != 0);
        reportButton.gameObject.SetActive((flags & MenuConfigFlags.Report) != 0);
        messageButton.gameObject.SetActive((flags & MenuConfigFlags.Message) != 0);
    }

    private void Setup(string userId, MenuConfigFlags configFlags)
    {
        this.userId = userId;
        this.currentConfigFlags = configFlags;

        ProcessActiveElements(configFlags);

        if ((configFlags & MenuConfigFlags.Block) != 0)
        {
            isBlocked = UserProfile.GetOwnUserProfile().blocked.Contains(userId);
            UpdateBlockButton();
        }
        if ((configFlags & MenuConfigFlags.Name) != 0)
        {
            string name = UserProfileController.userProfilesCatalog.Get(userId)?.name;
            userName.text = name;
        }
        if ((configFlags & MenuConfigFlags.Friendship) != 0 && FriendsController.i)
        {
            if (FriendsController.i.friends.TryGetValue(userId, out FriendsController.UserStatus status))
            {
                OnFriendStatusUpdate(userId, status);
            }
            else
            {
                SetupFriendship(userId, FriendshipStatus.NONE);
            }
            FriendsController.i.OnUpdateUserStatus -= OnFriendStatusUpdate;
            FriendsController.i.OnUpdateUserStatus += OnFriendStatusUpdate;
        }
    }

    private void SetupFriendship(string userId, FriendshipStatus friendshipStatus)
    {
        if (friendshipStatus == FriendshipStatus.FRIEND)
        {
            friendAddContainer.SetActive(false);
            friendRemoveContainer.SetActive(true);
            friendRequestedContainer.SetActive(false);
            deleteFriendButton.gameObject.SetActive(true);
        }
        else if (friendshipStatus == FriendshipStatus.REQUESTED_TO)
        {
            friendAddContainer.SetActive(false);
            friendRemoveContainer.SetActive(false);
            friendRequestedContainer.SetActive(true);
            deleteFriendButton.gameObject.SetActive(false);
        }
        else if (friendshipStatus == FriendshipStatus.NONE)
        {
            friendAddContainer.SetActive(true);
            friendRemoveContainer.SetActive(false);
            friendRequestedContainer.SetActive(false);
            deleteFriendButton.gameObject.SetActive(false);
        }
        else if (friendshipStatus == FriendshipStatus.REQUESTED_FROM)
        {
            friendAddContainer.SetActive(true);
            friendRemoveContainer.SetActive(false);
            friendRequestedContainer.SetActive(false);
            deleteFriendButton.gameObject.SetActive(false);
        }
    }

    private void OnFriendStatusUpdate(string userId, FriendsController.UserStatus status)
    {
        SetupFriendship(userId, status.friendshipStatus);
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
