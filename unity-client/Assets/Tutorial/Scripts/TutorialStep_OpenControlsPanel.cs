using System.Collections;
using UnityEngine;

namespace DCL.Tutorial
{
    /// <summary>
    /// Class that represents the onboarding tutorial step related to how to open the controls panel.
    /// </summary>
    public class TutorialStep_OpenControlsPanel : TutorialStep_WithProgressBar
    {
        private bool controlsHasBeenOpened = false;
        private bool controlsHasBeenClosed = false;

        public override void OnStepStart()
        {
            base.OnStepStart();

            if (TutorialController.i.hudController != null)
            {
                TutorialController.i.hudController.controlsHud.OnControlsOpened += ControlsHud_OnControlsOpened;
                TutorialController.i.hudController.controlsHud.OnControlsClosed += ControlsHud_OnControlsClosed;
            }

            TutorialController.i?.SetTimeBetweenSteps(0.5f);
        }

        public override IEnumerator OnStepExecute()
        {
            yield return new WaitUntil(() => controlsHasBeenOpened && controlsHasBeenClosed);
        }

        public override void OnStepFinished()
        {
            base.OnStepFinished();

            if (TutorialController.i.hudController != null)
            {
                TutorialController.i.hudController.controlsHud.OnControlsOpened -= ControlsHud_OnControlsOpened;
                TutorialController.i.hudController.controlsHud.OnControlsClosed -= ControlsHud_OnControlsClosed;
            }
        }

        private void ControlsHud_OnControlsOpened()
        {
            if (!controlsHasBeenOpened)
                controlsHasBeenOpened = true;
        }

        private void ControlsHud_OnControlsClosed()
        {
            if (controlsHasBeenOpened)
                controlsHasBeenClosed = true;
        }
    }
}