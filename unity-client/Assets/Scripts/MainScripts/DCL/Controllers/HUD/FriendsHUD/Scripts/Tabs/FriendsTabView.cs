public class FriendsTabView : FriendsTabViewBase
{
    public EntryList onlineFriendsList = new EntryList();
    public EntryList offlineFriendsList = new EntryList();

    public event System.Action<FriendEntry> OnJumpIn;
    public event System.Action<FriendEntry> OnWhisper;
    public event System.Action<FriendEntry> OnDeleteConfirmation;

    public override void Initialize(FriendsHUDView owner)
    {
        base.Initialize(owner);

        onlineFriendsList.toggleTextPrefix = "ONLINE";
        offlineFriendsList.toggleTextPrefix = "OFFLINE";
    }

    public override bool CreateEntry(string userId)
    {
        if (!base.CreateEntry(userId)) return false;

        var entry = GetEntry(userId) as FriendEntry;

        entry.OnJumpInClick += (x) => OnJumpIn?.Invoke(x);
        entry.OnWhisperClick += (x) => OnWhisper?.Invoke(x);

        return true;
    }

    public override bool RemoveEntry(string userId)
    {
        if (!base.RemoveEntry(userId))
            return false;

        offlineFriendsList.Remove(userId);
        onlineFriendsList.Remove(userId);
        return true;
    }

    public override bool UpdateEntry(string userId, FriendEntryBase.Model model)
    {
        if (!base.UpdateEntry(userId, model))
            return false;

        var entry = entries[userId];

        if (model.status == PresenceStatus.ONLINE)
        {
            offlineFriendsList.Remove(userId);
            onlineFriendsList.Add(userId, entry);
        }

        if (model.status == PresenceStatus.OFFLINE)
        {
            onlineFriendsList.Remove(userId);
            offlineFriendsList.Add(userId, entry);
        }

        return true;
    }

    protected override void OnPressDeleteButton(FriendEntryBase entry)
    {
        if (entry == null) return;

        confirmationDialog.SetText($"Are you sure you want to delete {entry.model.userName} as a friend?");
        confirmationDialog.Show(() =>
        {
            RemoveEntry(entry.userId);
            OnDeleteConfirmation?.Invoke(entry as FriendEntry);
        });
    }
}
