using System.Collections.Generic;
using UnityEngine;

public class FriendRequestsListView : MonoBehaviour
{
    public GameObject friendRequestEntryPrefab;
    public Transform receivedRequestsContainer;
    public Transform sentRequestsContainer;

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
