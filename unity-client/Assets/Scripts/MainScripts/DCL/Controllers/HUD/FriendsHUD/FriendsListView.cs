using DCL.Configuration;
using DCL.Helpers;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FriendsListView : MonoBehaviour, IPointerDownHandler
{
    const string BLOCK_BTN_BLOCK_TEXT = "Block";
    const string BLOCK_BTN_UNBLOCK_TEXT = "Unblock";

    [SerializeField] GameObject friendEntryPrefab;

    [SerializeField] TextMeshProUGUI onlineFriendsToggleText;
    [SerializeField] TextMeshProUGUI offlineFriendsToggleText;

    [SerializeField] GameObject emptyListImage;
    [SerializeField] internal GameObject friendMenuPanel;

    [SerializeField] internal Button friendPassportButton;
    [SerializeField] internal Button blockFriendButton;
    [SerializeField] internal TextMeshProUGUI blockFriendButtonText;
    [SerializeField] internal Button reportFriendButton;

    [SerializeField] internal Button deleteFriendButton;
    [SerializeField] GameObject deleteFriendDialog;
    [SerializeField] TextMeshProUGUI deleteFriendDialogText;
    [SerializeField] internal Button deleteFriendDialogCancelButton;
    [SerializeField] internal Button deleteFriendDialogConfirmButton;

    Dictionary<string, FriendEntry> friendEntries = new Dictionary<string, FriendEntry>();

    internal FriendEntry selectedFriendEntry;

    public Transform onlineFriendsContainer;
    public Transform offlineFriendsContainer;
    internal int onlineFriends = 0;
    internal int offlineFriends = 0;
    UserProfile ownUserProfile => UserProfile.GetOwnUserProfile();

    public event System.Action<FriendEntry> OnJumpIn;
    public event System.Action<FriendEntry> OnWhisper;
    public event System.Action<FriendEntry> OnDelete;
    public event System.Action<string> OnBlock;
    public event System.Action<string> OnPassport;
    public event System.Action<string> OnReport;

    public void Initialize()
    {
        friendPassportButton.onClick.AddListener(OnPassportButtonPressed);
        reportFriendButton.onClick.AddListener(OnReportFriendButtonPressed);
        deleteFriendButton.onClick.AddListener(OnDeleteFriendButtonPressed);
        blockFriendButton.onClick.AddListener(OnBlockFriendButtonPressed);

        deleteFriendDialogConfirmButton.onClick.AddListener(ConfirmFriendDelete);
        deleteFriendDialogCancelButton.onClick.AddListener(CancelConfirmationDialog);
    }

    void OnEnable()
    {
        (transform as RectTransform).ForceUpdateLayout();
    }

    void OnDisable()
    {
        CancelConfirmationDialog();
        friendMenuPanel.SetActive(false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.pointerPressRaycast.gameObject == null || eventData.pointerPressRaycast.gameObject.layer != PhysicsLayers.friendsHUDPlayerMenu)
            friendMenuPanel.SetActive(false);
    }

    void OnPassportButtonPressed()
    {
        OnPassport?.Invoke(selectedFriendEntry.userId);

        ToggleMenuPanel(selectedFriendEntry);
    }

    void OnReportFriendButtonPressed()
    {
        OnReport?.Invoke(selectedFriendEntry.userId);

        ToggleMenuPanel(selectedFriendEntry);
    }

    void OnDeleteFriendButtonPressed()
    {
        ToggleMenuPanel(selectedFriendEntry);

        OnFriendDelete();
    }

    void OnBlockFriendButtonPressed()
    {
        OnBlock?.Invoke(selectedFriendEntry.userId);

        selectedFriendEntry.ToggleBlockedImage(!selectedFriendEntry.playerBlockedImage.enabled);

        ToggleMenuPanel(selectedFriendEntry);
    }

    internal FriendEntry GetEntry(string userId)
    {
        if (!friendEntries.ContainsKey(userId))
            return null;

        return friendEntries[userId];
    }

    public void CreateOrUpdateEntry(string userId, FriendEntry.Model model)
    {
        bool firstUpdate = CreateEntry(userId);
        UpdateEntry(userId, model, firstUpdate);
    }

    public bool CreateEntry(string userId)
    {
        if (friendEntries.ContainsKey(userId)) return false;

        if (emptyListImage.activeSelf)
            emptyListImage.SetActive(false);

        if (!onlineFriendsToggleText.transform.parent.gameObject.activeSelf)
        {
            onlineFriendsToggleText.transform.parent.gameObject.SetActive(true);
            offlineFriendsToggleText.transform.parent.gameObject.SetActive(true);
        }

        var entry = Instantiate(friendEntryPrefab).GetComponent<FriendEntry>();
        friendEntries.Add(userId, entry);

        entry.OnMenuToggle += (x) => { selectedFriendEntry = x; ToggleMenuPanel(x); };

        entry.OnJumpInClick += (x) => OnJumpIn?.Invoke(x);
        entry.OnWhisperClick += (x) => OnWhisper?.Invoke(x);

        return true;
    }

    public bool UpdateEntry(string userId, FriendEntry.Model model, bool firstUpdate = false)
    {
        if (!friendEntries.ContainsKey(userId)) return false;

        var friendEntry = friendEntries[userId];
        var previousStatus = friendEntry.model.status;

        friendEntry.Populate(userId, model);

        if (friendEntry.model.status == FriendsController.PresenceStatus.ONLINE)
        {
            friendEntry.transform.SetParent(onlineFriendsContainer);
            onlineFriends++;
        }
        else
        {
            friendEntry.transform.SetParent(offlineFriendsContainer);
            offlineFriends++;
        }

        friendEntry.transform.localScale = Vector3.one;

        if (!firstUpdate)
        {
            if (previousStatus == FriendsController.PresenceStatus.ONLINE)
                onlineFriends--;
            else
                offlineFriends--;
        }

        UpdateUsersToggleTexts();

        friendEntry.ToggleBlockedImage(ownUserProfile.blocked.Contains(userId));

        (transform as RectTransform).ForceUpdateLayout();

        return true;
    }

    public void RemoveEntry(string userId)
    {
        if (!friendEntries.ContainsKey(userId)) return;

        var entry = friendEntries[userId];

        if (entry.model.status == FriendsController.PresenceStatus.ONLINE)
            onlineFriends--;
        else
            offlineFriends--;

        UpdateUsersToggleTexts();

        Object.Destroy(entry.gameObject);
        friendEntries.Remove(userId);

        if (friendEntries.Count == 0)
        {
            onlineFriendsToggleText.transform.parent.gameObject.SetActive(false);
            offlineFriendsToggleText.transform.parent.gameObject.SetActive(false);
        }

        (transform as RectTransform).ForceUpdateLayout();
    }

    void UpdateUsersToggleTexts()
    {
        onlineFriendsToggleText.text = $"ONLINE ({onlineFriends})";
        offlineFriendsToggleText.text = $"OFFLINE ({offlineFriends})";
    }

    void OnFriendDelete()
    {
        if (selectedFriendEntry == null) return;

        deleteFriendDialogText.text = $"Are you sure you want to delete {selectedFriendEntry.model.userName} as a friend?";
        deleteFriendDialog.SetActive(true);
    }

    void ConfirmFriendDelete()
    {
        if (selectedFriendEntry == null) return;

        RemoveEntry(selectedFriendEntry.userId);

        OnDelete?.Invoke(selectedFriendEntry);

        deleteFriendDialog.SetActive(false);
        selectedFriendEntry = null;
    }

    void CancelConfirmationDialog()
    {
        selectedFriendEntry = null;
        deleteFriendDialog.SetActive(false);
    }

    void ToggleMenuPanel(FriendEntry entry)
    {
        friendMenuPanel.transform.position = entry.menuPositionReference.position;

        friendMenuPanel.SetActive(selectedFriendEntry == entry ? !friendMenuPanel.activeSelf : true);

        if (friendMenuPanel.activeSelf)
            blockFriendButtonText.text = ownUserProfile.blocked.Contains(entry.userId) ? BLOCK_BTN_UNBLOCK_TEXT : BLOCK_BTN_BLOCK_TEXT;
    }

    [ContextMenu("AddFakeOnlineFriend")]
    public void AddFakeOnlineFriend()
    {
        string id1 = Random.Range(0, 1000000).ToString();

        UserProfileController.i.AddUserProfileToCatalog(new UserProfileModel()
        {
            userId = id1,
            name = "Brian-" + id1
        });

        FriendsController.i.UpdateFriendshipStatus(new FriendsController.FriendshipUpdateStatusMessage()
        {
            userId = id1,
            action = FriendsController.FriendshipAction.APPROVED
        });

        FriendsController.i.UpdateUserStatus(new FriendsController.UserStatus() { userId = id1, presence = FriendsController.PresenceStatus.ONLINE });
    }

    [ContextMenu("AddFakeOfflineFriend")]
    public void AddFakeOfflineFriend()
    {
        string id1 = Random.Range(0, 1000000).ToString();

        UserProfileController.i.AddUserProfileToCatalog(new UserProfileModel()
        {
            userId = id1,
            name = "Pravus-" + id1
        });

        FriendsController.i.UpdateFriendshipStatus(new FriendsController.FriendshipUpdateStatusMessage()
        {
            userId = id1,
            action = FriendsController.FriendshipAction.APPROVED
        });

        FriendsController.i.UpdateUserStatus(new FriendsController.UserStatus() { userId = id1, presence = FriendsController.PresenceStatus.OFFLINE });
    }
}
