using DCL;
using UnityEngine;

public class HideAvatarsModifier : AvatarModifier
{

    public override void ApplyModifier(GameObject avatar)
    {
        AvatarVisibility avatarVisibility = avatar.GetComponent<AvatarVisibility>();
        if (avatarVisibility != null)
        {
            avatarVisibility.SetVisibility(false);
            avatarVisibility.SetLock(true);
        }
    }

    public override void RemoveModifier(GameObject avatar)
    {
        AvatarVisibility avatarVisibility = avatar.GetComponent<AvatarVisibility>();
        if (avatarVisibility != null)
        {
            avatarVisibility.SetLock(false);
            avatarVisibility.SetVisibility(true);
        }
    }
}
