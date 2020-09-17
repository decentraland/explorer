using DCL;
using UnityEngine;

public class HideAvatarsModifier : AvatarModifier
{
    private const string HIDE_AVATARS_MODIFIER = "HIDE_AVATARS_MODIFIER";

    public override void ApplyModifier(AvatarShape avatarShape)
    {
        AvatarVisibility avatarVisibility = avatarShape.GetComponent<AvatarVisibility>();

        if (avatarVisibility == null) return;

        avatarVisibility.SetVisibility(HIDE_AVATARS_MODIFIER, false);
    }

    public override void RemoveModifier(AvatarShape avatarShape)
    {
        AvatarVisibility avatarVisibility = avatarShape.GetComponent<AvatarVisibility>();

        if (avatarVisibility == null) return;
        
        avatarVisibility.SetVisibility(HIDE_AVATARS_MODIFIER, true);
    }
}
