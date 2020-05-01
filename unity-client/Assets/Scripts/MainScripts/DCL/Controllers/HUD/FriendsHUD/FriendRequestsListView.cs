using DCL.Helpers;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FriendRequestsListView : MonoBehaviour
{
    public float notificationsDuration = 3f;

    [SerializeField] GameObject friendRequestEntryPrefab;
    [SerializeField] internal Transform receivedRequestsContainer;
    [SerializeField] internal Transform sentRequestsContainer;

    [SerializeField] TMP_InputField friendSearchInputField;
    [SerializeField] GameObject emptyListImage;
    [SerializeField] GameObject requestMenuPanel;
    [SerializeField] Button addFriendButton;
    [SerializeField] Button playerPassportButton;
    [SerializeField] Button blockPlayerButton;
    [SerializeField] TextMeshProUGUI receivedRequestsToggleText;
    [SerializeField] TextMeshProUGUI sentRequestsToggleText;

    [Header("Notifications")]
    [SerializeField] GameObject requestSentNotification;
    [SerializeField] TextMeshProUGUI requestSentNotificationText;
    [SerializeField] GameObject friendSearchFailedNotification;
    [SerializeField] GameObject acceptedFriendNotification;
    [SerializeField] TextMeshProUGUI acceptedFriendNotificationText;

    [Header("Confirmation Dialogs")]
    [SerializeField] GameObject rejectRequestDialog;
    [SerializeField] TextMeshProUGUI rejectRequestDialogText;
    [SerializeField] Button rejectRequestDialogCancelButton;
    [SerializeField] Button rejectRequestDialogConfirmButton;
    [SerializeField] GameObject cancelRequestDialog;
    [SerializeField] TextMeshProUGUI cancelRequestDialogText;
    [SerializeField] Button cancelRequestDialogCancelButton;
    [SerializeField] Button cancelRequestDialogConfirmButton;

    Dictionary<string, FriendRequestEntry> friendRequestEntries = new Dictionary<string, FriendRequestEntry>();
    Coroutine currentNotificationRoutine = null;
    GameObject currentNotification = null;
    FriendRequestEntry selectedRequestEntry = null;
    int receivedRequests = 0;
    int sentRequests = 0;

    public event System.Action<FriendRequestEntry> OnFriendRequestCancelled;
    public event System.Action<FriendRequestEntry> OnFriendRequestRejected;
    public event System.Action<FriendRequestEntry> OnFriendRequestApproved;
    public event System.Action<string> OnBlock;
    public event System.Action<string> OnPassport;
    public event System.Action<string> OnFriendRequestSent;

    public int entriesCount => friendRequestEntries.Count;
    internal FriendRequestEntry GetEntry(string userId)
    {
        if (!friendRequestEntries.ContainsKey(userId))
            return null;

        return friendRequestEntries[userId];
    }

    void Awake()
    {
        friendSearchInputField.onSubmit.AddListener(SendFriendRequest);
        friendSearchInputField.onValueChanged.AddListener(OnSearchInputValueChanged);
        addFriendButton.onClick.AddListener(() => friendSearchInputField.OnSubmit(null));

        playerPassportButton.onClick.AddListener(() => { OnPassport?.Invoke(selectedRequestEntry.userId); ToggleMenuPanel(selectedRequestEntry); });
        blockPlayerButton.onClick.AddListener(() => { OnBlock?.Invoke(selectedRequestEntry.userId); ToggleMenuPanel(selectedRequestEntry); });

        rejectRequestDialogConfirmButton.onClick.AddListener(ConfirmFriendRequestReceivedRejection);
        cancelRequestDialogConfirmButton.onClick.AddListener(ConfirmFriendRequestSentCancellation);

        rejectRequestDialogCancelButton.onClick.AddListener(CancelConfirmationDialog);
        cancelRequestDialogCancelButton.onClick.AddListener(CancelConfirmationDialog);
    }

    void OnDisable()
    {
        if (currentNotificationRoutine != null)
        {
            StopCoroutine(currentNotificationRoutine);
            currentNotification.SetActive(false);
            currentNotification = null;
        }

        CancelConfirmationDialog();

        requestMenuPanel.SetActive(false);
    }

    void SendFriendRequest(string friendId)
    {
        requestSentNotificationText.text = $"Your request to {friendId} successfully sent!";
        TriggerNotification(requestSentNotification);

        friendSearchInputField.placeholder.enabled = true;
        friendSearchInputField.text = string.Empty;

        addFriendButton.gameObject.SetActive(false);

        OnFriendRequestSent?.Invoke(friendId);
    }

    public void DisplayFriendUserNotFound()
    {
        TriggerNotification(friendSearchFailedNotification);

        addFriendButton.interactable = false;
    }

    void OnSearchInputValueChanged(string friendId)
    {
        if (!addFriendButton.gameObject.activeSelf)
            addFriendButton.gameObject.SetActive(true);

        if (!addFriendButton.interactable)
            addFriendButton.interactable = true;

        DismissCurrentNotification();
    }

    void DismissCurrentNotification()
    {
        if (currentNotificationRoutine == null) return;

        StopCoroutine(currentNotificationRoutine);
        currentNotificationRoutine = null;

        currentNotification.SetActive(false);
        currentNotification = null;
    }

    void TriggerNotification(GameObject notificationGameobject)
    {
        DismissCurrentNotification();

        currentNotification = notificationGameobject;

        notificationGameobject.SetActive(true);
        currentNotificationRoutine = StartCoroutine(WaitAndCloseCurrentNotification(notificationGameobject));
    }

    IEnumerator WaitAndCloseCurrentNotification(GameObject notificationGameobject)
    {
        yield return WaitForSecondsCache.Get(notificationsDuration);

        currentNotificationRoutine = null;

        notificationGameobject.SetActive(false);
        currentNotification = null;
    }

    public bool CreateEntry(string userId)
    {
        if (friendRequestEntries.ContainsKey(userId)) return false;

        if (emptyListImage.activeSelf)
            emptyListImage.SetActive(false);

        FriendRequestEntry entry;

        entry = Instantiate(friendRequestEntryPrefab).GetComponent<FriendRequestEntry>();
        entry.OnAccepted += OnFriendRequestReceivedAccepted;
        entry.OnMenuToggle += (x) => { selectedRequestEntry = x; ToggleMenuPanel(x); };
        entry.OnRejected += OnFriendRequestReceivedRejected;
        entry.OnCancelled += OnFriendRequestSentCancelled;
        friendRequestEntries.Add(userId, entry);

        return true;
    }

    public bool UpdateEntry(string userId, FriendEntry.Model model, bool? isReceived = null)
    {
        if (!friendRequestEntries.ContainsKey(userId))
            return false;

        var entry = friendRequestEntries[userId];
        entry.Populate(userId, model, isReceived);

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

        ForceUpdateLayout();
        return true;
    }

    public void CreateOrUpdateEntry(string userId, FriendEntry.Model model, bool isReceived)
    {
        CreateEntry(userId);
        UpdateEntry(userId, model, isReceived);
    }

    void OnFriendRequestReceivedAccepted(FriendRequestEntry requestEntry)
    {
        // Add placeholder friend to avoid affecting UX by roundtrip with kernel
        FriendsController.i.UpdateUserStatus(new FriendsController.UserStatus() { userId = requestEntry.userId, presenceStatus = FriendsController.PresenceStatus.OFFLINE });

        acceptedFriendNotificationText.text = $"You and {requestEntry.model.userName} are now friends!";
        TriggerNotification(acceptedFriendNotification);

        RemoveEntry(requestEntry.userId);

        OnFriendRequestApproved?.Invoke(requestEntry);
    }

    void OnFriendRequestReceivedRejected(FriendRequestEntry requestEntry)
    {
        selectedRequestEntry = requestEntry;

        rejectRequestDialogText.text = $"Are you sure you want to reject {requestEntry.model.userName} friend request?";
        rejectRequestDialog.SetActive(true);
    }

    void ConfirmFriendRequestReceivedRejection()
    {
        if (selectedRequestEntry == null) return;

        rejectRequestDialog.SetActive(false);
        RemoveEntry(selectedRequestEntry.userId);
        OnFriendRequestRejected?.Invoke(selectedRequestEntry);
        selectedRequestEntry = null;
    }

    void OnFriendRequestSentCancelled(FriendRequestEntry requestEntry)
    {
        selectedRequestEntry = requestEntry;

        cancelRequestDialogText.text = $"Are you sure you want to cancel {requestEntry.model.userName} friend request?";
        cancelRequestDialog.SetActive(true);
    }

    void ConfirmFriendRequestSentCancellation()
    {
        if (selectedRequestEntry == null) return;

        cancelRequestDialog.SetActive(false);
        RemoveEntry(selectedRequestEntry.userId);
        OnFriendRequestCancelled?.Invoke(selectedRequestEntry);
        selectedRequestEntry = null;
    }

    void CancelConfirmationDialog()
    {
        selectedRequestEntry = null;
        cancelRequestDialog.SetActive(false);
        rejectRequestDialog.SetActive(false);
    }

    void ToggleMenuPanel(FriendRequestEntry entry)
    {
        requestMenuPanel.transform.position = entry.menuPositionReference.position;

        requestMenuPanel.SetActive(selectedRequestEntry == entry ? !requestMenuPanel.activeSelf : true);
    }

    public void RemoveEntry(string userId)
    {
        if (!friendRequestEntries.ContainsKey(userId)) return;

        var entry = friendRequestEntries[userId];

        if (entry.isReceived)
            receivedRequests--;
        else
            sentRequests--;
        UpdateUsersToggleTexts();

        Destroy(entry.gameObject);
        friendRequestEntries.Remove(userId);

        ForceUpdateLayout();
    }

    public void ForceUpdateLayout()
    {
        RectTransform containerRectTransform = transform as RectTransform;

        Utils.InverseTransformChildTraversal<RectTransform>(
        (x) =>
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(x);
        },
        containerRectTransform);

        LayoutRebuilder.ForceRebuildLayoutImmediate(containerRectTransform);
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
            name = "Pravus"
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
            name = "Brian"
        });

        FriendsController.i.UpdateFriendshipStatus(new FriendsController.FriendshipUpdateStatusMessage()
        {
            userId = id1,
            action = FriendsController.FriendshipAction.REQUESTED_TO
        });
    }
}
