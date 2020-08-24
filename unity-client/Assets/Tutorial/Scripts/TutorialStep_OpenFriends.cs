using System.Collections;
using UnityEngine;

namespace DCL.Tutorial
{
    /// <summary>
    /// Class that represents the onboarding tutorial step related to how to open the friends window.
    /// </summary>
    public class TutorialStep_OpenFriends : TutorialStep_WithProgressBar
    {
        [SerializeField] InputAction_Trigger toggleFriendsInputAction;

        private bool friendsWindowIsOpen = false;

        public override void OnStepStart()
        {
            base.OnStepStart();

            toggleFriendsInputAction.OnTriggered += ToggleFriendsInputAction_OnTriggered;
        }

        public override IEnumerator OnStepExecute()
        {
            yield return new WaitUntil(() => friendsWindowIsOpen);
        }

        public override void OnStepFinished()
        {
            base.OnStepFinished();

            toggleFriendsInputAction.OnTriggered += ToggleFriendsInputAction_OnTriggered;
        }

        private void ToggleFriendsInputAction_OnTriggered(DCLAction_Trigger action)
        {
            friendsWindowIsOpen = true;
        }
    }
}