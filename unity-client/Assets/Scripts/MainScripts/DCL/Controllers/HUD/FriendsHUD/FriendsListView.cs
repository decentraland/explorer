using System.Collections.Generic;
using UnityEngine;

public class FriendsListView : MonoBehaviour
{
    public GameObject friendEntryPrefab;
    public Transform onlineFriendsContainer;
    public Transform offlineFriendsContainer;

    Dictionary<string, FriendEntry> friendEntries = new Dictionary<string, FriendEntry>();

    internal FriendEntry GetEntry(string userId)
    {
        return friendEntries[userId];
    }

    public void UpdateOrCreateFriendEntry(string userId, FriendEntry.Model model)
    {
        FriendEntry friendEntry;

        if (!friendEntries.ContainsKey(userId))
        {
            friendEntry = Instantiate(friendEntryPrefab).GetComponent<FriendEntry>();
            friendEntries.Add(userId, friendEntry);
        }
        else
        {
            friendEntry = friendEntries[userId];
        }

        friendEntry.Populate(model);
        friendEntry.transform.SetParent(model.status == FriendEntry.Model.Status.ONLINE ? onlineFriendsContainer : offlineFriendsContainer);
    }

    public void RemoveFriend(string userId)
    {
        if (!friendEntries.ContainsKey(userId))
            return;

        Object.Destroy(friendEntries[userId].gameObject);
        friendEntries.Remove(userId);
    }
}
