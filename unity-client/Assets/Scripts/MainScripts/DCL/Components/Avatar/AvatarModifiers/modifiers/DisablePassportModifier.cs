using DCL;
using UnityEngine;

public class DisablePassportModifier : AvatarModifier
{

    public override void ApplyModifier(AvatarShape avatarShape)
    {
        if (avatarShape == null) return;

        avatarShape.DisablePassport();
    }

    public override void RemoveModifier(AvatarShape avatarShape)
    {
        if (avatarShape == null) return;

        avatarShape.EnablePassport();
    }
}
