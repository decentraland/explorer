using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarAnimationEventAudioHandler : MonoBehaviour
{
    AudioEvent footstepWalk, footstepRun, clothesRustleShort;

    public void Init(AudioContainer audioContainer)
    {
        if (audioContainer == null)
            return;

        footstepWalk = audioContainer.GetEvent("FootstepWalk");
        footstepRun = audioContainer.GetEvent("FootstepRun");
        clothesRustleShort = audioContainer.GetEvent("ClothesRustleShort");
    }

    public void AnimEvent_FootstepWalk()
    {
        TryPlayingEvent(footstepWalk);
    }

    public void AnimEvent_FootstepRun()
    {
        TryPlayingEvent(footstepRun);
    }

    public void AnimEvent_ClothesRustleShort()
    {
        TryPlayingEvent(clothesRustleShort);
    }

    void TryPlayingEvent(AudioEvent audioEvent)
    {
        if (audioEvent != null)
            audioEvent.Play(true);
    }
}
