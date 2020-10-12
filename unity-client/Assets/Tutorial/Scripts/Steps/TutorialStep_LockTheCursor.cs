using System.Collections;
using UnityEngine;

namespace DCL.Tutorial
{
    /// <summary>
    /// Class that represents the onboarding tutorial step related to how to lock/unlock the cursor.
    /// </summary>
    public class TutorialStep_LockTheCursor : TutorialStep
    {
        public override void OnStepStart()
        {
            base.OnStepStart();

            tutorialController?.hudController?.taskbarHud?.SetVisibility(false);
        }

        public override IEnumerator OnStepExecute()
        {
            yield return new WaitUntil(() => mouseCatcher.isLocked);
        }

        public override IEnumerator OnStepPlayAnimationForHidding()
        {
            if (tutorialController != null)
                tutorialController.SetEagleEyeCameraActive(false);

            yield return base.OnStepPlayAnimationForHidding();
        }
    }
}