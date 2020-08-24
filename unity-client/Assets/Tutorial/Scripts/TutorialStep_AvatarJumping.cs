using System.Collections;
using UnityEngine;

namespace DCL.Tutorial
{
    /// <summary>
    /// Class that represents the onboarding tutorial step related to how to jump with the avatar.
    /// </summary>
    public class TutorialStep_AvatarJumping : TutorialStep_WithProgressBar
    {
        [SerializeField] InputAction_Hold jumpingInputAction;

        public override IEnumerator OnStepExecute()
        {
            yield return new WaitUntil(() => jumpingInputAction.isOn);
        }
    }
}