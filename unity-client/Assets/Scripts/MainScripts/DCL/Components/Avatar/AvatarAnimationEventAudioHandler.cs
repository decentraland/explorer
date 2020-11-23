using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarAnimationEventAudioHandler : MonoBehaviour
{
    AudioEvent footstepLight, footstepSlide, footstepWalk, footstepRun, clothesRustleShort, clap, throwMoney;

    public void Init(AudioContainer audioContainer)
    {
        if (audioContainer == null)
            return;

        footstepLight = audioContainer.GetEvent("FootstepLight");
        footstepSlide = audioContainer.GetEvent("FootstepSlide");
        footstepWalk = audioContainer.GetEvent("FootstepWalk");
        footstepRun = audioContainer.GetEvent("FootstepRun");
        clothesRustleShort = audioContainer.GetEvent("ClothesRustleShort");
        clap = audioContainer.GetEvent("ExpressionClap");
        throwMoney = audioContainer.GetEvent("ExpressionThrowMoney");
    }

    public void AnimEvent_FootstepLight()
    {
        TryPlayingEvent(footstepLight);
    }

    public void AnimEvent_FootstepSlide()
    {
        TryPlayingEvent(footstepSlide);
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
        Debug.Log("Rustle!");
        TryPlayingEvent(clothesRustleShort);
    }

    public void AnimEvent_Clap()
    {
        TryPlayingEvent(clap);
    }

    public void AnimEvent_ThrowMoney()
    {
        TryPlayingEvent(throwMoney);
    }

    void TryPlayingEvent(AudioEvent audioEvent)
    {
        if (audioEvent != null)
            audioEvent.Play(true);
    }
}
