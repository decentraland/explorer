using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialStepAnimatorHideAudioHandler : StateMachineBehaviour
{
    private void Awake()
    {
        DestroyImmediate(this);
        return;
    }

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        AudioScriptableObjects.fadeOut.Play(true);
    }
}
