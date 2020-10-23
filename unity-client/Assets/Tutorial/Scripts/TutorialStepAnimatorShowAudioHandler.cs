using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialStepAnimatorShowAudioHandler : StateMachineBehaviour
{
    private void Awake()
    {
        DestroyImmediate(this);
        return;
    }

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        AudioScriptableObjects.fadeIn.Play(true);
    }
}
