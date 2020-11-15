using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarAnimationEventAudioHandler : MonoBehaviour
{
    AudioEvent footstepWalkEvent, footstepRunEvent;

    public void Init(AudioContainer audioContainer)
    {
        if (audioContainer == null)
            return;

        footstepWalkEvent = audioContainer.GetEvent("FootstepWalk");
        footstepRunEvent = audioContainer.GetEvent("FootstepRun");
    }

    public void AnimEvent_FootstepWalk()
    {
        TryPlayingEvent(footstepWalkEvent);
    }

    public void AnimEvent_FootstepRun()
    {
        Debug.Log("FootstepRun");
        TryPlayingEvent(footstepRunEvent);
    }

    void TryPlayingEvent(AudioEvent audioEvent)
    {
        if (audioEvent != null)
            audioEvent.Play(true);
    }
}
