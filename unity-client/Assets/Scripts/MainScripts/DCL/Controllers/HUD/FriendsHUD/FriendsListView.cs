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

    [SerializeField] GameObject friendMenuPanel;
    [SerializeField] Button friendPassportButton;
    [SerializeField] Button blockFriendButton;
    [SerializeField] Button reportFriendButton;
    [SerializeField] Button deleteFriendButton;

    [SerializeField] Button deleteFriendDialogCancelButton;
    [SerializeField] Button deleteFriendDialogConfirmButton;

    Dictionary<string, FriendEntry> friendEntries = new Dictionary<string, FriendEntry>();
    FriendEntry selectedFriendEntry;

    void Awake()
    {
        // friendPassportButton.onClick.AddListener();
        // blockFriendButton.onClick.AddListener();
        // reportFriendButton.onClick.AddListener();
        deleteFriendButton.onClick.AddListener(OnFriendDelete);

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

        friendEntry.OnMenuToggle += ToggleMenuPanel;
        friendEntry.OnFocus += OnEntryFocused;
        friendEntry.OnBlur += OnEntryBlurred;

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

    void OnEntryFocused(FriendEntry entry)
    {
        selectedFriendEntry = entry;
    }

    void OnEntryBlurred(FriendEntry entry)
    {
        selectedFriendEntry = null;

        friendMenuPanel.SetActive(false);
    }

    void OnFriendDelete()
    {
        if (selectedFriendEntry == null) return;

        deleteFriendDialogText.text = $"Are you sure you want to delete {selectedFriendEntry.model.userName} as a friend?";
        deleteFriendDialog.SetActive(true);
    }

    void ConfirmFriendRequestSentCancellation()
    {
        if (selectedFriendEntry == null) return;

        RemoveEntry(selectedFriendEntry.userId);

        deleteFriendDialog.SetActive(false);
        selectedFriendEntry = null;

        // TODO: Notify Kernel
    }

    void CancelConfirmationDialog()
    {
        selectedFriendEntry = null;
        deleteFriendDialog.SetActive(false);
    }

    void ToggleMenuPanel(FriendEntry entry)
    {
        // Reposition menu panel to be over the corresponding entry
        friendMenuPanel.transform.position = entry.transform.position;

        friendMenuPanel.SetActive(!friendMenuPanel.activeSelf);
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