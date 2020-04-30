using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
        friendEntry.transform.localScale = Vector3.one;

        LayoutRebuilder.ForceRebuildLayoutImmediate(friendEntry.transform.parent as RectTransform);
    }

    public void RemoveFriend(string userId)
    {
        if (!friendEntries.ContainsKey(userId))
            return;

        RectTransform containerRectTransform = friendEntries[userId].transform.parent as RectTransform;

        Object.Destroy(friendEntries[userId].gameObject);
        friendEntries.Remove(userId);

        LayoutRebuilder.ForceRebuildLayoutImmediate(containerRectTransform);
    }

    [ContextMenu("AddFakeOnlineFriend")]
    public void AddFakeOnlineFriend()
    {
        var model1 = new FriendEntry.Model()
        {
            status = FriendEntry.Model.Status.ONLINE,
            userName = "Pravus",
        };

        string id1 = Random.Range(0, 1000000).ToString();

        UpdateOrCreateFriendEntry(id1, model1);
    }

    [ContextMenu("AddFakeOfflineFriend")]
    public void AddFakeOfflineFriend()
    {
        var model1 = new FriendEntry.Model()
        {
            status = FriendEntry.Model.Status.OFFLINE,
            userName = "Brian",
        };

        string id1 = Random.Range(0, 1000000).ToString();

        UpdateOrCreateFriendEntry(id1, model1);
    }
}
