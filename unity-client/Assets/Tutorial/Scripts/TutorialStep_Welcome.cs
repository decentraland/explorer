using System.Collections;
using TMPro;
using UnityEngine;

namespace DCL.Tutorial
{
    /// <summary>
    /// Class that represents the onboarding tutorial step related to how to show the welcome to the player.
    /// </summary>
    public class TutorialStep_Welcome : TutorialStep
    {
        [SerializeField] InputAction_Hold confirmInputAction;
        [SerializeField] InputAction_Hold cancelInputAction;
        [SerializeField] GameObject mainSectionTransform;
        [SerializeField] GameObject skipConfirmationSectionTransform;
        [SerializeField] TMP_Text descriptionText;

        private bool stepIsFinished = false;
        private bool skipConfirmationIsActive = false;

        public override void OnStepStart()
        {
            base.OnStepStart();

            descriptionText.text = descriptionText.text.Replace("{userName}", UserProfile.GetOwnUserProfile().userName);

            if (confirmInputAction != null)
                confirmInputAction.OnFinished += ConfirmInputAction_OnFinished;

            if (cancelInputAction != null)
                cancelInputAction.OnFinished += CancelInputAction_OnFinished;

            ShowMainSection();
        }

        public override IEnumerator OnStepExecute()
        {
            yield return new WaitUntil(() => stepIsFinished);
        }

        public override void OnStepFinished()
        {
            base.OnStepFinished();

            confirmInputAction.OnFinished -= ConfirmInputAction_OnFinished;
            cancelInputAction.OnFinished -= CancelInputAction_OnFinished;
        }

        private void ConfirmInputAction_OnFinished(DCLAction_Hold action)
        {
            if (!skipConfirmationIsActive)
                stepIsFinished = true;
            else
                TutorialController.i?.SkipAllSteps();
        }

        private void CancelInputAction_OnFinished(DCLAction_Hold action)
        {
            if (!skipConfirmationIsActive)
                ShowSkipConfirmationSection();
            else
                ShowMainSection();
        }

        private void ShowMainSection()
        {
            mainSectionTransform.SetActive(true);
            skipConfirmationSectionTransform.SetActive(false);
            skipConfirmationIsActive = false;
        }

        private void ShowSkipConfirmationSection()
        {
            mainSectionTransform.SetActive(false);
            skipConfirmationSectionTransform.SetActive(true);
            skipConfirmationIsActive = true;
        }
    }
}