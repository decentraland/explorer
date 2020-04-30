using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FriendsListView : MonoBehaviour
{
    public GameObject friendEntryPrefab;
    public Transform onlineFriendsContainer;
    public Transform offlineFriendsContainer;

    Dictionary<string, FriendEntry> friendEntries = new Dictionary<string, FriendEntry>();

    public event System.Action<FriendEntry> OnJumpInClick;
    public event System.Action<FriendEntry> OnWhisperClick;


    internal FriendEntry GetEntry(string userId)
    {
        return friendEntries[userId];
    }

    public bool UpdateEntry(string userId, FriendEntry.Model model)
    {
        if (!friendEntries.ContainsKey(userId))
            return false;

        var friendEntry = friendEntries[userId];

        friendEntry.Populate(model);
        friendEntry.transform.SetParent(model.status == FriendsController.PresenceStatus.ONLINE ? onlineFriendsContainer : offlineFriendsContainer);
        friendEntry.transform.localScale = Vector3.one;

        LayoutRebuilder.ForceRebuildLayoutImmediate(friendEntry.transform.parent as RectTransform);
        return true;
    }

    public bool CreateEntry(string userId)
    {
        if (friendEntries.ContainsKey(userId))
            return false;

        var entry = Instantiate(friendEntryPrefab).GetComponent<FriendEntry>();
        friendEntries.Add(userId, entry);
        return true;
    }

    public void CreateOrUpdateEntry(string userId, FriendEntry.Model model)
    {
        CreateEntry(userId);
        UpdateEntry(userId, model);
    }

    public void RemoveEntry(string userId)
    {
        if (!friendEntries.ContainsKey(userId))
            return;

        var entry = friendEntries[userId];

        RectTransform containerRectTransform = entry.transform.parent as RectTransform;

        Object.Destroy(entry.gameObject);
        friendEntries.Remove(userId);

        LayoutRebuilder.ForceRebuildLayoutImmediate(containerRectTransform);
    }

    [ContextMenu("AddFakeOnlineFriend")]
    public void AddFakeOnlineFriend()
    {
        var model1 = new FriendEntry.Model()
        {
            status = FriendsController.PresenceStatus.ONLINE,
            userName = "Pravus",
        };

        string id1 = Random.Range(0, 1000000).ToString();

        CreateOrUpdateEntry(id1, model1);
    }

    [ContextMenu("AddFakeOfflineFriend")]
    public void AddFakeOfflineFriend()
    {
        var model1 = new FriendEntry.Model()
        {
            status = FriendsController.PresenceStatus.OFFLINE,
            userName = "Brian",
        };

        string id1 = Random.Range(0, 1000000).ToString();

        CreateOrUpdateEntry(id1, model1);
    }
}
