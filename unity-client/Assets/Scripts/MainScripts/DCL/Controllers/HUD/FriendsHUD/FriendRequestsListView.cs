using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class FriendRequestsListView : MonoBehaviour
{
    [SerializeField] GameObject friendRequestEntryPrefab;
    [SerializeField] Transform receivedRequestsContainer;
    [SerializeField] Transform sentRequestsContainer;

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

    public void UpdateOrCreateFriendRequestEntry(string userId, bool isReceived)
    {
        friendRequestEntry = friendRequestEntries[userId];

        if (friendRequestEntry == null)
        {
            friendRequestEntry = Instantiate(friendRequestEntryPrefab).GetComponent<FriendRequestEntry>();
            friendRequestEntry.OnRemoved += OnFriendRequestRemoved;

            // TODO
            // friendRequestEntry.Populate();
        }

        friendRequestEntry.transform.SetParent(isReceived ? receivedRequestsContainer : sentRequestsContainer);
    }

    void OnFriendRequestRemoved(string playerId)
    {
        friendRequestEntries.Remove(playerId);
    }
}
