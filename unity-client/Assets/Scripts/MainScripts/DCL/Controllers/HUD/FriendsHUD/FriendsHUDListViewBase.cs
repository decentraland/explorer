using DCL.Configuration;
using DCL.Helpers;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FriendsHUDListViewBase : MonoBehaviour, IPointerDownHandler
{
    const string BLOCK_BTN_BLOCK_TEXT = "Block";
    const string BLOCK_BTN_UNBLOCK_TEXT = "Unblock";

    public float notificationsDuration = 3f;
    public GameObject entryPrefab;
    public GameObject emptyListImage;

    [Header("Context Menu References")]
    public RectTransform contextMenuPanel;
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

    protected Dictionary<string, FriendsHUDListEntry> entries = new Dictionary<string, FriendsHUDListEntry>();
    protected internal FriendsHUDListEntry selectedEntry;
    protected UserProfile ownUserProfile => UserProfile.GetOwnUserProfile();

    public System.Action<string> OnPassport;
    public System.Action<string> OnReport;
    public System.Action<string> OnBlock;
    public System.Action<FriendsHUDListEntry> OnDelete;

    public int entriesCount => entries.Count;
    internal FriendsHUDListEntry GetEntry(string userId)
    {
        if (!entries.ContainsKey(userId))
            return null;

        return entries[userId];
    }

    protected virtual void OnEnable()
    {
        (transform as RectTransform).ForceUpdateLayout();
    }

    protected virtual void OnDisable()
    {
        CloseDialog();
        contextMenuPanel.gameObject.SetActive(false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.pointerPressRaycast.gameObject == null || eventData.pointerPressRaycast.gameObject.layer != PhysicsLayers.friendsHUDPlayerMenu)
            contextMenuPanel.gameObject.SetActive(false);
    }

    public virtual void Initialize()
    {
        contextMenuPassportButton.onClick.AddListener(OnPassportButtonPressed);
        contextMenuReportButton.onClick.AddListener(OnReportUserButtonPressed);
        contextMenuDeleteButton.onClick.AddListener(OnDeleteUserButtonPressed);
        contextMenuBlockButton.onClick.AddListener(OnBlockUserButtonPressed);

        confirmationDialogCancelButton.onClick.AddListener(CloseDialog);
    }

    public virtual void CreateOrUpdateEntry(string userId, FriendsHUDListEntry.Model model)
    {
        bool firstUpdate = CreateEntry(userId);
        UpdateEntry(userId, model, firstUpdate);
    }

    public virtual bool CreateEntry(string userId)
    {
        if (entries.ContainsKey(userId)) return false;

        if (emptyListImage.activeSelf)
            emptyListImage.SetActive(false);

        var entry = Instantiate(entryPrefab).GetComponent<FriendsHUDListEntry>();
        entries.Add(userId, entry);

        entry.OnMenuToggle += (x) => { selectedEntry = x; ToggleMenuPanel(x); };

        return true;
    }

    public virtual bool UpdateEntry(string userId, FriendsHUDListEntry.Model model, bool firstUpdate = false)
    {
        if (!entries.ContainsKey(userId)) return false;

        var entry = entries[userId];
        var previousStatus = entry.model.status;

        entry.Populate(userId, model);

        entry.ToggleBlockedImage(ownUserProfile.blocked.Contains(userId));

        (transform as RectTransform).ForceUpdateLayout();

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

        (transform as RectTransform).ForceUpdateLayout();
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

        selectedEntry.ToggleBlockedImage(!selectedEntry.playerBlockedImage.enabled);

        ToggleMenuPanel(selectedEntry);
    }

    protected virtual void CloseDialog()
    {
        confirmationDialog.SetActive(false);
        selectedEntry = null;
    }

    protected void ToggleMenuPanel(FriendsHUDListEntry entry)
    {
        // By setting the pivot accordingly BEFORE we position the menu, we can have it always visible in an easier way
        if (entry.transform.parent.InverseTransformPoint(entry.menuPositionReference.position).y < 0f)
            contextMenuPanel.pivot = new Vector2(0.5f, 0f);
        else
            contextMenuPanel.pivot = new Vector2(0.5f, 1f);

        contextMenuPanel.transform.position = entry.menuPositionReference.position;

        contextMenuPanel.gameObject.SetActive(selectedEntry == entry ? !contextMenuPanel.gameObject.activeSelf : true);

        if (contextMenuPanel.gameObject.activeSelf)
            contextMenuBlockButtonText.text = ownUserProfile.blocked.Contains(entry.userId) ? BLOCK_BTN_UNBLOCK_TEXT : BLOCK_BTN_BLOCK_TEXT;
    }
}
