using DCL.Helpers;
using TMPro;
using UnityEngine;

public class FriendsListView : FriendsHUDListViewBase
{
    [SerializeField] TextMeshProUGUI onlineFriendsToggleText;
    [SerializeField] TextMeshProUGUI offlineFriendsToggleText;

    public Transform onlineFriendsContainer;
    public Transform offlineFriendsContainer;
    internal int onlineFriends = 0;
    internal int offlineFriends = 0;

    public event System.Action<FriendEntry> OnJumpIn;
    public event System.Action<FriendEntry> OnWhisper;

    public override bool CreateEntry(string userId)
    {
        if (entries.ContainsKey(userId)) return false;

        if (emptyListImage.activeSelf)
            emptyListImage.SetActive(false);

        if (!onlineFriendsToggleText.transform.parent.gameObject.activeSelf)
        {
            onlineFriendsToggleText.transform.parent.gameObject.SetActive(true);
            offlineFriendsToggleText.transform.parent.gameObject.SetActive(true);
        }

        var entry = Instantiate(entryPrefab).GetComponent<FriendEntry>();
        entries.Add(userId, entry);

        entry.OnMenuToggle += (x) => { selectedEntry = x; ToggleMenuPanel(x); };
        entry.OnJumpInClick += (x) => OnJumpIn?.Invoke(x);
        entry.OnWhisperClick += (x) => OnWhisper?.Invoke(x);

        return true;
    }

    public override bool UpdateEntry(string userId, FriendsHUDListEntry.Model model, bool firstUpdate = false)
    {
        if (!entries.ContainsKey(userId)) return false;

        FriendEntry entry = entries[userId] as FriendEntry;
        var previousStatus = entry.model.status;

        entry.Populate(userId, model);

        if (entry.model.status == FriendsController.PresenceStatus.ONLINE)
        {
            entry.transform.SetParent(onlineFriendsContainer);
            onlineFriends++;
        }
        else
        {
            entry.transform.SetParent(offlineFriendsContainer);
            offlineFriends++;
        }

        entry.transform.localScale = Vector3.one;

        if (!firstUpdate)
        {
            if (previousStatus == FriendsController.PresenceStatus.ONLINE)
                onlineFriends--;
            else
                offlineFriends--;
        }

        UpdateUsersToggleTexts();

        entry.ToggleBlockedImage(ownUserProfile.blocked.Contains(userId));

        (transform as RectTransform).ForceUpdateLayout();

        return true;
    }

    public override void RemoveEntry(string userId)
    {
        if (!entries.ContainsKey(userId)) return;

        var entry = entries[userId];

        if (entry.model.status == FriendsController.PresenceStatus.ONLINE)
            onlineFriends--;
        else
            offlineFriends--;

        UpdateUsersToggleTexts();

        Object.Destroy(entry.gameObject);
        entries.Remove(userId);

        if (entries.Count == 0)
        {
            emptyListImage.SetActive(true);
            onlineFriendsToggleText.transform.parent.gameObject.SetActive(false);
            offlineFriendsToggleText.transform.parent.gameObject.SetActive(false);
        }

        (transform as RectTransform).ForceUpdateLayout();
    }

    void UpdateUsersToggleTexts()
    {
        onlineFriendsToggleText.text = $"ONLINE ({onlineFriends})";
        offlineFriendsToggleText.text = $"OFFLINE ({offlineFriends})";
    }

    protected override void OnDeleteUserButtonPressed()
    {
        base.OnDeleteUserButtonPressed();

        if (selectedEntry == null) return;

        TriggerDialog($"Are you sure you want to delete {selectedEntry.model.userName} as a friend?", ConfirmFriendDelete);
    }

    void ConfirmFriendDelete()
    {
        if (selectedEntry == null) return;

        RemoveEntry(selectedEntry.userId);

        OnDelete?.Invoke(selectedEntry);
    }

    [ContextMenu("AddFakeOnlineFriend")]
    public void AddFakeOnlineFriend()
    {
        string id1 = Random.Range(0, 1000000).ToString();

        UserProfileController.i.AddUserProfileToCatalog(new UserProfileModel()
        {
            userId = id1,
            name = "Brian-" + id1
        });

        FriendsController.i.UpdateFriendshipStatus(new FriendsController.FriendshipUpdateStatusMessage()
        {
            userId = id1,
            action = FriendsController.FriendshipAction.APPROVED
        });

        FriendsController.i.UpdateUserStatus(new FriendsController.UserStatus() { userId = id1, presence = FriendsController.PresenceStatus.ONLINE });
    }

    [ContextMenu("AddFakeOfflineFriend")]
    public void AddFakeOfflineFriend()
    {
        string id1 = Random.Range(0, 1000000).ToString();

        UserProfileController.i.AddUserProfileToCatalog(new UserProfileModel()
        {
            userId = id1,
            name = "Pravus-" + id1
        });

        FriendsController.i.UpdateFriendshipStatus(new FriendsController.FriendshipUpdateStatusMessage()
        {
            userId = id1,
            action = FriendsController.FriendshipAction.APPROVED
        });

        FriendsController.i.UpdateUserStatus(new FriendsController.UserStatus() { userId = id1, presence = FriendsController.PresenceStatus.OFFLINE });
    }
}
