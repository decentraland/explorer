using DCL.Configuration;
using DCL.Helpers;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FriendsListView : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] GameObject friendEntryPrefab;

    [SerializeField] TextMeshProUGUI onlineFriendsToggleText;
    [SerializeField] TextMeshProUGUI offlineFriendsToggleText;

    [SerializeField] GameObject emptyListImage;
    [SerializeField] internal GameObject friendMenuPanel;

    [SerializeField] internal Button friendPassportButton;
    [SerializeField] internal Button blockFriendButton;
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

    public event System.Action<FriendEntry> OnJumpIn;
    public event System.Action<FriendEntry> OnWhisper;
    public event System.Action<FriendEntry> OnDelete;
    public event System.Action<string> OnBlock;
    public event System.Action<string> OnPassport;
    public event System.Action<string> OnReport;

    public void Initialize()
    {
        friendPassportButton.onClick.AddListener(() => { OnPassport?.Invoke(selectedFriendEntry.userId); ToggleMenuPanel(selectedFriendEntry); });
        blockFriendButton.onClick.AddListener(() => { OnBlock?.Invoke(selectedFriendEntry.userId); ToggleMenuPanel(selectedFriendEntry); });
        reportFriendButton.onClick.AddListener(() => { OnReport?.Invoke(selectedFriendEntry.userId); ToggleMenuPanel(selectedFriendEntry); });
        deleteFriendButton.onClick.AddListener(() => { ToggleMenuPanel(selectedFriendEntry); OnFriendDelete(); });

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

    internal FriendEntry GetEntry(string userId)
    {
        if (!friendEntries.ContainsKey(userId))
            return null;

        return friendEntries[userId];
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

        (transform as RectTransform).ForceUpdateLayout();
        return true;
    }

    public bool CreateEntry(string userId)
    {
        if (friendEntries.ContainsKey(userId)) return false;

        if (emptyListImage.activeSelf)
            emptyListImage.SetActive(false);

        var entry = Instantiate(friendEntryPrefab).GetComponent<FriendEntry>();
        friendEntries.Add(userId, entry);

        entry.OnMenuToggle += (x) => { selectedFriendEntry = x; ToggleMenuPanel(x); };

        entry.OnJumpInClick += (x) => OnJumpIn?.Invoke(x);
        entry.OnWhisperClick += (x) => OnWhisper?.Invoke(x);

        return true;
    }

    public void CreateOrUpdateEntry(string userId, FriendEntry.Model model)
    {
        bool firstUpdate = CreateEntry(userId);
        UpdateEntry(userId, model, firstUpdate);
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

        FriendsController.i.UpdateUserStatus(new FriendsController.UserStatus() { userId = id1, presenceStatus = FriendsController.PresenceStatus.ONLINE });
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

        FriendsController.i.UpdateUserStatus(new FriendsController.UserStatus() { userId = id1, presenceStatus = FriendsController.PresenceStatus.OFFLINE });
    }
}
