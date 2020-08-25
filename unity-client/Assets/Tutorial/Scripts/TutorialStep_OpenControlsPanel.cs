using System.Collections;
using UnityEngine;

namespace DCL.Tutorial
{
    /// <summary>
    /// Class that represents the onboarding tutorial step related to how to open the controls panel.
    /// </summary>
    public class TutorialStep_OpenControlsPanel : TutorialStep_WithProgressBar
    {
        [SerializeField] InputAction_Trigger toggleControlsPanelInputAction;

        private bool controlsPanelIsOpen = false;

        public override void OnStepStart()
        {
            base.OnStepStart();

            toggleControlsPanelInputAction.OnTriggered += ToggleControlsPanelInputAction_OnTriggered;

            TutorialController.i?.SetTimeBetweenSteps(0.5f);
        }

        public override IEnumerator OnStepExecute()
        {
            yield return new WaitUntil(() => controlsPanelIsOpen);
        }

        public override void OnStepFinished()
        {
            base.OnStepFinished();

            toggleControlsPanelInputAction.OnTriggered += ToggleControlsPanelInputAction_OnTriggered;
        }

        private void ToggleControlsPanelInputAction_OnTriggered(DCLAction_Trigger action)
        {
            controlsPanelIsOpen = true;
        }
    }
}