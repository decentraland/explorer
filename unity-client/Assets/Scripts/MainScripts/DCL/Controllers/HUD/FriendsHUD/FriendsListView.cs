using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    public event System.Action<FriendEntry> OnJumpIn;
    public event System.Action<FriendEntry> OnWhisper;
    public event System.Action<FriendEntry> OnBlock;
    public event System.Action<FriendEntry> OnPassport;
    public event System.Action<FriendEntry> OnDelete;
    public event System.Action<FriendEntry> OnReport;

    void Awake()
    {
        friendPassportButton.onClick.AddListener(() => { OnPassport?.Invoke(selectedFriendEntry); ToggleMenuPanel(selectedFriendEntry); });
        blockFriendButton.onClick.AddListener(() => { OnBlock?.Invoke(selectedFriendEntry); ToggleMenuPanel(selectedFriendEntry); });
        reportFriendButton.onClick.AddListener(() => { OnReport?.Invoke(selectedFriendEntry); ToggleMenuPanel(selectedFriendEntry); });
        deleteFriendButton.onClick.AddListener(() => { ToggleMenuPanel(selectedFriendEntry); OnFriendDelete(); });

        deleteFriendDialogConfirmButton.onClick.AddListener(ConfirmFriendDelete);
        deleteFriendDialogCancelButton.onClick.AddListener(CancelConfirmationDialog);
    }

    void OnDisable()
    {
        CancelConfirmationDialog();
        friendMenuPanel.SetActive(false);
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

        var entry = Instantiate(friendEntryPrefab).GetComponent<FriendEntry>();
        friendEntries.Add(userId, entry);

        entry.OnMenuToggle += (x) => { selectedFriendEntry = x; ToggleMenuPanel(x); };

        entry.OnJumpInClick += (x) => OnJumpIn?.Invoke(x);
        entry.OnWhisperClick += (x) => OnWhisper?.Invoke(x);

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

    void OnFriendDelete()
    {
        if (selectedFriendEntry == null) return;

        deleteFriendDialogText.text = $"Are you sure you want to delete {selectedFriendEntry.model.userName} as a friend?";
        deleteFriendDialog.SetActive(true);
    }

    void ConfirmFriendDelete()
    {
        if (selectedFriendEntry == null) return;

        RemoveEntry(selectedFriendEntry.userId);

        OnDelete?.Invoke(selectedFriendEntry);

        deleteFriendDialog.SetActive(false);
        selectedFriendEntry = null;
    }

    void CancelConfirmationDialog()
    {
        selectedFriendEntry = null;
        deleteFriendDialog.SetActive(false);
    }

    void ToggleMenuPanel(FriendEntry entry)
    {
        // Reposition menu panel to be over the corresponding entry
        // friendMenuPanel.transform.position = entry.transform.position;
        friendMenuPanel.transform.position = entry.menuPositionReference.position;

        friendMenuPanel.SetActive(selectedFriendEntry == entry ? !friendMenuPanel.activeSelf : true);
    }

    [ContextMenu("AddFakeOnlineFriend")]
    public void AddFakeOnlineFriend()
    {
        string id1 = Random.Range(0, 1000000).ToString();
        var model1 = new FriendEntry.Model()
        {
            status = FriendsController.PresenceStatus.ONLINE,
            userName = id1,
        };

        CreateOrUpdateEntry(id1, model1);
    }

    [ContextMenu("AddFakeOfflineFriend")]
    public void AddFakeOfflineFriend()
    {
        string id1 = Random.Range(0, 1000000).ToString();

        var model1 = new FriendEntry.Model()
        {
            status = FriendsController.PresenceStatus.OFFLINE,
            userName = id1,
        };

        CreateOrUpdateEntry(id1, model1);
    }
}
