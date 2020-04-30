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
    IEnumerator currentNotificationRoutine = null;
    GameObject currentNotification = null;
    FriendRequestEntry currentDialogRequestEntry = null;

    internal FriendRequestEntry GetEntry(string userId)
    {
        return friendRequestEntries[userId];
    }

    void Awake()
    {
        friendSearchInputField.onSubmit.AddListener(SendFriendRequest);

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
    }

    void SendFriendRequest(string friendId)
    {
        // TODO: Check existence with kernel, if the user exists trigger requestSentNotification

        requestSentNotificationText.text = $"Your request to {friendId} successfully sent!";
        TriggerNotification(requestSentNotification);

        // If friend Id doesn't exist:
        // TriggerNotification(friendSearchFailedNotification);
    }

    void TriggerNotification(GameObject notificationGameobject)
    {
        if (currentNotificationRoutine != null)
        {
            StopCoroutine(currentNotificationRoutine);
            currentNotification.SetActive(false);
            currentNotification = null;
        }

        notificationGameobject.SetActive(true);
        StartCoroutine(WaitAndCloseNotification(notificationGameobject));
    }

    IEnumerator WaitAndCloseNotification(GameObject notificationGameobject)
    {
        currentNotification = notificationGameobject;

        yield return WaitForSecondsCache.Get(notificationsDuration);
        notificationGameobject.SetActive(false);
    }

    public void UpdateOrCreateFriendRequestEntry(string userId, FriendEntry.Model model, bool isReceived)
    {
        FriendRequestEntry entry;

        if (!friendRequestEntries.ContainsKey(userId))
        {
            entry = Instantiate(friendRequestEntryPrefab).GetComponent<FriendRequestEntry>();
            entry.OnAccepted += (x) => { OnFriendRequestReceivedAccepted(x); RemoveRequestEntry(userId); };
            entry.OnRejected += OnFriendRequestReceivedRejected;
            entry.OnCancelled += OnFriendRequestSentCancelled;
            friendRequestEntries.Add(userId, entry);
        }
        else
        {
            entry = friendRequestEntries[userId];
        }

        entry.Populate(userId, model, isReceived);
        entry.transform.SetParent(isReceived ? receivedRequestsContainer : sentRequestsContainer);
        entry.transform.localScale = Vector3.one;

        LayoutRebuilder.ForceRebuildLayoutImmediate(entry.transform.parent as RectTransform);
    }

    void OnFriendRequestReceivedAccepted(FriendRequestEntry requestEntry)
    {
        // TODO: Notify Kernel & Add to friends list

        acceptedFriendNotificationText.text = $"You and {requestEntry.model.userName} are now friends!";
        TriggerNotification(acceptedFriendNotification);
    }

    void OnFriendRequestReceivedRejected(FriendRequestEntry requestEntry)
    {
        currentDialogRequestEntry = requestEntry;

        rejectRequestDialogText.text = $"Are you sure you want to reject {requestEntry.model.userName} friend request?";
        rejectRequestDialog.SetActive(true);
    }

    void ConfirmFriendRequestReceivedRejection()
    {
        if (currentDialogRequestEntry == null) return;

        RemoveRequestEntry(currentDialogRequestEntry.userId);

        rejectRequestDialog.SetActive(false);
        currentDialogRequestEntry = null;

        // TODO: Notify Kernel
    }

    void OnFriendRequestSentCancelled(FriendRequestEntry requestEntry)
    {
        currentDialogRequestEntry = requestEntry;

        cancelRequestDialogText.text = $"Are you sure you want to cancel {requestEntry.model.userName} friend request?";
        cancelRequestDialog.SetActive(true);
    }

    void ConfirmFriendRequestSentCancellation()
    {
        if (currentDialogRequestEntry == null) return;

        RemoveRequestEntry(currentDialogRequestEntry.userId);

        cancelRequestDialog.SetActive(false);
        currentDialogRequestEntry = null;

        // TODO: Notify Kernel
    }

    void CancelConfirmationDialog()
    {
        currentDialogRequestEntry = null;
        cancelRequestDialog.SetActive(false);
        rejectRequestDialog.SetActive(false);
    }

    public void RemoveRequestEntry(string userId)
    {
        if (!friendRequestEntries.ContainsKey(userId))
            return;

        RectTransform containerRectTransform = friendRequestEntries[userId].transform.parent as RectTransform;

        Destroy(friendRequestEntries[userId].gameObject);
        friendRequestEntries.Remove(userId);

        LayoutRebuilder.ForceRebuildLayoutImmediate(containerRectTransform);
    }

    [ContextMenu("AddFakeRequestReceived")]
    public void AddFakeRequestReceived()
    {
        var model1 = new FriendEntry.Model()
        {
            status = FriendEntry.Model.Status.ONLINE,
            userName = "Pravus",
        };

        string id1 = Random.Range(0, 1000000).ToString();

        UpdateOrCreateFriendRequestEntry(id1, model1, true);
    }

    [ContextMenu("AddFakeRequestSent")]
    public void AddFakeRequestSent()
    {
        var model1 = new FriendEntry.Model()
        {
            status = FriendEntry.Model.Status.ONLINE,
            userName = "Brian",
        };

        string id1 = Random.Range(0, 1000000).ToString();

        UpdateOrCreateFriendRequestEntry(id1, model1, false);
    }
}
