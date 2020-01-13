using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

[assembly: InternalsVisibleTo("UserProfileTests")]

[CreateAssetMenu(fileName = "UserProfile", menuName = "UserProfile")]
public class UserProfile : ScriptableObject //TODO Move to base variable
{
    public event Action<UserProfile> OnUpdate;

    public string userName => model.name;
    public string email => model.email;
    public AvatarModel avatar => model.avatar;
    internal Dictionary<string, int> inventory = new Dictionary<string, int>();

    public Sprite faceSnapshot { get; private set; }
    public Sprite bodySnapshot { get; private set; }

    internal UserProfileModel model = new UserProfileModel() //Empty initialization to avoid nullchecks
    {
        avatar = new AvatarModel()
    };

    public void UpdateData(UserProfileModel newModel, bool downloadAssets = true)
    {
        if (model?.snapshots?.face != null)
            ThumbnailsManager.CancelRequest(model.snapshots.face, OnFaceSnapshotReady);

        if (model?.snapshots?.body != null)
            ThumbnailsManager.CancelRequest(model.snapshots.body, OnBodySnapshotReady);

        model.name = newModel?.name;
        model.email = newModel?.email;
        model.avatar.CopyFrom(newModel?.avatar);
        model.snapshots = newModel?.snapshots;
        model.inventory = newModel?.inventory;
        inventory.Clear();
        if (model.inventory != null)
        {
            inventory = model.inventory.GroupBy(x => x).ToDictionary(x => x.Key, x => x.Count());
        }
        faceSnapshot = null;
        bodySnapshot = null;

        if (downloadAssets && model?.snapshots?.face != null)
            ThumbnailsManager.RequestThumbnail(model.snapshots.face, OnFaceSnapshotReady);

        if (downloadAssets && model?.snapshots?.body != null)
            ThumbnailsManager.RequestThumbnail(model.snapshots.body, OnBodySnapshotReady);

        OnUpdate?.Invoke(this);
    }

    public int GetItemAmount(string itemId)
    {
        if (inventory == null || !inventory.ContainsKey(itemId))
            return 0;

        return inventory[itemId];
    }

    private void OnFaceSnapshotReady(Sprite sprite)
    {
        faceSnapshot = sprite;
        OnUpdate?.Invoke(this);
    }

    private void OnBodySnapshotReady(Sprite sprite)
    {
        bodySnapshot = sprite;
        OnUpdate?.Invoke(this);
    }

    public void OverrideAvatar(AvatarModel newModel, Sprite faceSnapshot, Sprite bodySnapshot)
    {
        if (model?.snapshots?.face != null)
            ThumbnailsManager.CancelRequest(model.snapshots.face, OnFaceSnapshotReady);

        if (model?.snapshots?.body != null)
            ThumbnailsManager.CancelRequest(model.snapshots.body, OnBodySnapshotReady);

        model.avatar.CopyFrom(newModel);
        this.faceSnapshot = faceSnapshot;
        this.bodySnapshot = bodySnapshot;
        OnUpdate?.Invoke(this);
    }

    internal static UserProfile ownUserProfile;

    public static UserProfile GetOwnUserProfile()
    {
        if (ownUserProfile == null)
        {
            ownUserProfile = Resources.Load<UserProfile>("ScriptableObjects/OwnUserProfile");
        }

        return ownUserProfile;
    }

#if UNITY_EDITOR
    private void OnEnable()
    {
        Application.quitting -= CleanUp;
        Application.quitting += CleanUp;
    }

    private void CleanUp()
    {
        Application.quitting -= CleanUp;
        if (UnityEditor.AssetDatabase.Contains(this))
            Resources.UnloadAsset(this);
    }
#endif
}