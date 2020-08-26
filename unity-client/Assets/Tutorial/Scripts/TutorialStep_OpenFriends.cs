using System.Collections;
using UnityEngine;

namespace DCL.Tutorial
{
    /// <summary>
    /// Class that represents the onboarding tutorial step related to how to open the friends window.
    /// </summary>
    public class TutorialStep_OpenFriends : TutorialStep_WithProgressBar
    {
        private bool friendsHasBeenOpened = false;
        private bool friendsHasBeenClosed = false;

        public override void OnStepStart()
        {
            base.OnStepStart();

            if (TutorialController.i.hudController != null)
            {
                TutorialController.i.hudController.friendsHud.OnFriendsOpened += FriendsHud_OnFriendsOpened;
                TutorialController.i.hudController.friendsHud.OnFriendsClosed += FriendsHud_OnFriendsClosed;
            }
        }

        public override IEnumerator OnStepExecute()
        {
            yield return new WaitUntil(() => friendsHasBeenOpened && friendsHasBeenClosed);
        }

        public override void OnStepFinished()
        {
            base.OnStepFinished();

            if (TutorialController.i.hudController != null)
            {
                TutorialController.i.hudController.friendsHud.OnFriendsOpened -= FriendsHud_OnFriendsOpened;
                TutorialController.i.hudController.friendsHud.OnFriendsClosed -= FriendsHud_OnFriendsClosed;
            }
        }

        private void FriendsHud_OnFriendsOpened()
        {
            if (!friendsHasBeenOpened)
                friendsHasBeenOpened = true;
        }

        private void FriendsHud_OnFriendsClosed()
        {
            if (friendsHasBeenOpened)
                friendsHasBeenClosed = true;
        }
    }
}