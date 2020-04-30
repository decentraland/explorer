using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FriendsListView : MonoBehaviour
{
    [SerializeField] GameObject friendEntryPrefab;
    public Transform onlineFriendsContainer;
    public Transform offlineFriendsContainer;
    [SerializeField] GameObject deleteFriendDialog;
    [SerializeField] TextMeshProUGUI deleteFriendDialogText;
    [SerializeField] Button deleteFriendDialogCancelButton;
    [SerializeField] Button deleteFriendDialogConfirmButton;

    Dictionary<string, FriendEntry> friendEntries = new Dictionary<string, FriendEntry>();
    FriendEntry currentDialogFriendEntry;

    void Awake()
    {
        deleteFriendDialogConfirmButton.onClick.AddListener(ConfirmFriendRequestSentCancellation);
        deleteFriendDialogCancelButton.onClick.AddListener(CancelConfirmationDialog);
    }

    void OnDisable()
    {
        CancelConfirmationDialog();
    }

    internal FriendEntry GetEntry(string userId)
    {
        return friendEntries[userId];
    }

    public bool UpdateEntry(string userId, FriendEntry.Model model)
    {
        if (!friendEntries.ContainsKey(userId))
            return false;

        var friendEntry = friendEntries[userId];

        friendEntry.Populate(userId, model);
        friendEntry.transform.SetParent(model.status == FriendsController.PresenceStatus.ONLINE ? onlineFriendsContainer : offlineFriendsContainer);
        friendEntry.transform.localScale = Vector3.one;

        LayoutRebuilder.ForceRebuildLayoutImmediate(friendEntry.transform.parent as RectTransform);
        return true;
    }

    public bool CreateEntry(string userId)
    {
        if (friendEntries.ContainsKey(userId))
            return false;

        var friendEntry = Instantiate(friendEntryPrefab).GetComponent<FriendEntry>();
        friendEntries.Add(userId, friendEntry);

        friendEntry.OnDelete += OnFriendDelete;
        // friendEntry.OnBlock += ;
        // friendEntry.OnJumpIn += ;
        // friendEntry.OnPassport += ;
        // friendEntry.OnReport += ;
        // friendEntry.OnWhisper += ;

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

        RectTransform containerRectTransform = friendEntries[userId].transform.parent as RectTransform;

        Object.Destroy(friendEntries[userId].gameObject);
        friendEntries.Remove(userId);

        LayoutRebuilder.ForceRebuildLayoutImmediate(containerRectTransform);
    }

    void ConfirmFriendRequestSentCancellation()
    {
        if (currentDialogFriendEntry == null) return;

        RemoveEntry(currentDialogFriendEntry.userId);

        deleteFriendDialog.SetActive(false);
        currentDialogFriendEntry = null;

        // TODO: Notify Kernel
    }

    void CancelConfirmationDialog()
    {
        currentDialogFriendEntry = null;
        deleteFriendDialog.SetActive(false);
    }

    void OnFriendDelete(FriendEntry entry)
    {
        currentDialogFriendEntry = entry;

        deleteFriendDialogText.text = $"Are you sure you want to delete {entry.model.userName} as a friend?";
        deleteFriendDialog.SetActive(true);
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