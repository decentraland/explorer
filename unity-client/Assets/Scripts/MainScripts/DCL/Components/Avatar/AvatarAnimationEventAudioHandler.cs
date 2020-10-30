using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarAnimationEventAudioHandler : MonoBehaviour
{
    AudioEvent footstepWalkEvent;

    public void Init(AudioContainer audioContainer)
    {
        if (audioContainer == null)
            return;

        footstepWalkEvent = audioContainer.GetEvent("FootstepWalk");
    }

    public void AnimEvent_FootstepWalk()
    {
        Debug.Log("AnimEvent_FootstepWalk");

        if (footstepWalkEvent != null)
            footstepWalkEvent.Play(true);
    }

    public void AnimEvent_FootstepRun()
    {
        Debug.Log("AnimEvent_FootstepRun");
    }
}
