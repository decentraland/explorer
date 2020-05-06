using DCL.Configuration;
using DCL.Helpers;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FriendsTabViewBase : MonoBehaviour, IPointerDownHandler
{
    const string BLOCK_BTN_BLOCK_TEXT = "Block";
    const string BLOCK_BTN_UNBLOCK_TEXT = "Unblock";

    public GameObject entryPrefab;
    public GameObject emptyListImage;

    protected RectTransform rectTransform;

    protected FriendsHUDView owner;

    [Header("Context Menu References")]
    public GameObject contextMenuPanel;
    public Button contextMenuPassportButton;
    public Button contextMenuBlockButton;
    public TextMeshProUGUI contextMenuBlockButtonText;
    public Button contextMenuReportButton;
    public Button contextMenuDeleteButton;

    [Header("Confirmation Dialog References")]
    public GameObject confirmationDialog;
    public TextMeshProUGUI confirmationDialogText;
    public Button confirmationDialogCancelButton;
    public Button confirmationDialogConfirmButton;

    protected Dictionary<string, FriendEntryBase> entries = new Dictionary<string, FriendEntryBase>();
    protected internal FriendEntryBase selectedEntry;

    public System.Action<string> OnPassport;
    public System.Action<string> OnReport;
    public System.Action<string> OnBlock;
    public System.Action<FriendEntryBase> OnDelete;

    public int entriesCount => entries.Count;

    internal List<FriendEntryBase> GetAllEntries()
    {
        return entries.Values.ToList();
    }

    internal FriendEntryBase GetEntry(string userId)
    {
        if (!entries.ContainsKey(userId))
            return null;

        return entries[userId];
    }

    protected virtual void OnEnable()
    {
        rectTransform.ForceUpdateLayout();
    }

    protected virtual void OnDisable()
    {
        CloseDialog();
        contextMenuPanel.SetActive(false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.pointerPressRaycast.gameObject == null || eventData.pointerPressRaycast.gameObject.layer != PhysicsLayers.friendsHUDPlayerMenu)
            contextMenuPanel.SetActive(false);
    }

    public virtual void Initialize(FriendsHUDView owner)
    {
        this.owner = owner;

        rectTransform = transform as RectTransform;

        contextMenuPassportButton.onClick.AddListener(OnPassportButtonPressed);
        contextMenuReportButton.onClick.AddListener(OnReportUserButtonPressed);
        contextMenuDeleteButton.onClick.AddListener(OnDeleteUserButtonPressed);
        contextMenuBlockButton.onClick.AddListener(OnBlockUserButtonPressed);

        confirmationDialogCancelButton.onClick.AddListener(CloseDialog);
    }

    public virtual void CreateOrUpdateEntry(string userId, FriendEntryBase.Model model)
    {
        bool firstUpdate = CreateEntry(userId);
        UpdateEntry(userId, model, firstUpdate);
    }

    public virtual bool CreateEntry(string userId)
    {
        if (entries.ContainsKey(userId)) return false;

        if (emptyListImage.activeSelf)
            emptyListImage.SetActive(false);

        var entry = Instantiate(entryPrefab).GetComponent<FriendEntryBase>();
        entries.Add(userId, entry);

        entry.OnMenuToggle += (x) => { selectedEntry = x; ToggleMenuPanel(x); };

        return true;
    }

    public virtual bool UpdateEntry(string userId, FriendEntryBase.Model model, bool firstUpdate = false)
    {
        if (!entries.ContainsKey(userId)) return false;

        var entry = entries[userId];

        entry.Populate(model);
        entry.userId = userId;

        rectTransform.ForceUpdateLayout();

        return true;
    }

    public virtual void RemoveEntry(string userId)
    {
        if (!entries.ContainsKey(userId)) return;

        var entry = entries[userId];

        Object.Destroy(entry.gameObject);
        entries.Remove(userId);

        if (entries.Count == 0)
            emptyListImage.SetActive(true);

        rectTransform.ForceUpdateLayout();
    }

    protected void TriggerDialog(string text, UnityEngine.Events.UnityAction ConfirmCallback)
    {
        confirmationDialogText.text = text;

        confirmationDialogConfirmButton.onClick.RemoveAllListeners();
        confirmationDialogConfirmButton.onClick.AddListener(() => { ConfirmCallback(); CloseDialog(); });
    }

    protected virtual void OnPassportButtonPressed()
    {
        OnPassport?.Invoke(selectedEntry.userId);

        ToggleMenuPanel(selectedEntry);
    }

    protected virtual void OnReportUserButtonPressed()
    {
        OnReport?.Invoke(selectedEntry.userId);

        ToggleMenuPanel(selectedEntry);
    }

    protected virtual void OnDeleteUserButtonPressed()
    {
        ToggleMenuPanel(selectedEntry);
    }

    protected virtual void OnBlockUserButtonPressed()
    {
        OnBlock?.Invoke(selectedEntry.userId);
        selectedEntry.model.blocked = !selectedEntry.model.blocked;
        selectedEntry.Populate(selectedEntry.model);

        ToggleMenuPanel(selectedEntry);
    }

    protected virtual void CloseDialog()
    {
        confirmationDialog.SetActive(false);
        selectedEntry = null;
    }

    protected void ToggleMenuPanel(FriendEntryBase entry)
    {
        contextMenuPanel.transform.position = entry.menuPositionReference.position;

        contextMenuPanel.SetActive(selectedEntry == entry ? !contextMenuPanel.activeSelf : true);

        if (contextMenuPanel.activeSelf)
            contextMenuBlockButtonText.text = entry.model.blocked ? BLOCK_BTN_UNBLOCK_TEXT : BLOCK_BTN_BLOCK_TEXT;
    }
}
