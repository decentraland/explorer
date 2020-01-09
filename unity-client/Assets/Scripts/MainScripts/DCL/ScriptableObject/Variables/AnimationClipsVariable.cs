using UnityEngine;

[CreateAssetMenu(fileName = "AnimationClipsVariable", menuName = "AnimationClipsVariable")]
public class AnimationClipsVariable : BaseVariable<AnimationClip[]>
{
    public override bool Equals(AnimationClip[] other)
    {
        if (value.Length != other.Length) return false;

        for (var i = 0; i < value.Length; i++)
        {
            if (value[i] != other[i]) return false;
        }

        return true;
    }
}