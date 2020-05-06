using DCL.Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class FriendRequestsTabView : FriendsTabViewBase
{
    [SerializeField] internal Transform receivedRequestsContainer;
    [SerializeField] internal Transform sentRequestsContainer;

    [SerializeField] internal TMP_InputField friendSearchInputField;
    [SerializeField] internal Button addFriendButton;
    [SerializeField] internal TextMeshProUGUI receivedRequestsToggleText;
    [SerializeField] internal TextMeshProUGUI sentRequestsToggleText;

    [Header("Notifications")]
    [SerializeField] internal Notification requestSentNotification;
    [SerializeField] internal Notification friendSearchFailedNotification;
    [SerializeField] internal Notification acceptedFriendNotification;

    internal int receivedRequests = 0;
    internal int sentRequests = 0;

    public event System.Action<FriendRequestEntry> OnFriendRequestCancelled;
    public event System.Action<FriendRequestEntry> OnFriendRequestRejected;
    public event System.Action<FriendRequestEntry> OnFriendRequestApproved;
    public event System.Action<string> OnFriendRequestSent;

    public override void Initialize(FriendsHUDView owner)
    {
        base.Initialize(owner);

        requestSentNotification.model.timer = owner.notificationsDuration;
        requestSentNotification.model.groupID = FriendsHUDView.NOTIFICATIONS_ID;

        friendSearchFailedNotification.model.timer = owner.notificationsDuration;
        friendSearchFailedNotification.model.groupID = FriendsHUDView.NOTIFICATIONS_ID;

        acceptedFriendNotification.model.timer = owner.notificationsDuration;
        acceptedFriendNotification.model.groupID = FriendsHUDView.NOTIFICATIONS_ID;

        friendSearchInputField.onSubmit.AddListener(SendFriendRequest);
        friendSearchInputField.onValueChanged.AddListener(OnSearchInputValueChanged);
        addFriendButton.onClick.AddListener(() => friendSearchInputField.OnSubmit(null));
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        NotificationsController.i.DismissAllNotifications(FriendsHUDView.NOTIFICATIONS_ID);
    }

    public void CreateOrUpdateEntry(string userId, FriendEntry.Model model, bool isReceived)
    {
        CreateEntry(userId);
        UpdateEntry(userId, model, isReceived);
    }

    public override bool CreateEntry(string userId)
    {
        if (entries.ContainsKey(userId)) return false;

        if (emptyListImage.activeSelf)
            emptyListImage.SetActive(false);

        if (!sentRequestsToggleText.transform.parent.gameObject.activeSelf)
        {
            receivedRequestsToggleText.transform.parent.gameObject.SetActive(true);
            sentRequestsToggleText.transform.parent.gameObject.SetActive(true);
        }

        FriendRequestEntry entry;

        entry = Instantiate(entryPrefab).GetComponent<FriendRequestEntry>();
        entry.OnAccepted += OnFriendRequestReceivedAccepted;
        entry.OnMenuToggle += (x) => { selectedEntry = x; ToggleMenuPanel(x); };
        entry.OnRejected += OnFriendRequestReceivedRejected;
        entry.OnCancelled += OnFriendRequestSentCancelled;
        entries.Add(userId, entry);

        return true;
    }

    public bool UpdateEntry(string userId, FriendEntryBase.Model model, bool? isReceived = null)
    {
        if (!entries.ContainsKey(userId))
            return false;

        FriendRequestEntry entry = entries[userId] as FriendRequestEntry;
        entry.userId = userId;
        entry.Populate(model, isReceived);

        if (isReceived.HasValue)
        {
            if (isReceived.Value)
            {
                entry.transform.SetParent(receivedRequestsContainer);
                receivedRequests++;
            }
            else
            {
                entry.transform.SetParent(sentRequestsContainer);
                sentRequests++;
            }

            UpdateUsersToggleTexts();
        }

        entry.transform.localScale = Vector3.one;
        rectTransform.ForceUpdateLayout();

        return true;
    }

    public override void RemoveEntry(string userId)
    {
        if (!entries.ContainsKey(userId)) return;

        FriendRequestEntry entry = entries[userId] as FriendRequestEntry;

        if (entry.isReceived)
            receivedRequests--;
        else
            sentRequests--;

        UpdateUsersToggleTexts();

        Destroy(entry.gameObject);
        entries.Remove(userId);

        if (entries.Count == 0)
        {
            emptyListImage.SetActive(true);
            receivedRequestsToggleText.transform.parent.gameObject.SetActive(false);
            sentRequestsToggleText.transform.parent.gameObject.SetActive(false);
        }

        rectTransform.ForceUpdateLayout();
    }

    void SendFriendRequest(string friendId)
    {
        requestSentNotification.model.message = $"Your request to {friendId} successfully sent!";
        NotificationsController.i.ShowNotification(requestSentNotification);

        friendSearchInputField.placeholder.enabled = true;
        friendSearchInputField.text = string.Empty;

        addFriendButton.gameObject.SetActive(false);

        OnFriendRequestSent?.Invoke(friendId);
    }

    public void DisplayFriendUserNotFound()
    {
        NotificationsController.i.ShowNotification(friendSearchFailedNotification);
        addFriendButton.interactable = false;
    }

    void OnSearchInputValueChanged(string friendId)
    {
        if (!addFriendButton.gameObject.activeSelf)
            addFriendButton.gameObject.SetActive(true);

        if (!addFriendButton.interactable)
            addFriendButton.interactable = true;

        NotificationsController.i.DismissAllNotifications(FriendsHUDView.NOTIFICATIONS_ID);
    }

    void OnFriendRequestReceivedAccepted(FriendRequestEntry requestEntry)
    {
        // Add placeholder friend to avoid affecting UX by roundtrip with kernel
        FriendsController.i.UpdateFriendshipStatus(new FriendsController.FriendshipUpdateStatusMessage()
        {
            userId = requestEntry.userId,
            action = FriendsController.FriendshipAction.APPROVED
        });

        FriendsController.i.UpdateUserStatus(new FriendsController.UserStatus()
        {
            userId = requestEntry.userId,
            presence = FriendsController.PresenceStatus.OFFLINE
        });

        acceptedFriendNotification.model.message = $"You and {requestEntry.model.userName} are now friends!";
        NotificationsController.i.ShowNotification(acceptedFriendNotification);

        RemoveEntry(requestEntry.userId);

        OnFriendRequestApproved?.Invoke(requestEntry);
    }

    void OnFriendRequestReceivedRejected(FriendRequestEntry requestEntry)
    {
        selectedEntry = requestEntry;

        TriggerDialog($"Are you sure you want to reject {requestEntry.model.userName} friend request?", ConfirmFriendRequestReceivedRejection);
    }

    void ConfirmFriendRequestReceivedRejection()
    {
        if (selectedEntry == null) return;

        RemoveEntry(selectedEntry.userId);
        OnFriendRequestRejected?.Invoke(selectedEntry as FriendRequestEntry);
    }

    void OnFriendRequestSentCancelled(FriendRequestEntry requestEntry)
    {
        selectedEntry = requestEntry;

        TriggerDialog($"Are you sure you want to cancel {requestEntry.model.userName} friend request?", ConfirmFriendRequestSentCancellation);
    }

    void ConfirmFriendRequestSentCancellation()
    {
        if (selectedEntry == null) return;

        RemoveEntry(selectedEntry.userId);
        OnFriendRequestCancelled?.Invoke(selectedEntry as FriendRequestEntry);
    }

    void UpdateUsersToggleTexts()
    {
        receivedRequestsToggleText.text = $"RECEIVED ({receivedRequests})";
        sentRequestsToggleText.text = $"SENT ({sentRequests})";
    }

    [ContextMenu("AddFakeRequestReceived")]
    public void AddFakeRequestReceived()
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
            action = FriendsController.FriendshipAction.REQUESTED_FROM
        });
    }

    [ContextMenu("AddFakeRequestSent")]
    public void AddFakeRequestSent()
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
            action = FriendsController.FriendshipAction.REQUESTED_TO
        });
    }
}
