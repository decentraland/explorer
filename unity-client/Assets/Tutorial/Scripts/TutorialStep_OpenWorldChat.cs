using System.Collections;
using UnityEngine;

namespace DCL.Tutorial
{
    /// <summary>
    /// Class that represents the onboarding tutorial step related to how to open the world chat.
    /// </summary>
    public class TutorialStep_OpenWorldChat : TutorialStep_WithProgressBar
    {
        [SerializeField] InputAction_Trigger toggleWorldChatInputAction;

        private bool worldChatIsOpen = false;

        public override void OnStepStart()
        {
            base.OnStepStart();

            toggleWorldChatInputAction.OnTriggered += ToggleWorldChatInputAction_OnTriggered;
        }

        public override IEnumerator OnStepExecute()
        {
            yield return new WaitUntil(() => worldChatIsOpen);
        }

        public override void OnStepFinished()
        {
            base.OnStepFinished();

            toggleWorldChatInputAction.OnTriggered -= ToggleWorldChatInputAction_OnTriggered;
        }

        private void ToggleWorldChatInputAction_OnTriggered(DCLAction_Trigger action)
        {
            worldChatIsOpen = true;
        }
    }
}