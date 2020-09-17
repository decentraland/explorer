using DCL;
using UnityEngine;

public abstract class AvatarModifier
{

    public abstract void ApplyModifier(AvatarShape avatar);
    public abstract void RemoveModifier(AvatarShape avatar);

}
