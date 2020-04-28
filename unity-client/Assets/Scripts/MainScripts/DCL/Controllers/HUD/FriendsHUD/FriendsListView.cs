using System.Collections.Generic;
using UnityEngine;

public class FriendsListView : MonoBehaviour
{
    public GameObject friendEntryPrefab;
    public Transform onlineFriendsContainer;
    public Transform offlineFriendsContainer;

    FriendEntry friendEntry;
    Dictionary<string, FriendEntry> friendEntries = new Dictionary<string, FriendEntry>();

    public void UpdateOrCreateFriendEntry(string userId, bool isOnline)
    {
        friendEntry = friendEntries[userId];

        if (friendEntry == null)
        {
            friendEntry = Instantiate(friendEntryPrefab).GetComponent<FriendEntry>();

            // TODO
            // friendEntry.Populate();
        }

        friendEntry.transform.SetParent(isOnline ? onlineFriendsContainer : offlineFriendsContainer);
    }
}
