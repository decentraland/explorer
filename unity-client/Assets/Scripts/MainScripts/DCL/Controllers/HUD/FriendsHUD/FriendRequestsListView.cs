using System.Collections.Generic;
using UnityEngine;

public class FriendRequestsListView : MonoBehaviour
{
    public GameObject friendRequestEntryPrefab;
    public Transform receivedRequestsContainer;
    public Transform sentRequestsContainer;

    FriendRequestEntry friendRequestEntry;
    Dictionary<string, FriendRequestEntry> friendEntries = new Dictionary<string, FriendRequestEntry>();

    public void UpdateOrCreateFriendRequestEntry(string userId, bool isReceived)
    {
        friendRequestEntry = friendEntries[userId];

        if (friendRequestEntry == null)
        {
            friendRequestEntry = Instantiate(friendRequestEntryPrefab).GetComponent<FriendRequestEntry>();

            // TODO
            // friendRequestEntry.Populate();
        }

        friendRequestEntry.transform.SetParent(isReceived ? receivedRequestsContainer : sentRequestsContainer);
    }
}
