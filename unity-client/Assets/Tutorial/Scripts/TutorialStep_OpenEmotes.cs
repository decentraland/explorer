using System.Collections;
using UnityEngine;

namespace DCL.Tutorial
{
    /// <summary>
    /// Class that represents the onboarding tutorial step related to how to open the emotes menu.
    /// </summary>
    public class TutorialStep_OpenEmotes : TutorialStep_WithProgressBar
    {
        private bool emoteSelected = false;

        public override void OnStepStart()
        {
            base.OnStepStart();

            UserProfile.GetOwnUserProfile().OnAvatarExpressionSet += TutorialStep_OpenEmotes_OnAvatarExpressionSet;
        }

        public override IEnumerator OnStepExecute()
        {
            yield return new WaitUntil(() => emoteSelected);
        }

        public override void OnStepFinished()
        {
            base.OnStepFinished();

            UserProfile.GetOwnUserProfile().OnAvatarExpressionSet -= TutorialStep_OpenEmotes_OnAvatarExpressionSet;
        }

        private void TutorialStep_OpenEmotes_OnAvatarExpressionSet(string id, long timestamp)
        {
            emoteSelected = true;
        }
    }
}