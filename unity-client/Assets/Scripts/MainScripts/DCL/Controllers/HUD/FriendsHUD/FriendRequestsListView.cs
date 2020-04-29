using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using UnityEngine;
using DCL.Interface;

public class FriendRequestsListView : MonoBehaviour
{
    public float notificationsDuration = 3f;

    [SerializeField] GameObject friendRequestEntryPrefab;
    [SerializeField] Transform receivedRequestsContainer;
    [SerializeField] Transform sentRequestsContainer;

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

    FriendRequestEntry friendRequestEntry;
    Dictionary<string, FriendRequestEntry> friendRequestEntries = new Dictionary<string, FriendRequestEntry>();
    IEnumerator currentNotificationRoutine = null;
    GameObject currentNotification = null;
    FriendRequestEntry currentDialogRequestEntry = null;

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

        notificationGameobject.SetActive(false);
        StartCoroutine(WaitAndCloseNotification(notificationGameobject));
    }

    IEnumerator WaitAndCloseNotification(GameObject notificationGameobject)
    {
        currentNotification = notificationGameobject;

        yield return WaitForSecondsCache.Get(notificationsDuration);
    }

    public void UpdateOrCreateFriendRequestEntry(string userId, bool isReceived)
    {
        friendRequestEntry = friendRequestEntries[userId];

        if (friendRequestEntry == null)
        {
            friendRequestEntry = Instantiate(friendRequestEntryPrefab).GetComponent<FriendRequestEntry>();
            friendRequestEntry.OnAccepted += OnFriendRequestReceivedAccepted;
            friendRequestEntry.OnRejected += OnFriendRequestReceivedRejected;
            friendRequestEntry.OnCancelled += OnFriendRequestSentCancelled;

            // TODO
            // friendRequestEntry.Populate();
        }

        friendRequestEntry.transform.SetParent(isReceived ? receivedRequestsContainer : sentRequestsContainer);
    }

    void OnFriendRequestReceivedAccepted(FriendRequestEntry requestEntry)
    {
        // TODO: Notify Kernel & Add to friends list

        acceptedFriendNotificationText.text = $"You and {requestEntry.friendId} are now friends!";
        TriggerNotification(acceptedFriendNotification);

        RemoveRequestEntry(requestEntry);
    }

    void OnFriendRequestReceivedRejected(FriendRequestEntry requestEntry)
    {
        currentDialogRequestEntry = requestEntry;

        rejectRequestDialogText.text = $"Are you sure you want to reject {requestEntry.friendId} friend request?";
        rejectRequestDialog.SetActive(true);
    }

    void ConfirmFriendRequestReceivedRejection()
    {
        if (currentDialogRequestEntry == null) return;

        rejectRequestDialog.SetActive(false);

        RemoveRequestEntry(currentDialogRequestEntry);
        currentDialogRequestEntry = null;

        // TODO: Notify Kernel
    }

    void OnFriendRequestSentCancelled(FriendRequestEntry requestEntry)
    {
        cancelRequestDialogText.text = $"Are you sure you want to cancel {requestEntry.friendId} friend request?";
        cancelRequestDialog.SetActive(true);
    }

    void ConfirmFriendRequestSentCancellation()
    {
        if (currentDialogRequestEntry == null) return;

        cancelRequestDialog.SetActive(false);

        RemoveRequestEntry(currentDialogRequestEntry);
        currentDialogRequestEntry = null;

        // TODO: Notify Kernel
    }

    void CancelConfirmationDialog()
    {
        currentDialogRequestEntry = null;
        cancelRequestDialog.SetActive(false);
        rejectRequestDialog.SetActive(false);
    }

    void RemoveRequestEntry(FriendRequestEntry requestEntry)
    {
        friendRequestEntries.Remove(requestEntry.friendId);
        Destroy(requestEntry.gameObject);
    }
}
