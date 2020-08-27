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

            if (tutorialController != null && tutorialController.hudController != null)
            {
                tutorialController.hudController.friendsHud.OnFriendsOpened += FriendsHud_OnFriendsOpened;
                tutorialController.hudController.friendsHud.OnFriendsClosed += FriendsHud_OnFriendsClosed;
            }
        }

        public override IEnumerator OnStepExecute()
        {
            yield return new WaitUntil(() => friendsHasBeenOpened && friendsHasBeenClosed);

            tutorialController.PlayTeacherAnimation(TutorialTeacher.TeacherAnimation.Goodbye);
        }

        public override void OnStepFinished()
        {
            base.OnStepFinished();

            if (tutorialController != null && tutorialController.hudController != null)
            {
                tutorialController.hudController.friendsHud.OnFriendsOpened -= FriendsHud_OnFriendsOpened;
                tutorialController.hudController.friendsHud.OnFriendsClosed -= FriendsHud_OnFriendsClosed;
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