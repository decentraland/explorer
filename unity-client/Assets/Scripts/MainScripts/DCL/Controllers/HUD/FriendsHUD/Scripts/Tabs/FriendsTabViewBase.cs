using DCL.Helpers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FriendsTabViewBase : MonoBehaviour
{
    [SerializeField] protected GameObject entryPrefab;
    [SerializeField] protected GameObject emptyListImage;

    protected RectTransform rectTransform;
    protected FriendsHUDView owner;

    public FriendsHUD_ContextMenu contextMenuPanel;
    public FriendsHUD_DialogBox confirmationDialog;

    protected Dictionary<string, FriendEntryBase> entries = new Dictionary<string, FriendEntryBase>();

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
        confirmationDialog.Hide();
        contextMenuPanel.Hide();
    }

    public virtual void Initialize(FriendsHUDView owner)
    {
        this.owner = owner;

        rectTransform = transform as RectTransform;

        contextMenuPanel.OnBlock += OnPressBlockButton;
        contextMenuPanel.OnDelete += OnPressDeleteButton;
        contextMenuPanel.OnPassport += OnPressPassportButton;
        contextMenuPanel.OnReport += OnPressReportButton;
    }

    protected virtual void OnPressReportButton(FriendEntryBase obj)
    {
    }

    protected virtual void OnPressPassportButton(FriendEntryBase obj)
    {
    }

    protected virtual void OnPressDeleteButton(FriendEntryBase obj)
    {
    }

    protected virtual void OnPressBlockButton(FriendEntryBase entry)
    {
        entry.model.blocked = !entry.model.blocked;
        entry.Populate(entry.model);
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

        entry.OnMenuToggle += (x) => { contextMenuPanel.Toggle(entry); };

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

        UnityEngine.Object.Destroy(entry.gameObject);
        entries.Remove(userId);

        if (entries.Count == 0)
            emptyListImage.SetActive(true);

        rectTransform.ForceUpdateLayout();
    }
}
