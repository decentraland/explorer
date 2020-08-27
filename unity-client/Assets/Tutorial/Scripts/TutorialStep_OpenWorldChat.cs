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

        private bool worldChatHasBeenOpened = false;
        private bool worldChatHasBeenClosed = false;

        public override void OnStepStart()
        {
            base.OnStepStart();

            if (tutorialController != null && tutorialController.hudController != null)
            {
                tutorialController.hudController.worldChatWindowHud.view.OnDeactivatePreview += View_OnDeactivatePreview;
                tutorialController.hudController.worldChatWindowHud.view.OnActivatePreview += View_OnActivatePreview;
            }
        }

        public override IEnumerator OnStepExecute()
        {
            yield return new WaitUntil(() => worldChatHasBeenOpened && worldChatHasBeenClosed);

            tutorialController.PlayTeacherAnimation(TutorialTeacher.TeacherAnimation.Goodbye);
        }

        public override void OnStepFinished()
        {
            base.OnStepFinished();

            if (tutorialController != null && tutorialController.hudController != null)
            {
                tutorialController.hudController.worldChatWindowHud.view.OnDeactivatePreview -= View_OnDeactivatePreview;
                tutorialController.hudController.worldChatWindowHud.view.OnActivatePreview -= View_OnActivatePreview;
            }
        }

        private void View_OnDeactivatePreview()
        {
            if (!worldChatHasBeenOpened)
                worldChatHasBeenOpened = true;
        }

        private void View_OnActivatePreview()
        {
            if (worldChatHasBeenOpened)
                worldChatHasBeenClosed = true;
        }
    }
}