using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class FriendRequestsListView : MonoBehaviour
{
    public float notificationsDuration = 3f;

    [SerializeField] GameObject friendRequestEntryPrefab;
    [SerializeField] Transform receivedRequestsContainer;
    [SerializeField] Transform sentRequestsContainer;

    [SerializeField] TMP_InputField friendSearchInputField;

    [SerializeField] GameObject requestSentNotification;
    [SerializeField] TextMeshProUGUI requestSentNotificationText;
    [SerializeField] GameObject friendSearchFailedNotification;
    [SerializeField] GameObject acceptedFriendNotification;
    [SerializeField] TextMeshProUGUI acceptedFriendNotificationText;

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

    void Awake()
    {
        friendSearchInputField.onSubmit.AddListener(SendFriendRequest);
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
    }

    public void UpdateOrCreateFriendRequestEntry(string userId, bool isReceived)
    {
        friendRequestEntry = friendRequestEntries[userId];

        if (friendRequestEntry == null)
        {
            friendRequestEntry = Instantiate(friendRequestEntryPrefab).GetComponent<FriendRequestEntry>();
            friendRequestEntry.OnRemoved += OnFriendRequestRemoved;
            friendRequestEntry.OnAccepted += OnFriendRequestReceivedAccepted;

            // TODO
            // friendRequestEntry.Populate();
        }

        friendRequestEntry.transform.SetParent(isReceived ? receivedRequestsContainer : sentRequestsContainer);
    }

    void OnFriendRequestRemoved(string playerId)
    {
        friendRequestEntries.Remove(playerId);
    }

    void OnFriendRequestReceivedAccepted(string friendId)
    {
        acceptedFriendNotificationText.text = $"You and {friendId} are now friends!";
        TriggerNotification(acceptedFriendNotification);
    }
}
