using System.Collections;
using UnityEngine;

namespace DCL.Tutorial
{
    public class TutorialStep_AvatarJumping : TutorialStep_WithProgressBar
    {
        [SerializeField] InputAction_Hold jumpingInputAction;

        public override IEnumerator OnStepExecute()
        {
            yield return new WaitUntil(() => jumpingInputAction.isOn);
        }
    }
}